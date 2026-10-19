using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using libCanopenSimple;

namespace ConformancePlugin
{
    public sealed class CanopenTransport : IHeartbeatTransport
    {
        private readonly libCanopenSimple.libCanopenSimple can;
        private readonly byte node;
        private readonly object sync = new object();
        private readonly BusObservation observation = new BusObservation();
        private long stateSerial;
        private long commandStateSerial;
        private List<HeartbeatSample> heartbeatCapture;
        private bool captureOverflow;

        public CanopenTransport(libCanopenSimple.libCanopenSimple can, byte node)
        {
            if (can == null || !can.isopen())
            {
                throw new InvalidOperationException("Connect CanMonitor to the CAN bus first.");
            }
            this.can = can;
            this.node = node;
            can.packetevent += Observe;
        }

        private void Observe(canpacket packet, DateTime timestamp)
        {
            lock (sync)
            {
                observation.Frames++;
                if (packet.data == null || packet.len > 8 || packet.len != packet.data.Length)
                {
                    observation.InvalidFrames++;
                    return;
                }
                if (packet.cob == 0x700 + node)
                {
                    if (heartbeatCapture != null)
                    {
                        if (heartbeatCapture.Count < 10000)
                        {
                            heartbeatCapture.Add(new HeartbeatSample
                            {
                                Milliseconds = Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency,
                                Length = packet.len,
                                State = packet.len > 0 ? packet.data[0] : (byte)0
                            });
                        }
                        else
                        {
                            captureOverflow = true;
                        }
                    }
                    if (packet.len == 1 && packet.data[0] == 0)
                    {
                        // Boot-up shares the error-control COB-ID but is not evidence
                        // that periodic heartbeat production is working.
                        observation.LastState = 0;
                        return;
                    }
                    observation.Heartbeats++;
                    if (packet.len != 1 || (packet.data[0] != 0 && packet.data[0] != 4 && packet.data[0] != 5 && packet.data[0] != 127))
                    {
                        observation.InvalidHeartbeats++;
                    }
                    else
                    {
                        observation.LastState = packet.data[0];
                        stateSerial++;
                    }
                }
                if (packet.cob == 0x80 + node)
                {
                    observation.Emergencies++;
                    if (packet.len != 8)
                    {
                        observation.InvalidEmergencies++;
                    }
                }
            }
        }

        public UploadResult Upload(byte target, ushort index, byte subindex, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (target != node || !can.isopen())
            {
                throw new InvalidOperationException("Connection closed or target node changed.");
            }
            SDO transfer = can.SDOread(target, index, subindex, null);
            Stopwatch elapsed = Stopwatch.StartNew();
            try
            {
                while (!transfer.WaitOne(25))
                {
                    cancellation.ThrowIfCancellationRequested();
                    if (!can.isopen() || elapsed.ElapsedMilliseconds > 4000)
                    {
                        transfer.SDOFinish();
                        return new UploadResult { TimedOut = true };
                    }
                }
                cancellation.ThrowIfCancellationRequested();
                if (transfer.state != SDO.SDO_STATE.SDO_FINISHED)
                {
                    return new UploadResult { Abort = transfer.expitideddata, TimedOut = transfer.expitideddata == 0x05040000 };
                }
                byte[] data = transfer.databuffer ?? new byte[0];
                return new UploadResult { Data = transfer.expitided ? data.Take(transfer.returnlen / 8).ToArray() : (byte[])data.Clone() };
            }
            catch (OperationCanceledException)
            {
                // Cancel only our transfer, never another plugin's SDO queue.
                transfer.SDOFinish();
                throw;
            }
        }

        public BusObservation Snapshot()
        {
            lock (sync)
            {
                return new BusObservation
                {
                    Frames = observation.Frames,
                    InvalidFrames = observation.InvalidFrames,
                    Heartbeats = observation.Heartbeats,
                    InvalidHeartbeats = observation.InvalidHeartbeats,
                    LastState = observation.LastState,
                    Emergencies = observation.Emergencies,
                    InvalidEmergencies = observation.InvalidEmergencies
                };
            }
        }

        public void SetProducerHeartbeat(byte target, ushort milliseconds, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (target != node || !can.isopen())
            {
                throw new InvalidOperationException("Connection closed or target node changed.");
            }
            SDO transfer = can.SDOwrite(target, 0x1017, 0, milliseconds, null);
            Stopwatch elapsed = Stopwatch.StartNew();
            try
            {
                while (!transfer.WaitOne(25))
                {
                    cancellation.ThrowIfCancellationRequested();
                    if (!can.isopen() || elapsed.ElapsedMilliseconds > 4000)
                    {
                        transfer.SDOFinish();
                        throw new TimeoutException("Producer heartbeat download timed out.");
                    }
                }
                cancellation.ThrowIfCancellationRequested();
                if (transfer.state != SDO.SDO_STATE.SDO_FINISHED)
                {
                    throw new InvalidOperationException("Producer heartbeat SDO abort 0x" + transfer.expitideddata.ToString("X8"));
                }
            }
            catch (OperationCanceledException)
            {
                transfer.SDOFinish();
                throw;
            }
        }

        public HeartbeatWindow ObserveHeartbeats(int milliseconds, CancellationToken cancellation)
        {
            lock (sync)
            {
                heartbeatCapture = new List<HeartbeatSample>();
                captureOverflow = false;
            }
            try
            {
                if (cancellation.WaitHandle.WaitOne(milliseconds))
                {
                    cancellation.ThrowIfCancellationRequested();
                }
                lock (sync)
                {
                    return new HeartbeatWindow { Samples = heartbeatCapture.ToArray(), Overflow = captureOverflow };
                }
            }
            finally
            {
                lock (sync)
                {
                    heartbeatCapture = null;
                }
            }
        }

        public void SetNmt(byte target, byte command)
        {
            if (target != node || target == 0 || !can.isopen())
            {
                throw new InvalidOperationException("NMT testing requires the connected target node, never broadcast.");
            }
            lock (sync)
            {
                commandStateSerial = stateSerial;
            }
            switch (command)
            {
                case 1: can.NMT_start(target); break;
                case 2: can.NMT_stop(target); break;
                case 128: can.NMT_preop(target); break;
                default: throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        public bool WaitForState(byte state, int milliseconds, CancellationToken cancellation)
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < milliseconds)
            {
                cancellation.ThrowIfCancellationRequested();
                lock (sync)
                {
                    if (stateSerial > commandStateSerial && observation.LastState == state)
                    {
                        return true;
                    }
                }
                if (!can.isopen())
                {
                    return false;
                }
                cancellation.WaitHandle.WaitOne(20);
            }
            return false;
        }

        public void Dispose()
        {
            can.packetevent -= Observe;
        }
    }
}
