using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using libEDSsharp;

namespace ConformancePlugin
{
    public enum Verdict
    {
        Pass,
        Fail,
        Warning,
        Skip,
        Error
    }

    public sealed class CheckResult
    {
        public string Check { get; set; }
        public Verdict Verdict { get; set; }
        public string Evidence { get; set; }
    }

    public sealed class UploadResult
    {
        public byte[] Data = new byte[0];
        public uint Abort;
        public bool TimedOut;
    }

    public sealed class BusObservation
    {
        public long Frames;
        public long InvalidFrames;
        public long Heartbeats;
        public long InvalidHeartbeats;
        public byte? LastState;
        public long Emergencies;
        public long InvalidEmergencies;
    }

    // This boundary allows repeatable device simulations without a physical adapter.
    public interface IConformanceTransport : IDisposable
    {
        UploadResult Upload(byte node, ushort index, byte subindex, CancellationToken cancellation);
        BusObservation Snapshot();
        void SetNmt(byte node, byte command);
        bool WaitForState(byte state, int milliseconds, CancellationToken cancellation);
    }

    public sealed class RunOptions
    {
        public byte Node = 1;
        public bool ReadDictionary = true;
        public bool AbortProbes = true;
        public bool NmtTransitions;
        public bool TestProducerHeartbeat;
        public int ObserveMilliseconds = 3000;
        public string Connection = "Current CanMonitor connection";
    }

    public sealed class ConformanceReport
    {
        public const string Scope = "CANopen development pre-conformance checks; not CiA certification or a complete CiA 310-1 implementation.";
        public DateTime StartedUtc = DateTime.UtcNow;
        public DateTime FinishedUtc;
        public string SourceName;
        public string SourceHash;
        public string Connection;
        public byte Node;
        public bool Cancelled;
        public readonly List<CheckResult> Results = new List<CheckResult>();

        public void Add(string check, Verdict verdict, string evidence)
        {
            Results.Add(new CheckResult { Check = check, Verdict = verdict, Evidence = evidence });
        }

        public void SaveHtml(string filename)
        {
            Func<string, string> h = value => WebUtility.HtmlEncode(value ?? "");
            StringBuilder html = new StringBuilder("<!doctype html><html><head><meta charset='utf-8'><title>CANopen conformance report</title><style>body{font:15px Segoe UI,sans-serif;margin:32px;max-width:1400px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #ccc;padding:8px;text-align:left;vertical-align:top}th{background:#eee}.Fail,.Error{color:#a00000}.Warning{color:#855800}.Pass{color:#176632}td:last-child{white-space:pre-wrap;overflow-wrap:anywhere}</style></head><body><h1>CANopen conformance report</h1>");
            html.Append("<p>" + h(Scope) + "</p><p>Node: 0x" + Node.ToString("X2") + "<br>Connection: " + h(Connection));
            html.Append("<br>Description: " + h(SourceName) + "<br>SHA-256: " + h(SourceHash));
            html.Append("<br>UTC: " + StartedUtc.ToString("O") + " to " + FinishedUtc.ToString("O") + "<br>Cancelled: " + Cancelled + "</p>");
            html.Append("<p>" + h(string.Join("; ", Enum.GetValues(typeof(Verdict)).Cast<Verdict>().Select(v => v + ": " + Results.Count(r => r.Verdict == v)))) + "</p>");
            html.Append("<p>A Pass applies only to the named check and observed conditions. Skipped or unimplemented services have not passed. Host timings are observations, not wire-level timing certification. Reads do not verify configured defaults.</p><table><tr><th>Check</th><th>Result</th><th>Evidence</th></tr>");
            foreach (CheckResult result in Results)
            {
                html.Append("<tr><td>" + h(result.Check) + "</td><td class='" + result.Verdict + "'>" + result.Verdict + "</td><td>" + h(result.Evidence) + "</td></tr>");
            }
            html.Append("</table><p>References: <a href='https://www.can-cia.org/services/canopen-conformance-test-tool'>CiA test scope</a>; <a href='https://canopennode.github.io/CANopenNode/group__CO__NMT__Heartbeat.html'>NMT/heartbeat</a>; <a href='https://canopennode.github.io/CANopenNode/md_doc_2objectDictionary.html'>Object dictionary</a>.</p></body></html>");
            File.WriteAllText(filename, html.ToString(), new UTF8Encoding(false));
        }
    }

