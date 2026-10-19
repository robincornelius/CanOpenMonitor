using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using libCanopenSimple;
using libEDSsharp;
using SDOEditorPlugin;

internal static class PdoEditorTests
{
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    private static T Field<T>(object instance, string name)
    {
        return (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
    }

    private static void Pump(Func<bool> condition)
    {
        var timer = Stopwatch.StartNew();
        while (!condition())
        {
            Require(timer.ElapsedMilliseconds < 5000, "UI operation did not complete");
            Application.DoEvents();
            Thread.Sleep(1);
        }
    }

    private static ODentry Entry(string name, ushort index, DataType type, PDOMappingType mapping = PDOMappingType.no)
    {
        return new ODentry(name, index, type, "0", EDSsharp.AccessType.rw, mapping);
    }

    private static EDSsharp Dictionary()
    {
        var dictionary = new EDSsharp();
        foreach (ushort index in new ushort[] { 0x1400, 0x1800 })
        {
            var record = new ODentry(index == 0x1800 ? "Transmit process data" : "Receive process data", index, 0);
            record.objecttype = ObjectType.RECORD;
            record.addsubobject(0, Entry("Highest subindex", index, DataType.UNSIGNED8));
            record.Getsubobject(0).accesstype = EDSsharp.AccessType.ro;
            record.addsubobject(1, Entry("COB-ID", index, DataType.UNSIGNED32));
            record.addsubobject(2, Entry("Transmission type", index, DataType.UNSIGNED8));
            record.addsubobject(3, Entry("Inhibit time", index, DataType.UNSIGNED16));
            dictionary.ods.Add(index, record);
            var map = new ODentry("Mapping", (ushort)(index + 0x200), 0);
            map.addsubobject(0, Entry("Mapped objects", map.Index, DataType.UNSIGNED8));
            dictionary.ods.Add(map.Index, map);
        }
        var parent = new ODentry("Process values", 0x2000, 0);
        parent.addsubobject(1, Entry("Measured value", 0x2000, DataType.UNSIGNED16, PDOMappingType.TPDO));
        parent.addsubobject(2, Entry("Demand value", 0x2000, DataType.UNSIGNED16, PDOMappingType.RPDO));
        dictionary.ods.Add(parent.Index, parent);
        dictionary.ods.Add(0x2001, Entry("Shared counter", 0x2001, DataType.UNSIGNED32, PDOMappingType.optional));
        dictionary.ods.Add(0x2002, Entry("Not PDO mappable", 0x2002, DataType.UNSIGNED16));
        return dictionary;
    }

    private sealed class Device : IPdoEditorDevice
    {
        internal Dictionary<byte, byte[]> Values = new Dictionary<byte, byte[]> {
            { 0, new byte[] { 3 } }, { 1, BitConverter.GetBytes(0x199U) }, { 2, new byte[] { 255 } }, { 3, BitConverter.GetBytes((ushort)200) } };
        internal uint[] Mappings = { 0x20000110 };
        internal SdoParameterWrite[] LastWrites;
        internal int Saves;
        internal bool FailSave;
        internal TaskCompletionSource<bool> SaveGate;

        public Task<PdoConfigurationSnapshot> ReadAsync(PdoSelection selection)
        {
            return Task.FromResult(new PdoConfigurationSnapshot(selection.Index, Values,
                Mappings, new Dictionary<byte, string>()));
        }

        public async Task<PdoConfigurationResult> SaveAsync(SdoParameterWrite[] writes)
        {
            Saves++;
            LastWrites = writes;
            if (SaveGate != null)
            {
                await SaveGate.Task;
            }
            if (FailSave)
            {
                throw new InvalidOperationException("Injected device abort");
            }
            var mapping = Mappings.ToList();
            foreach (var write in writes)
            {
                if (write.Index == 0x1800 || write.Index == 0x1400)
                {
                    Values[write.Subindex] = (byte[])write.Data.Clone();
                }
                else if (write.Subindex != 0)
                {
                    while (mapping.Count < write.Subindex)
                    {
                        mapping.Add(0);
                    }
                    mapping[write.Subindex - 1] = BitConverter.ToUInt32(write.Data, 0);
                }
            }
            var count = writes.FirstOrDefault(write => (write.Index == 0x1A00 || write.Index == 0x1600) && write.Subindex == 0);
            Mappings = count == null ? mapping.ToArray() : mapping.Take(count.Data[0]).ToArray();
            return new PdoConfigurationResult();
        }
    }

    private static void Model()
    {
        EDSsharp dictionary = Dictionary();
        Require(PdoEditorModel.Selections(dictionary).Length == 2, "TX/RX selection missing");
        var tx = PdoEditorModel.Selections(dictionary).Single(selection => selection.Transmit);
        var options = PdoEditorModel.Options(dictionary, true);
        Require(options.Any(option => option.Descriptor == 0x20000110) && options.Any(option => option.Descriptor == 0x20010020)
            && !options.Any(option => option.Descriptor == 0x20000210 || option.Descriptor == 0x20020010), "TX mapping eligibility wrong");
        Require(PdoEditorModel.Options(dictionary, false).Any(option => option.Descriptor == 0x20000210), "RX mapping eligibility wrong");
        Require(PdoEditorModel.Decode(dictionary, 0x20000110).Contains("Process values / Measured value")
            && PdoEditorModel.Decode(dictionary, 0x23000010).Contains("unknown"), "Mapping name/index decoding failed");
        var snapshot = new Device().ReadAsync(tx).Result;
        var draft = new Dictionary<byte, string> { { 3, "0xC8" } };
        Require(PdoEditorModel.BuildWrites(tx, snapshot, draft, snapshot.Mappings.ToList(), options, true).Length == 0, "Equivalent numeric draft changed device");
        draft[3] = "250";
        var writes = PdoEditorModel.BuildWrites(tx, snapshot, draft, new List<uint> { 0x20010020 }, options, true);
        Require(writes.Any(write => write.Index == 0x1800 && write.Subindex == 3 && write.Data.Length == 2)
            && writes.Any(write => write.Index == 0x1A00 && write.Subindex == 0 && write.Data[0] == 1), "Typed writes or mapping count missing");
        foreach (string invalid in new[] { "65536", "-1", "not a number" })
        {
            bool failed = false;
            try
            {
                PdoEditorModel.Parse(invalid, 2);
            }
            catch (FormatException)
            {
                failed = true;
            }
            Require(failed, "Invalid communication value accepted");
        }
        bool overflow = false;
        try
        {
            PdoEditorModel.BuildWrites(tx, snapshot, draft, new List<uint> { 0x20010020, 0x20010020, 0x20000110 }, options, true);
        }
        catch (InvalidOperationException)
        {
            overflow = true;
        }
        Require(overflow, "Oversized PDO accepted");
        Console.WriteLine("PASS: PDO catalog, TX/RX eligibility, EDS decoding, widths, count and payload validation");
    }

    private static Task Save(PdoEditorForm form)
    {
        return (Task)typeof(PdoEditorForm).GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
    }

    private static void Window()
    {
        EDSsharp dictionary = Dictionary();
        var device = new Device();
        using (var form = new PdoEditorForm(dictionary, 1, device))
        {
            form.Show();
            var selector = Field<ComboBox>(form, "selector");
            selector.SelectedItem = selector.Items.Cast<PdoSelection>().Single(selection => selection.Transmit);
            Pump(() => Field<PdoConfigurationSnapshot>(form, "snapshot") != null && !Field<bool>(form, "busy"));
            var grid = Field<DataGridView>(form, "grid");
            Require(grid.Rows.Count == 1 && Convert.ToString(grid.Rows[0].Cells["current"].Value).Contains("Measured value"), "Current mapping not decoded in window");
            Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text = "250";
            Field<Button>(form, "revert").PerformClick();
            Require(Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text == "200" && device.Saves == 0, "Revert wrote hardware or failed to restore draft");
            Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text = "250";
            Task save = Save(form);
            Pump(() => save.IsCompleted);
            Require(device.Saves == 1 && device.LastWrites.Length == 1 && device.LastWrites[0].Index == 0x1800
                && device.LastWrites[0].Subindex == 3 && BitConverter.ToUInt16(device.LastWrites[0].Data, 0) == 250, "Window communication save wrong");
            grid.Rows[0].Cells["target"].Value = 0x20010020U;
            Field<Button>(form, "add").PerformClick();
            Require(grid.Rows.Count == 2 && Field<List<uint>>(form, "draftMappings").Sum(value => (byte)value) == 48, "Mapping selection/add failed");
            save = Save(form);
            Pump(() => save.IsCompleted);
            Require(device.Mappings.Length == 2 && device.Mappings[0] == 0x20010020
                && device.LastWrites.Any(write => write.Index == 0x1A00 && write.Subindex == 0 && write.Data[0] == 2), "Window mapping save omitted count");
            device.FailSave = true;
            Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text = "300";
            save = Save(form);
            Pump(() => save.IsCompleted);
            Require(Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text == "300"
                && Field<Label>(form, "status").Text.Contains("Injected device abort"), "Failed save discarded draft or error");
            Field<Button>(form, "revert").PerformClick();
            device.FailSave = false;
            device.Values[3] = BitConverter.GetBytes((ushort)400);
            Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text = "300";
            int before = device.Saves;
            save = Save(form);
            Pump(() => save.IsCompleted);
            Require(device.Saves == before && Field<Label>(form, "status").Text.Contains("Device values changed"), "Stale device values were overwritten");
            Field<Button>(form, "revert").PerformClick();
            Field<Button>(form, "reload").PerformClick();
            Pump(() => Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text == "400");
            device.SaveGate = new TaskCompletionSource<bool>();
            Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text = "500";
            save = Save(form);
            Require(!save.IsCompleted && !Field<Button>(form, "close").Enabled, "Save blocked UI or permitted premature close");
            form.Close();
            Require(!form.IsDisposed, "Window closed during device configuration");
            device.SaveGate.SetResult(true);
            Pump(() => save.IsCompleted);
            Require(Field<Dictionary<byte, TextBox>>(form, "editors")[3].Text == "500", "Successful save did not refresh baseline");
            form.Close();
        }
        dictionary.ods[0x1A00].Getsubobject(0).accesstype = EDSsharp.AccessType.ro;
        using (var form = new PdoEditorForm(dictionary, 1, new Device()))
        {
            form.Show();
            var selector = Field<ComboBox>(form, "selector");
            selector.SelectedItem = selector.Items.Cast<PdoSelection>().Single(selection => selection.Transmit);
            Pump(() => Field<PdoConfigurationSnapshot>(form, "snapshot") != null);
            Require(!Field<Button>(form, "add").Enabled && Field<DataGridView>(form, "grid").Columns["target"].ReadOnly,
                "Fixed mapping was editable");
            form.Close();
        }
        Console.WriteLine("PASS: actual window current/draft values, mapping selectors, save/revert, failures, stale reads, busy close and fixed mappings");
    }

    private static void RecentFiles()
    {
        string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "canmonitor-mru-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(directory);
        try
        {
            string storage = System.IO.Path.Combine(directory, "history", "SDOMRU.txt");
            var first = new RecentDeviceFiles(storage);
            Require(first.Load().Length == 0, "Fresh MRU was not empty");
            string a = System.IO.Path.Combine(directory, "A & B.EDS");
            first.Add(a);
            Require(System.IO.File.Exists(storage) && new RecentDeviceFiles(storage).Load().Single() == a,
                "MRU was not persisted immediately");
            var second = new RecentDeviceFiles(storage);
            second.Load();
            string b = System.IO.Path.Combine(directory, "second.dcf");
            first.Add(b);
            string c = System.IO.Path.Combine(directory, "third.xdd");
            Require(second.Add(c).SequenceEqual(new[] { c, b, a }), "Stale editor overwrote newer history");
            Require(first.Add(a.ToUpperInvariant()).Length == 3, "Case variant duplicated recent path");
            for (int index = 0; index < 15; index++)
            {
                first.Add(System.IO.Path.Combine(directory, index + ".eds"));
            }
            Require(first.Load().Length == 10 && first.Load()[0].EndsWith("14.eds"), "MRU cap/order failed");
            System.IO.File.WriteAllLines(storage, new[] { "", "bad\0path.eds", a, a.ToLowerInvariant(), "unsupported.txt" });
            Require(first.Load().Single() == a, "Malformed/duplicate legacy history was not normalized");
            var blocked = new RecentDeviceFiles(directory);
            Require(blocked.Add(a).Length == 1 && blocked.Error != null, "MRU write error escaped or was hidden");
            Console.WriteLine("PASS: immediate MRU persistence, reload, stale editors, case deduplication, cap and storage failures");
        }
        finally
        {
            System.IO.Directory.Delete(directory, true);
        }
    }

    private static void ValueComparison()
    {
        Require(OdValueComparison.Different(DataType.INTEGER8, "-1", "0xFF") == false, "Signed hex mismatch");
        Require(OdValueComparison.Different(DataType.UNSIGNED32, "4294967295", "0xFFFFFFFF") == false, "Unsigned32 mismatch");
        Require(OdValueComparison.Different(DataType.UNSIGNED16, "200", "0x00C9") == true, "Numeric mismatch missing");
        Require(OdValueComparison.Different(DataType.UNSIGNED8, "256", "0") == null, "Invalid numeric source compared");
        Require(OdValueComparison.Different(DataType.UNSIGNED32, "$NODEID+0x180", "409") == null, "Unresolved expression compared");
        Require(OdValueComparison.Different(DataType.INTEGER16, null, "0") == null, "Missing current compared");
        Require(OdValueComparison.Different(DataType.INTEGER16, " **ERROR **", "0") == null, "Error compared as a value");
        Require(OdValueComparison.Different(DataType.INTEGER64, "-1", "0xFFFFFFFFFFFFFFFF") == false, "Signed64 mismatch");
        Require(OdValueComparison.Different(DataType.UNSIGNED64, "18446744073709551615", "0xFFFFFFFFFFFFFFFF") == false, "Unsigned64 mismatch");
        Require(OdValueComparison.Different(DataType.REAL32, "1.0", "1") == false, "Float formatting mismatch");
        Require(OdValueComparison.Different(DataType.VISIBLE_STRING, "", "") == false, "Empty string mismatch");
        Require(OdValueComparison.Different(DataType.VISIBLE_STRING, "a", "b") == true, "String mismatch missing");
        Require(OdValueComparison.Different(DataType.DOMAIN, "Domain Len = 4", "Domain Len = 4") == null, "Domain lengths treated as values");
        Require(OdValueComparison.Different(DataType.OCTET_STRING, "a b ", "0A 0x0B") == false, "Octet formatting mismatch");
        Require(OdValueComparison.Different(DataType.OCTET_STRING, "a b", "a c") == true, "Octet mismatch missing");
        Console.WriteLine("PASS: typed value comparisons, numeric representations, missing values, errors and expressions");
    }

    private static void IntegerDisplay()
    {
        foreach (DataType type in new[] { DataType.INTEGER8, DataType.INTEGER16, DataType.INTEGER32,
            DataType.UNSIGNED8, DataType.UNSIGNED16, DataType.UNSIGNED32 })
        {
            int width = OdIntegerDisplay.Width(type);
            bool signed = type == DataType.INTEGER8 || type == DataType.INTEGER16 || type == DataType.INTEGER32;
            long min = signed ? -(1L << (width - 1)) : 0;
            long max = signed ? (1L << (width - 1)) - 1 : (1L << width) - 1;
            foreach (long value in new[] { min, 0, max })
            {
                string number = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string hex = OdIntegerDisplay.Format(type, number, true);
                Require(hex.Length == 2 + width / 4, "Hex width was not preserved");
                Require(OdIntegerDisplay.Format(type, hex, false) == number, "Integer format round trip failed");
            }
            Require(OdIntegerDisplay.Format(type, "$NODEID+0x180", true) == "$NODEID+0x180", "Expression changed");
            Require(OdIntegerDisplay.Format(type, " **ERROR **", true) == " **ERROR **", "Error text changed");
        }
        Require(OdIntegerDisplay.Format(DataType.INTEGER8, "0xFF", false) == "-1", "Signed byte decoded as unsigned");
        Require(OdIntegerDisplay.Format(DataType.UNSIGNED32, "0xFFFFFFFF", false) == "4294967295", "Unsigned32 overflow");
        Require(OdIntegerDisplay.Format(DataType.REAL32, "1.25", true) == "1.25", "Non-integer display changed");
        Console.WriteLine("PASS: signed/unsigned integer bounds, fixed-width hex, expressions and round trips");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--demo")
        {
            Application.EnableVisualStyles();
            Application.Run(new PdoEditorForm(Dictionary(), 1, new Device()));
            return 0;
        }
        using (var timeout = new System.Threading.Timer(ignored => Environment.Exit(2), null, 60000, Timeout.Infinite))
        {
            try
            {
                RecentFiles();
                ValueComparison();
                IntegerDisplay();
                Model();
                Window();
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
        }
    }
}
