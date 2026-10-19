using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ConformancePlugin;
using libEDSsharp;

internal static class ConformanceTests
{
    private sealed class ExistingDecoder : PDOInterface.IPDOParser
    {
        public void deregisterplugin()
        {
        }
        public void registerPDOS()
        {
        }
        public void setlco(libCanopenSimple.libCanopenSimple can)
        {
        }
        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "existing decoder";
        }
    }

    private sealed class Device : IConformanceTransport
    {
        public readonly Dictionary<uint, UploadResult> Values = new Dictionary<uint, UploadResult>();
        public readonly List<byte> Commands = new List<byte>();
        public readonly List<uint> Reads = new List<uint>();
        public BusObservation Bus = new BusObservation { Frames = 100, Heartbeats = 3, LastState = 127 };
        public CancellationTokenSource CancelOnState;

        public UploadResult Upload(byte node, ushort index, byte subindex, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            uint key = ((uint)index << 8) | subindex;
            Reads.Add(key);
            UploadResult value;
            if (index == 0x1018 && subindex > 1)
            {
                return new UploadResult { Abort = 0x06090011 };
            }
            return Values.TryGetValue(key, out value) ? value : new UploadResult { Abort = 0x06020000 };
        }

        public void Put(ushort index, byte subindex, uint value, int bytes)
        {
            Values[((uint)index << 8) | subindex] = new UploadResult { Data = BitConverter.GetBytes(value).Take(bytes).ToArray() };
        }

        public BusObservation Snapshot()
        {
            return Bus;
        }
        public void SetNmt(byte node, byte command)
        {
            Commands.Add(command);
        }
        public bool WaitForState(byte state, int milliseconds, CancellationToken token)
        {
            if (CancelOnState != null)
            {
                CancelOnState.Cancel();
                CancelOnState = null;
                token.ThrowIfCancellationRequested();
            }
            Bus.LastState = state;
            return true;
        }
        public void Dispose()
        {
        }
    }

    private static DeviceDescription Description(Device device)
    {
        var dictionary = new EDSsharp();
        dictionary.ods.Clear();
        Add(dictionary, device, 0x1000, 0, DataType.UNSIGNED32, 0, 4);
        Add(dictionary, device, 0x1001, 0, DataType.UNSIGNED8, 0, 1);
        Add(dictionary, device, 0x1018, 0, DataType.UNSIGNED8, 1, 1);
        Add(dictionary, device, 0x1018, 1, DataType.UNSIGNED32, 123, 4);
        Add(dictionary, device, 0x1017, 0, DataType.UNSIGNED16, 100, 2);
        Add(dictionary, device, 0x2000, 0, DataType.UNSIGNED16, 10, 2);
        dictionary.ods[0x2000].PDOtype = PDOMappingType.optional;
        Add(dictionary, device, 0x1800, 1, DataType.UNSIGNED32, 0x181, 4);
        Add(dictionary, device, 0x1800, 2, DataType.UNSIGNED8, 254, 1);
        Add(dictionary, device, 0x1A00, 0, DataType.UNSIGNED8, 1, 1);
        Add(dictionary, device, 0x1A00, 1, DataType.UNSIGNED32, 0x20000010, 4);
        return new DeviceDescription { Dictionary = dictionary, Name = "generic-fixture.xdd", Hash = "fixture" };
    }

    private static void Add(EDSsharp dictionary, Device device, ushort index, byte subindex, DataType type, uint value, int bytes)
    {
        bool record = index == 0x1018 || index == 0x1800 || index == 0x1A00;
        ODentry parent;
        if (!dictionary.ods.TryGetValue(index, out parent))
        {
            parent = new ODentry { Index = index, parameter_name = "Object " + index.ToString("X4"), datatype = type, objecttype = record ? ObjectType.RECORD : ObjectType.VAR, accesstype = EDSsharp.AccessType.ro };
            dictionary.ods.Add(index, parent);
        }
        if (record)
        {
            parent.addsubobject(subindex, new ODentry("Sub " + subindex, index, type, value.ToString(), EDSsharp.AccessType.ro, PDOMappingType.no, parent));
        }
        device.Put(index, subindex, value, bytes);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    private static ConformanceReport Run(Device device, DeviceDescription description, bool nmt = false, CancellationToken token = default(CancellationToken))
    {
        return new ConformanceRunner(device).Run(description, new RunOptions { ObserveMilliseconds = 0, NmtTransitions = nmt }, token);
    }

    private static bool Has(ConformanceReport report, string check, Verdict verdict)
    {
        return report.Results.Any(r => r.Check.StartsWith(check, StringComparison.Ordinal) && r.Verdict == verdict);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "--hardware")
            {
                return Hardware(args);
            }
            var device = new Device();
            var description = Description(device);
            var report = Run(device, description);
            Require(!report.Results.Any(r => r.Verdict == Verdict.Fail || r.Verdict == Verdict.Error), "Healthy fixture failed");
            Require(ExitCode(report) == 0, "Healthy report exit code");
            Require(device.Commands.Count == 0, "Default run changed NMT state");
            Require(Has(report, "Coverage gaps", Verdict.Skip), "Unimplemented tests presented as passed");
            Console.WriteLine("PASS: healthy fixture, PDO decoding, abort recovery, read-only default and explicit coverage gaps");

            device = new Device(); description = Description(device);
            description.Dictionary.ods[0x2000].datatype = DataType.BOOLEAN;
            device.Put(0x2000, 0, 1, 1);
            device.Put(0x1A00, 1, 0x20000008, 4);
            report = Run(device, description);
            Require(Has(report, "PDO mapping 1A00/01", Verdict.Pass), "Byte-wise BOOLEAN mapping rejected");
            foreach (uint mode in new uint[] { 254, 255 })
            {
                device = new Device(); description = Description(device);
                device.Put(0x1A00, 0, mode, 1);
                device.Put(0x1800, 2, 254, 1);
                report = Run(device, description);
                Require(Has(report, "Multiplexed PDO", Verdict.Skip) && Has(report, "MPDO transmission type", Verdict.Pass), "MPDO mode incorrectly treated as mapping count");
                Require(!device.Reads.Contains(0x1A0002) && !Has(report, "PDO payload", Verdict.Fail), "MPDO generated spurious mapping probes/failures");
            }
            Console.WriteLine("PASS: byte-wise BOOLEAN mapping and SAM/DAM MPDO special modes");

            device = new Device(); description = Description(device);
            device.Put(0x1000, 0, 0, 2);
            device.Put(0x2000, 0, 0, 4);
            device.Put(0x1A00, 1, 0x20000050, 4);
            device.Bus.InvalidHeartbeats = 1;
            device.Bus.InvalidEmergencies = 1;
            report = Run(device, description);
            Require(Has(report, "Device type", Verdict.Fail) && Has(report, "SDO upload 2000", Verdict.Fail), "Wrong data width escaped detection");
            Require(Has(report, "PDO mapping 1A00/01", Verdict.Fail) && Has(report, "PDO payload", Verdict.Fail), "Oversized mapping escaped detection");
            Require(Has(report, "Observed heartbeat", Verdict.Fail) && Has(report, "Observed EMCY", Verdict.Fail), "Malformed observation escaped detection");
            Require(ExitCode(report) == 1, "A failing hardware report must return nonzero");
            Console.WriteLine("PASS: fixed-width errors, oversized mappings and malformed heartbeat/EMCY are failures");

            device = new Device(); description = Description(device);
            device.Values[0x100000] = new UploadResult { TimedOut = true };
            device.Values[0x200000] = new UploadResult { Abort = 0x06010001 };
            description.Dictionary.ods[0x2000].PDOtype = PDOMappingType.no;
            report = Run(device, description);
            Require(Has(report, "Device type", Verdict.Error) && Has(report, "SDO upload 2000", Verdict.Fail), "Timeout/abort verdict wrong");
            Require(Has(report, "PDO mapping 1A00/01", Verdict.Fail), "Unmappable target accepted");
            Require(ExitCode(report) == 2, "An interrupted hardware check must return an error exit code");
            Console.WriteLine("PASS: timeouts, unexpected aborts and unmappable objects remain visible");

            device = new Device(); description = Description(device);
            device.Put(0x1001, 0, 0x40, 1);
            device.Put(0x1800, 2, 241, 1);
            description.Dictionary.ods[0x2000].accesstype = EDSsharp.AccessType.wo;
            device.Values[0x200000] = new UploadResult { Abort = 0x06010001 };
            device.Bus.Frames = 0;
            report = Run(device, description);
            Require(Has(report, "Error register reserved bit", Verdict.Fail), "Reserved error bit accepted");
            Require(Has(report, "PDO transmission type 1800", Verdict.Fail), "Reserved transmission type accepted");
            Require(Has(report, "SDO write-only upload rejection", Verdict.Pass), "Write-only access rejection not checked");
            Require(Has(report, "Observed CAN frame shape", Verdict.Skip), "No observations counted as a pass");
            Console.WriteLine("PASS: reserved fields, write-only access rejection and no-traffic uncertainty");

            device = new Device(); description = Description(device);
            Add(description.Dictionary, device, 0x1003, 0, DataType.UNSIGNED8, 0, 1);
            // A live zero error count must not lead to reads of unused history entries.
            description.Dictionary.ods[0x1003].objecttype = ObjectType.ARRAY;
            description.Dictionary.ods[0x1003].addsubobject(0, new ODentry("Count", 0x1003, DataType.UNSIGNED8, "0", EDSsharp.AccessType.ro, PDOMappingType.no, description.Dictionary.ods[0x1003]));
            description.Dictionary.ods[0x1003].addsubobject(1, new ODentry("Error", 0x1003, DataType.UNSIGNED32, "0", EDSsharp.AccessType.ro, PDOMappingType.no, description.Dictionary.ods[0x1003]));
            report = Run(device, description);
            Require(!device.Reads.Contains(0x100301) && Has(report, "SDO upload 1003/01", Verdict.Skip), "Unused error history read");
            Console.WriteLine("PASS: dynamic error-history count respected");

            device = new Device(); description = Description(device);
            report = Run(device, description, true);
            Require(device.Commands.SequenceEqual(new byte[] { 128, 1, 2, 128 }), "NMT sequence/restoration incorrect");
            device = new Device(); description = Description(device); device.Bus.LastState = null;
            report = Run(device, description, true);
            Require(device.Commands.Count == 0 && Has(report, "NMT state transitions", Verdict.Skip), "Unknown initial state changed");
            device = new Device(); description = Description(device);
            device.Bus.LastState = 5;
            using (var cancel = new CancellationTokenSource())
            {
                device.CancelOnState = cancel;
                report = Run(device, description, true, cancel.Token);
                Require(report.Cancelled && device.Commands.SequenceEqual(new byte[] { 128, 1 }), "Cancellation failed to restore operational NMT state");
                Require(ExitCode(report) == 3, "A cancelled report must not return success");
            }
            Console.WriteLine("PASS: NMT transitions, unknown-state refusal and restoration on cancellation");

            string filename = Path.GetTempFileName();
            try
            {
                report.SourceName = "<script>alert(1)</script>";
                report.SaveHtml(filename);
                string html = File.ReadAllText(filename);
                Require(!html.Contains("<script>") && html.Contains("&lt;script&gt;") && html.Contains("Coverage gaps"), "Report escaping/coverage failed");
            }
            finally
            {
                File.Delete(filename);
            }
            var plugin = new ConformancePlugin.ConformancePlugin();
            Require(plugin.GetVerbs("Tools").Any(v => v.Name == "CANopen Conformance"), "Tools menu not registered");
            Require(!((object)plugin is PDOInterface.IPDOParser), "Tool-only plugin must not replace the active packet decoder");
            using (var form = new ConformanceForm(() => null))
            {
                form.Show(); Application.DoEvents();
                Require(form.Text == "CANopen Conformance" && form.Controls.Count > 0, "UI failed to create");
                form.Close();
            }
            using (var menu = new MenuStrip())
            {
                var manager = new CanMonitor.PluginManager { menuStrip1 = menu };
                var decoder = new ExistingDecoder();
                manager.ipdo = decoder;
                manager.loadplugin(typeof(ConformancePlugin.ConformancePlugin).Assembly.Location, false);
                Require(manager.plugins.Count == 1 && ReferenceEquals(manager.ipdo, decoder), "Loading tool replaced decoder or failed");
                var tools = menu.Items.OfType<ToolStripMenuItem>().Single(m => m.Text == "Tools");
                tools.DropDownItems.OfType<ToolStripMenuItem>().Single(m => m.Text == "CANopen Conformance").PerformClick();
                Application.DoEvents();
                var window = Application.OpenForms.OfType<ConformanceForm>().Single();
                Require(window.Visible, "Actual Tools action did not open the conformance window");
                window.Close();
            }
            Console.WriteLine("PASS: HTML evidence escaping, plugin registration and actual form lifecycle");
            Console.WriteLine("PASS: production plugin loader, actual Tools menu action and existing decoder preservation");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static int Hardware(string[] args)
    {
        if (args.Length < 7)
        {
            throw new ArgumentException("--hardware <description> <node-hex> <500|250|125|1000> <driver-without-extension> <adapter-path> <report.html> [--nmt]");
        }
        byte target = Convert.ToByte(args[2], 16);
        var speed = args[3] == "500" ? libCanopenSimple.BUSSPEED.BUS_500Kbit : args[3] == "250" ? libCanopenSimple.BUSSPEED.BUS_250Kbit : args[3] == "125" ? libCanopenSimple.BUSSPEED.BUS_125Kbit : args[3] == "1000" ? libCanopenSimple.BUSSPEED.BUS_1Mbit : throw new ArgumentException("Unsupported bitrate");
        DeviceDescription description = DeviceDescription.Load(args[1]);
        var can = new libCanopenSimple.libCanopenSimple();
        can.dbglevel = libCanopenSimple.debuglevel.DEBUG_NONE;
        can.ListenerDiagnostic += (sender, diagnostic) => Console.WriteLine("Listener diagnostic: " + diagnostic.Kind + "; " + diagnostic.Operation + "; " + diagnostic.Exception?.Message);
        if (!can.open(args[5], speed, args[4]))
        {
            throw new InvalidOperationException("Could not open the selected CAN adapter.");
        }
        using (var cancellation = new CancellationTokenSource())
        {
            ConsoleCancelEventHandler cancel = (sender, e) =>
            {
                e.Cancel = true;
                cancellation.Cancel();
            };
            Console.CancelKeyPress += cancel;
            try
            {
                using (var transport = new CanopenTransport(can, target))
                {
                    var runner = new ConformanceRunner(transport) { Progress = r => Console.WriteLine(r.Verdict + " " + r.Check + ": " + r.Evidence) };
                    ConformanceReport report = runner.Run(description, new RunOptions { Node = target, Connection = Path.GetFileName(args[4]) + "; " + args[5] + "; " + args[3] + " kbit/s", NmtTransitions = args.Contains("--nmt") }, cancellation.Token);
                    report.SaveHtml(args[6]);
                    Console.WriteLine("Report: " + args[6]);
                    return ExitCode(report);
                }
            }
            finally
            {
                Console.CancelKeyPress -= cancel;
                can.close();
            }
        }
    }

    private static int ExitCode(ConformanceReport report)
    {
        return report.Cancelled ? 3 : report.Results.Any(r => r.Verdict == Verdict.Error) ? 2 : report.Results.Any(r => r.Verdict == Verdict.Fail) ? 1 : 0;
    }
}

// Exercise the production PluginManager with no hardware discovery or user settings.
namespace CanMonitor
{
    internal static class Program
    {
        public static libCanopenSimple.libCanopenSimple lco = null;
        public static string assemblyfolder = AppDomain.CurrentDomain.BaseDirectory;
        public static string appdatafolder = AppDomain.CurrentDomain.BaseDirectory;
    }
}
