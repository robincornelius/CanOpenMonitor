using System;
using System.Linq;
using System.Threading;

namespace ConformancePlugin
{
    public sealed class HeartbeatSample
    {
        public double Milliseconds;
        public int Length;
        public byte State;
    }

    public sealed class HeartbeatWindow
    {
        public HeartbeatSample[] Samples = new HeartbeatSample[0];
        public bool Overflow;
    }

    public interface IHeartbeatTransport : IConformanceTransport
    {
        void SetProducerHeartbeat(byte node, ushort milliseconds, CancellationToken cancellation);
        HeartbeatWindow ObserveHeartbeats(int milliseconds, CancellationToken cancellation);
    }

    public static class HeartbeatProbe
    {
        public static void Run(IHeartbeatTransport transport, byte node, Action<string, Verdict, string> add, CancellationToken cancellation, Action whileEnabled = null)
        {
            UploadResult before = transport.Upload(node, 0x1017, 0, cancellation);
            if (before.TimedOut || before.Abort != 0 || before.Data.Length != 2)
            {
                add("Heartbeat test setup", Verdict.Error, "Could not read the original UNSIGNED16 producer period; no configuration changed.");
                return;
            }
            ushort original = BitConverter.ToUInt16(before.Data, 0);
            const ushort period = 500;
            add("Original heartbeat period", Verdict.Pass, "0x1017/00 = " + original + " ms. Temporarily testing at 500 ms; original value will be restored.");
            try
            {
                // Even a failed/uncertain download may have reached the device: always restore.
                transport.SetProducerHeartbeat(node, period, cancellation);
                UploadResult enabled = transport.Upload(node, 0x1017, 0, cancellation);
                bool readback = !enabled.TimedOut && enabled.Abort == 0 && enabled.Data.Length == 2 && BitConverter.ToUInt16(enabled.Data, 0) == period;
                add("Heartbeat period write/readback", readback ? Verdict.Pass : Verdict.Fail, "Requested 500 ms at 0x1017/00; readback " + (readback ? "500 ms." : "did not match."));
                if (!readback)
                {
                    return;
                }
                HeartbeatWindow window = transport.ObserveHeartbeats(4000, cancellation);
                bool format = window.Samples.All(s => s.Length == 1 && (s.State == 4 || s.State == 5 || s.State == 127));
                add("Enabled heartbeat production", window.Overflow ? Verdict.Error : window.Samples.Length >= 6 ? Verdict.Pass : Verdict.Fail,
                    window.Samples.Length + " frames observed in 4000 ms at COB-ID 0x" + (0x700 + node).ToString("X3") + "; expected approximately 8 at 500 ms (at least 6 required by this host-level check).");
                add("Enabled heartbeat frame/state", window.Samples.Length == 0 ? Verdict.Skip : format ? Verdict.Pass : Verdict.Fail,
                    "Expected DLC 1 and state 4, 5 or 127. Observed states: " + string.Join(", ", window.Samples.Select(s => "0x" + s.State.ToString("X2")).Distinct()) + ". Boot-up is not periodic heartbeat.");
                if (window.Samples.Length >= 3 && !window.Overflow)
                {
                    double[] gaps = window.Samples.Skip(1).Select((sample, i) => sample.Milliseconds - window.Samples[i].Milliseconds).ToArray();
                    double mean = gaps.Average();
                    // USB buffering and Windows scheduling prevent wire-level timing certification.
                    bool plausible = mean >= 400 && mean <= 600 && gaps.Min() >= 250 && gaps.Max() <= 1000;
                    add("Heartbeat host interval observation", plausible ? Verdict.Pass : Verdict.Warning,
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0} intervals: min {1:F2}, mean {2:F2}, max {3:F2} ms; configured 500 ms. Host plausibility bounds: mean 400..600, individual 250..1000 ms; these are diagnostic tolerances, not CiA timing limits.", gaps.Length, gaps.Min(), mean, gaps.Max()));
                }
                else
                {
                    add("Heartbeat host interval observation", Verdict.Skip, "Insufficient samples or capture overflow; no timing conclusion.");
                }
                whileEnabled?.Invoke();
            }
            finally
            {
                // Restoration is independent of cancellation and does not store settings to flash.
                try
                {
                    transport.SetProducerHeartbeat(node, original, CancellationToken.None);
                    UploadResult restored = transport.Upload(node, 0x1017, 0, CancellationToken.None);
                    bool matches = !restored.TimedOut && restored.Abort == 0 && restored.Data.Length == 2 && BitConverter.ToUInt16(restored.Data, 0) == original;
                    add("Restore heartbeat period", matches ? Verdict.Pass : Verdict.Error, "Original " + original + " ms " + (matches ? "verified by fresh SDO readback." : "could not be verified; check 0x1017/00 on the device."));
                    if (matches && original == 0)
                    {
                        transport.ObserveHeartbeats(500, CancellationToken.None);
                        HeartbeatWindow quiet = transport.ObserveHeartbeats(1000, CancellationToken.None);
                        add("Heartbeat disabled after restoration", quiet.Samples.Length == 0 && !quiet.Overflow ? Verdict.Pass : Verdict.Fail,
                            quiet.Samples.Length + " frames in a 1000 ms quiet window after a 500 ms settling period; expected zero.");
                    }
                }
                catch (Exception error)
                {
                    add("Restore heartbeat period", Verdict.Error, "Restoration failed: " + error.Message + "; manually check 0x1017/00 (original " + original + " ms).");
                }
            }
        }
    }
}