    public sealed class DeviceDescription
    {
        public EDSsharp Dictionary;
        public string Name;
        public string Hash;

        public static DeviceDescription Load(string filename)
        {
            EDSsharp dictionary;
            if (string.Equals(Path.GetExtension(filename), ".xdd", StringComparison.OrdinalIgnoreCase))
            {
                dictionary = XddLoader.Load(filename);
            }
            else
            {
                dictionary = new EDSsharp();
                dictionary.Loadfile(filename);
            }
            if (dictionary == null || dictionary.ods.Count == 0)
            {
                throw new FormatException("The description did not contain an object dictionary.");
            }
            using (SHA256 sha = SHA256.Create())
            using (Stream stream = File.OpenRead(filename))
            {
                return new DeviceDescription { Dictionary = dictionary, Name = Path.GetFileName(filename), Hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "") };
            }
        }
    }

    public sealed class ConformanceRunner
    {
        private readonly IConformanceTransport transport;
        private readonly Dictionary<uint, UploadResult> uploads = new Dictionary<uint, UploadResult>();
        private ConformanceReport report;
        private RunOptions options;
        private CancellationToken cancellation;
        public Action<CheckResult> Progress;

        public ConformanceRunner(IConformanceTransport transport)
        {
            this.transport = transport;
        }

        private void Add(string check, Verdict verdict, string evidence)
        {
            report.Add(check, verdict, evidence);
            Progress?.Invoke(report.Results.Last());
        }

        private UploadResult Read(ushort index, byte subindex)
        {
            cancellation.ThrowIfCancellationRequested();
            uint key = ((uint)index << 8) | subindex;
            UploadResult result;
            if (!uploads.TryGetValue(key, out result))
            {
                result = transport.Upload(options.Node, index, subindex, cancellation);
                uploads.Add(key, result);
            }
            return result;
        }

        private static string Evidence(UploadResult value)
        {
            return value.TimedOut ? "Timed out; no conformance conclusion." : value.Abort != 0 ? "SDO abort 0x" + value.Abort.ToString("X8") : value.Data.Length + " bytes: " + BitConverter.ToString(value.Data);
        }

        private bool CheckWidth(ushort index, byte subindex, int width, string purpose)
        {
            UploadResult value = Read(index, subindex);
            bool ok = !value.TimedOut && value.Abort == 0 && value.Data.Length == width;
            Add(purpose + " " + index.ToString("X4") + "/" + subindex.ToString("X2"),
                value.TimedOut ? Verdict.Error : ok ? Verdict.Pass : Verdict.Fail, "Expected " + width + " bytes. " + Evidence(value));
            return ok;
        }

        public ConformanceReport Run(DeviceDescription description, RunOptions runOptions, CancellationToken token)
        {
            if (runOptions.Node < 1 || runOptions.Node > 127 || runOptions.ObserveMilliseconds < 0 || runOptions.ObserveMilliseconds > 60000)
            {
                throw new ArgumentOutOfRangeException(nameof(runOptions));
            }
            options = runOptions;
            cancellation = token;
            uploads.Clear();
            report = new ConformanceReport { SourceName = description.Name, SourceHash = description.Hash, Connection = options.Connection, Node = options.Node };
            try
            {
                Add("Scope", Verdict.Warning, ConformanceReport.Scope);
                CheckWidth(0x1000, 0, 4, "Device type");
                if (CheckWidth(0x1001, 0, 1, "Error register"))
                {
                    Add("Error register reserved bit", (Read(0x1001, 0).Data[0] & 0x40) == 0 ? Verdict.Pass : Verdict.Fail, "Bit 6 must be zero.");
                }
                if (CheckWidth(0x1018, 0, 1, "Identity highest subindex"))
                {
                    byte highest = Read(0x1018, 0).Data[0];
                    Add("Identity structure", highest >= 1 && highest <= 4 ? Verdict.Pass : Verdict.Fail, "Highest subindex = " + highest + "; expected 1..4.");
                    for (byte sub = 1; sub <= Math.Min(4, (int)highest); sub++)
                    {
                        CheckWidth(0x1018, sub, 4, "Identity");
                    }
                }
                foreach (ushort index in new ushort[] { 0x1000, 0x1001, 0x1018 })
                {
                    Add("Description mandatory object " + index.ToString("X4"), description.Dictionary.ods.ContainsKey(index) ? Verdict.Pass : Verdict.Fail, "Presence in the supplied description.");
                }
                if (options.ReadDictionary)
                {
                    CheckDictionary(description.Dictionary);
                    CheckPdos(description.Dictionary);
                    CheckCommunication(description.Dictionary);
                }
                else
                {
                    Add("Object dictionary and PDO consistency", Verdict.Skip, "Full dictionary reads disabled.");
                }
                if (options.AbortProbes)
                {
                    UploadResult identityCount = Read(0x1018, 0);
                    if (identityCount.Abort == 0 && !identityCount.TimedOut && identityCount.Data.Length == 1 && identityCount.Data[0] < 255)
                    {
                        byte absentSub = (byte)(identityCount.Data[0] + 1);
                        UploadResult subProbe = transport.Upload(options.Node, 0x1018, absentSub, cancellation);
                        Add("SDO nonexistent-subindex abort", subProbe.TimedOut ? Verdict.Error : subProbe.Abort == 0x06090011 ? Verdict.Pass : Verdict.Fail,
                            "Identity subindex " + absentSub.ToString("X2") + " exceeds its advertised maximum; expected 0x06090011. " + Evidence(subProbe));
                    }
                    ushort absent = 0x5FFF;
                    while (absent >= 0x2000 && description.Dictionary.ods.ContainsKey(absent))
                    {
                        absent--;
                    }
                    if (absent >= 0x2000)
                    {
                        UploadResult probe = transport.Upload(options.Node, absent, 0, cancellation);
                        // An EDS can omit implemented objects: a successful upload is inconclusive.
                        Add("SDO nonexistent-object abort", probe.TimedOut ? Verdict.Error : probe.Abort == 0x06020000 ? Verdict.Pass : probe.Abort == 0 ? Verdict.Warning : Verdict.Fail,
                            "Probe " + absent.ToString("X4") + "/00 (not described); expected 0x06020000 if absent. " + Evidence(probe));
                    }
                    UploadResult recovery = transport.Upload(options.Node, 0x1000, 0, cancellation);
                    Add("SDO recovery after abort probe", !recovery.TimedOut && recovery.Abort == 0 && recovery.Data.Length == 4 ? Verdict.Pass : Verdict.Fail, Evidence(recovery));
                }
                else
                {
                    Add("SDO abort/recovery", Verdict.Skip, "Abort probes disabled.");
                }
                if (cancellation.WaitHandle.WaitOne(options.ObserveMilliseconds))
                {
                    cancellation.ThrowIfCancellationRequested();
                }
                CheckObservation(description.Dictionary);
                bool nmtExercised = false;
                if (options.TestProducerHeartbeat)
                {
                    IHeartbeatTransport heartbeat = transport as IHeartbeatTransport;
                    if (heartbeat == null)
                    {
                        Add("Enabled heartbeat test", Verdict.Skip, "Transport does not support controlled heartbeat configuration.");
                    }
                    else
                    {
                        HeartbeatProbe.Run(heartbeat, options.Node, Add, cancellation, () =>
                        {
                            if (options.NmtTransitions)
                            {
                                nmtExercised = true;
                                CheckNmt();
                            }
                        });
                    }
                }
                if (options.NmtTransitions && !nmtExercised)
                {
                    CheckNmt();
                }
                else if (!options.NmtTransitions)
                {
                    Add("NMT state transitions", Verdict.Skip, "State-changing tests not selected.");
                }
            }
            catch (OperationCanceledException)
            {
                report.Cancelled = true;
                Add("Run cancelled", Verdict.Skip, "Partial results only; unexecuted tests have not passed.");
            }
            catch (Exception error)
            {
                Add("Run interrupted", Verdict.Error, error.GetType().Name + ": " + error.Message);
            }
            finally
            {
                Add("Coverage gaps", Verdict.Skip, "Not exercised: physical/electrical layer, bit timing/error injection/bus-off, official CiA 310-1 cases, general SDO downloads (apart from optional heartbeat-period writes) and block transfers, reset/boot-up, PDO remapping/writes and triggered PDO timing, SYNC/TIME/EMCY generation, heartbeat consumer failures, LSS and device/application profiles. No unsupported service is counted as passed.");
                report.FinishedUtc = DateTime.UtcNow;
            }
            return report;
        }

        private void CheckDictionary(EDSsharp dictionary)
        {
            foreach (KeyValuePair<ushort, ODentry> item in dictionary.ods.Where(p => p.Key >= 0x1000))
            {
                ODentry parent = item.Value;
                var entries = parent.subobjects.Count == 0
                    ? new[] { new KeyValuePair<ushort, ODentry>(0, parent) }
                    : parent.subobjects.ToArray();
                foreach (KeyValuePair<ushort, ODentry> entry in entries)
                {
                    cancellation.ThrowIfCancellationRequested();
                    string address = item.Key.ToString("X4") + "/" + entry.Key.ToString("X2") + " " + entry.Value.parameter_name;
                    if (entry.Key > 255)
                    {
                        Add("Description " + address, Verdict.Fail, "Subindex exceeds 8 bits.");
                        continue;
                    }
                    AccessSDO access = entry.Value.AccessSDO();
                    if (access != AccessSDO.ro && access != AccessSDO.rw)
                    {
                        if (access == AccessSDO.wo && options.AbortProbes)
                        {
                            UploadResult denied = Read(item.Key, (byte)entry.Key);
                            Add("SDO write-only upload rejection " + address,
                                denied.TimedOut ? Verdict.Error : denied.Abort == 0x06010001 ? Verdict.Pass : Verdict.Fail,
                                "Expected 0x06010001 for an upload from a write-only object. " + Evidence(denied));
                        }
                        else
                        {
                            Add("SDO upload " + address, Verdict.Skip, "Not declared SDO-readable; no write is attempted.");
                        }
                        continue;
                    }
                    // Error-history entries beyond the live count need not be available.
                    if (item.Key == 0x1003 && entry.Key > 0)
                    {
                        UploadResult count = Read(item.Key, 0);
                        if (count.Abort == 0 && count.Data.Length == 1 && entry.Key > count.Data[0])
                        {
                            Add("SDO upload " + address, Verdict.Skip, "Beyond the current error-history count.");
                            continue;
                        }
                    }
                    UploadResult value = Read(item.Key, (byte)entry.Key);
                    if (value.TimedOut || value.Abort != 0)
                    {
                        Add("SDO upload " + address, value.TimedOut ? Verdict.Error : Verdict.Fail, Evidence(value));
                        continue;
                    }
                    DataType type = entry.Value.datatype == DataType.UNKNOWN ? parent.datatype : entry.Value.datatype;
                    int bits = entry.Value.Sizeofdatatype();
                    bool variable = type == DataType.VISIBLE_STRING || type == DataType.OCTET_STRING || type == DataType.UNICODE_STRING || type == DataType.DOMAIN || bits == 0;
                    bool valid = variable || value.Data.Length == (bits + 7) / 8;
                    if (type == DataType.BOOLEAN && value.Data.Length == 1 && value.Data[0] > 1)
                    {
                        valid = false;
                    }
                    Add("SDO upload " + address, valid ? Verdict.Pass : Verdict.Fail,
                        "Declared " + type + (variable ? " (variable/unspecified width). " : ", expected " + (bits + 7) / 8 + " bytes. ") + Evidence(value));
                }
            }
        }

        private bool Number(ushort index, byte sub, out uint number)
        {
            UploadResult value = Read(index, sub);
            number = 0;
            if (value.TimedOut || value.Abort != 0 || value.Data.Length == 0 || value.Data.Length > 4)
            {
                return false;
            }
            for (int i = 0; i < value.Data.Length; i++)
            {
                number |= (uint)value.Data[i] << (8 * i);
            }
            return true;
        }

        private void CheckPdos(EDSsharp dictionary)
        {
            foreach (ushort index in dictionary.ods.Keys.Where(i => (i >= 0x1600 && i <= 0x17FF) || (i >= 0x1A00 && i <= 0x1BFF)))
            {
                uint count;
                if (!Number(index, 0, out count))
                {
                    Add("PDO mapping " + index.ToString("X4"), Verdict.Error, "Mapping count could not be read.");
                    continue;
                }
                bool tx = index >= 0x1A00;
                int totalBits = 0;
                bool valid = count <= 64;
                // 254/255 select multiplexed PDO modes, not a list of 254/255 mappings.
                // Their scanner/dispatch services require a different test plan.
                bool multiplexed = count == 254 || count == 255;
                if (multiplexed)
                {
                    Add("Multiplexed PDO " + index.ToString("X4"), Verdict.Skip,
                        count == 254 ? "0xFE selects SAM-MPDO; ordinary mapping entries are unused. Scanner-list/service conformance not implemented."
                        : "0xFF selects DAM-MPDO; ordinary mapping entries are unused. Dispatch/service conformance not implemented.");
                    uint transmission;
                    if (Number((ushort)(index - 0x200), 2, out transmission))
                    {
                        Add("MPDO transmission type " + (index - 0x200).ToString("X4"), transmission == 254 || transmission == 255 ? Verdict.Pass : Verdict.Fail,
                            "Transmission type = " + transmission + "; expected 254 or 255 for multiplexed PDO.");
                    }
                }
                for (byte sub = 1; !multiplexed && sub <= Math.Min(count, 64U); sub++)
                {
                    uint map;
                    if (!Number(index, sub, out map))
                    {
                        valid = false;
                        continue;
                    }
                    ushort targetIndex = (ushort)(map >> 16);
                    byte targetSub = (byte)(map >> 8);
                    byte length = (byte)map;
                    totalBits += length;
                    ODentry target;
                    bool dummy = targetIndex >= 1 && targetIndex <= 7 && targetSub == 0;
                    bool found = dictionary.ods.TryGetValue(targetIndex, out target);
                    if (found && target.subobjects.Count > 0)
                    {
                        found = target.subobjects.TryGetValue(targetSub, out target);
                    }
                    else if (targetSub != 0)
                    {
                        found = false;
                    }
                    bool direction = found && (target.AccessPDO() == AccessPDO.tr || target.AccessPDO() == (tx ? AccessPDO.t : AccessPDO.r));
                    // The OD helper reports BOOLEAN as one logical bit, but its storage
                    // and SDO width are one byte. Byte-wise BOOLEAN mappings are valid.
                    // Partial mappings may also occupy fewer bits than their OD storage.
                    int capacity = found ? ((target.Sizeofdatatype() + 7) / 8) * 8 : 0;
                    int dummyCapacity = dummy ? new[] { 0, 8, 8, 16, 32, 8, 16, 32 }[targetIndex] : 0;
                    bool entryValid = length > 0 && length <= 64 && (dummy ? length <= dummyCapacity : direction && length <= capacity);
                    valid &= entryValid;
                    Add("PDO mapping " + index.ToString("X4") + "/" + sub.ToString("X2"), entryValid ? Verdict.Pass : Verdict.Fail,
                        "0x" + map.ToString("X8") + " -> " + targetIndex.ToString("X4") + "/" + targetSub.ToString("X2") + ", " + length + " bits; " + (found ? target.parameter_name : dummy ? "dummy mapping" : "not described") + "; " + (tx ? "TPDO" : "RPDO"));
                }
                if (!multiplexed)
                {
                    Add("PDO payload " + index.ToString("X4"), valid && totalBits <= 64 ? Verdict.Pass : Verdict.Fail,
                        count + " active mappings, " + totalBits + " bits; classic CANopen payload limit 64 bits.");
                }
                uint cob;
                if (Number((ushort)(index - 0x200), 1, out cob))
                {
                    Add("PDO COB-ID " + (index - 0x200).ToString("X4"), (cob & 0x20000000) != 0 ? Verdict.Skip : (cob & 0x1FFFF800) == 0 ? Verdict.Pass : Verdict.Fail,
                        "0x" + cob.ToString("X8") + ((cob & 0x80000000) != 0 ? "; disabled" : "; enabled") + "; extended identifiers cannot be fully observed by this transport.");
                }
            }
        }

        private void CheckObservation(EDSsharp dictionary)
        {
            BusObservation bus = transport.Snapshot();
            Add("Observed CAN frame shape", bus.InvalidFrames != 0 ? Verdict.Fail : bus.Frames > 0 ? Verdict.Pass : Verdict.Skip, bus.Frames + " decoded frames; " + bus.InvalidFrames + " malformed. Adapter/library may discard invalid frames before this observer.");
            Add("Observed heartbeat format", bus.InvalidHeartbeats != 0 ? Verdict.Fail : bus.Heartbeats > 0 ? Verdict.Pass : Verdict.Skip,
                bus.Heartbeats + " target-node heartbeats; " + bus.InvalidHeartbeats + " invalid. Last state: " + (bus.LastState.HasValue ? bus.LastState.Value.ToString() : "unknown"));
            if (dictionary.ods.ContainsKey(0x1017))
            {
                uint period;
                if (Number(0x1017, 0, out period))
                {
                    Add("Heartbeat producer", period == 0 ? Verdict.Skip : period > options.ObserveMilliseconds ? Verdict.Skip : bus.Heartbeats == 0 ? Verdict.Fail : Verdict.Pass,
                        "Configured period " + period + " ms; final observation window " + options.ObserveMilliseconds + " ms. Zero disables production. No wire-level timing assertion.");
                }
            }
            Add("Observed EMCY format", bus.InvalidEmergencies != 0 ? Verdict.Fail : bus.Emergencies > 0 ? Verdict.Pass : Verdict.Skip,
                bus.Emergencies + " observed on the default target EMCY COB-ID; " + bus.InvalidEmergencies + " malformed. EMCY generation not stimulated; custom COB-IDs not checked.");
        }

        private void CheckCommunication(EDSsharp dictionary)
        {
            foreach (ushort index in dictionary.ods.Keys.Where(i => (i >= 0x1400 && i <= 0x15FF) || (i >= 0x1800 && i <= 0x19FF)))
            {
                bool tx = index >= 0x1800;
                if (CheckWidth(index, 2, 1, "PDO transmission type width"))
                {
                    uint transmission;
                    Number(index, 2, out transmission);
                    bool allowed = transmission <= 240 || transmission >= 254 || (tx && transmission >= 252);
                    Add("PDO transmission type " + index.ToString("X4"), allowed ? Verdict.Pass : Verdict.Fail,
                        "Value " + transmission + "; reserved transmission types are rejected; RTR modes are TPDO-only.");
                }
                if (tx && dictionary.ods[index].subobjects.ContainsKey(6) && CheckWidth(index, 6, 1, "SYNC start width"))
                {
                    uint start;
                    Number(index, 6, out start);
                    Add("SYNC start value " + index.ToString("X4"), start <= 240 ? Verdict.Pass : Verdict.Fail, "Value " + start + "; expected 0..240.");
                }
            }
        }

        private void CheckNmt()
        {
            byte? original = transport.Snapshot().LastState;
            if (!original.HasValue || (original != 5 && original != 127 && original != 4))
            {
                Add("NMT state transitions", Verdict.Skip, "Cannot safely restore an unknown initial state; enable heartbeat before running this optional test.");
                return;
            }
            try
            {
                foreach (var transition in new[] { new byte[] { 128, 127 }, new byte[] { 1, 5 }, new byte[] { 2, 4 } })
                {
                    cancellation.ThrowIfCancellationRequested();
                    transport.SetNmt(options.Node, transition[0]);
                    bool observed = transport.WaitForState(transition[1], 5000, cancellation);
                    Add("NMT command " + transition[0], observed ? Verdict.Pass : Verdict.Error, "Expected a fresh heartbeat with state " + transition[1] + " within the host observation timeout (5 seconds).");
                }
            }
            finally
            {
                // Restoration must still execute if the user cancels a state-changing run.
                byte command = original == 5 ? (byte)1 : original == 127 ? (byte)128 : (byte)2;
                transport.SetNmt(options.Node, command);
                bool restored = transport.WaitForState(original.Value, 5000, CancellationToken.None);
                Add("Restore initial NMT state", restored ? Verdict.Pass : Verdict.Error, "Initial state " + original + (restored ? " observed again." : " could not be verified; check the device."));
            }
        }
    }
}
