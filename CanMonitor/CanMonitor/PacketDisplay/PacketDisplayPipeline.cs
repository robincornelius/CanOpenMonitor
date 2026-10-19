using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace CanMonitor.PacketDisplay
{
    internal sealed class PacketDisplayPipeline : IDisposable
    {
        private sealed class Session
        {
            internal ConcurrentQueue<PacketLogRecord> Input = new ConcurrentQueue<PacketLogRecord>();
            internal ConcurrentQueue<PacketLogRow> Output = new ConcurrentQueue<PacketLogRow>();
            internal long PendingInput;
            internal long PendingOutput;
        }

        private readonly Func<PacketLogRecord, PacketLogRow> format;
        private readonly AutoResetEvent wake = new AutoResetEvent(false);
        private readonly Thread worker;
        private Session session = new Session();
        private PacketDisplayFilter filter = new PacketDisplayFilter();
        private int disposed;
        private long received;
        private long processed;
        private long filtered;
        private long errors;
        private Exception lastError;

        internal PacketDisplayPipeline(Func<PacketLogRecord, PacketLogRow> format)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            this.format = format;
            worker = new Thread(Run) { IsBackground = true, Name = "CAN display formatting" };
            worker.Start();
        }

        internal PacketDisplayFilter Filter
        {
            set
            {
                Volatile.Write(ref filter, value);
            }
        }

        internal long PendingInput
        {
            get
            {
                return Interlocked.Read(ref Volatile.Read(ref session).PendingInput);
            }
        }
        internal long PendingOutput
        {
            get
            {
                return Interlocked.Read(ref Volatile.Read(ref session).PendingOutput);
            }
        }
        internal long Received
        {
            get
            {
                return Interlocked.Read(ref received);
            }
        }
        internal long Processed
        {
            get
            {
                return Interlocked.Read(ref processed);
            }
        }
        internal long Filtered
        {
            get
            {
                return Interlocked.Read(ref filtered);
            }
        }
        internal long Errors
        {
            get
            {
                return Interlocked.Read(ref errors);
            }
        }
        internal Exception LastError
        {
            get
            {
                return Volatile.Read(ref lastError);
            }
        }

        internal void Enqueue(PacketLogRecord record)
        {
            if (Volatile.Read(ref disposed) != 0)
            {
                return;
            }

            Session target = Volatile.Read(ref session);
            Interlocked.Increment(ref received);
            long count = Interlocked.Increment(ref target.PendingInput);
            target.Input.Enqueue(record);
            if (count == 1)
            {
                Wake();
            }
        }

        internal bool TryTake(out PacketLogRow row)
        {
            Session target = Volatile.Read(ref session);
            if (target.Output.TryDequeue(out row))
            {
                Interlocked.Decrement(ref target.PendingOutput);
                return true;
            }

            return false;
        }

        internal void Clear()
        {
            // A session swap also invalidates a record currently being decoded.
            // It cannot reappear after Clear even if its plugin returns later.
            ReplaceSession();
            Wake();
        }

        private void ReplaceSession()
        {
            Session retired = Interlocked.Exchange(ref session, new Session());
            // A blocked plugin holds its Session until it returns. Detach both
            // queues so Clear/Dispose can release a large backlog immediately,
            // without walking it on the UI thread or retaining it via that worker.
            Interlocked.Exchange(ref retired.Input, new ConcurrentQueue<PacketLogRecord>());
            Interlocked.Exchange(ref retired.Output, new ConcurrentQueue<PacketLogRow>());
        }

        private void Run()
        {
            try
            {
                while (Volatile.Read(ref disposed) == 0)
                {
                    Session target = Volatile.Read(ref session);
                    PacketLogRecord record;
                    if (!target.Input.TryDequeue(out record))
                    {
                        wake.WaitOne(100);
                        continue;
                    }

                    Interlocked.Decrement(ref target.PendingInput);
                    PacketLogRow row = null;
                    try
                    {
                        bool accepted = Volatile.Read(ref filter).Matches(record);
                        if (accepted)
                        {
                            row = format(record);
                        }
                        if (!accepted || row == null)
                        {
                            row = null;
                            Interlocked.Increment(ref filtered);
                        }
                    }
                    catch (Exception error)
                    {
                        Volatile.Write(ref lastError, error);
                        Interlocked.Increment(ref errors);
                        row = new PacketLogRow(record, new[] { PacketTimestamp.ToDisplayString(record.Timestamp),
                            record.TypeName, record.CobId.ToString("x3"), "",
                            record.PayloadText(), "Decode error: " + error.Message },
                            Color.White, Color.DarkRed);
                        try
                        {
                            Trace.WriteLine(error);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    finally
                    {
                        Interlocked.Increment(ref processed);
                    }

                    if (row != null && ReferenceEquals(target, Volatile.Read(ref session))
                        && Volatile.Read(ref disposed) == 0)
                    {
                        Interlocked.Increment(ref target.PendingOutput);
                        target.Output.Enqueue(row);
                    }
                }
            }
            finally
            {
                wake.Dispose();
            }
        }

        private void Wake()
        {
            try
            {
                wake.Set();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                ReplaceSession();
                Wake();
            }
            // A slow plugin must not make document disposal block the UI. The
            // worker owns no controls and exits when that callback returns.
        }
    }
}
