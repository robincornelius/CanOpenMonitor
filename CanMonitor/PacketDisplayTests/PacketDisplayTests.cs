using System.Linq;
using CanMonitor.PacketDisplay;
using libCanopenSimple;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class PacketDisplayTests
{
    private static readonly DateTime Timestamp = new DateTime(2026, 1, 2, 3, 4, 5, 678);
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }
    private static void Await(Func<bool> condition, string message)
    {
        Require(SpinWait.SpinUntil(condition, 10000), message);
    }
    private static PacketLogRecord Record(int index, PacketLogKind kind = PacketLogKind.Pdo)
    {
        return new PacketLogRecord(kind, 0x181, Timestamp, BitConverter.GetBytes((ulong)index));
    }
    private static PacketLogRow Row(PacketLogRecord record)
    {
        return new PacketLogRow(record, new[] { "time", "PDO", "181", "", "00", "test" });
    }

    private static void LosslessBacklogAndIsolation()
    {
        var entered = new ManualResetEvent(false);
        var release = new ManualResetEvent(false);
        int formattingThread = 0;
        using (var pipeline = new PacketDisplayPipeline(record =>
        {
            formattingThread = Thread.CurrentThread.ManagedThreadId;
            if (record.Payload == 0)
            {
                entered.Set();
                release.WaitOne();
            }
            return Row(record);
        }))
        {
            pipeline.Enqueue(Record(0));
            Require(entered.WaitOne(2000), "Formatter did not start");
            Task producer = Task.Run(() =>
            {
                for (int index = 1; index < 100000; index++)
                {
                    pipeline.Enqueue(Record(index));
                }
            });
            Require(producer.Wait(5000), "Capture was blocked by slow decoding or absent UI");
            Require(pipeline.PendingInput == 99999, "Pending input was capped or discarded");
            Require(formattingThread != Thread.CurrentThread.ManagedThreadId, "Formatting ran on the calling thread");
            release.Set();
            Await(() => pipeline.PendingOutput == 100000, "Burst lost entries during formatting");
            for (int index = 0; index < 100000; index++)
            {
                PacketLogRow row;
                Require(pipeline.TryTake(out row) && row.Record.Payload == (ulong)index, "Burst reordered or lost packets");
            }
            Require(pipeline.PendingInput == 0 && pipeline.PendingOutput == 0 && pipeline.Errors == 0,
                "Pipeline counters did not drain");
        }
    }

    private static void MultipleProducers()
    {
        using (var pipeline = new PacketDisplayPipeline(Row))
        {
            var producers = new Task[4];
            for (int producer = 0; producer < producers.Length; producer++)
            {
                int source = producer;
                producers[source] = Task.Run(() =>
                {
                    for (int index = 0; index < 10000; index++)
                    {
                        pipeline.Enqueue(Record(source * 10000 + index));
                    }
                });
            }
            Require(Task.WaitAll(producers, 5000), "Concurrent producers blocked");
            Await(() => pipeline.PendingOutput == 40000, "Concurrent producers lost entries");
            int[] sequence = { -1, -1, -1, -1 };
            for (int index = 0; index < 40000; index++)
            {
                PacketLogRow row = null;
                Await(() => pipeline.TryTake(out row), "Concurrent output disappeared");
                int source = (int)row.Record.Payload / 10000;
                int number = (int)row.Record.Payload % 10000;
                Require(number == ++sequence[source], "Per-producer order changed or an entry duplicated");
            }
        }
    }

    private static void ClearAndDecoderErrors()
    {
        var entered = new ManualResetEvent(false);
        var release = new ManualResetEvent(false);
        using (var pipeline = new PacketDisplayPipeline(record =>
        {
            if (record.Payload == 0)
            {
                entered.Set();
                release.WaitOne();
            }
            if (record.Payload == 3)
            {
                throw new Exception("Injected decoder failure");
            }
            return Row(record);
        }))
        {
            pipeline.Enqueue(Record(0));
            Require(entered.WaitOne(2000), "Clear test formatter did not enter");
            pipeline.Enqueue(Record(1));
            object retired = typeof(PacketDisplayPipeline).GetField("session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pipeline);
            pipeline.Clear();
            object retiredInput = retired.GetType().GetField("Input", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(retired);
            Require((bool)retiredInput.GetType().GetProperty("IsEmpty").GetValue(retiredInput),
                "Clear retained a backlog through a blocked formatter");
            pipeline.Enqueue(Record(2));
            release.Set();
            Await(() => pipeline.PendingOutput == 1, "New session did not resume after clear");
            PacketLogRow row;
            Require(pipeline.TryTake(out row) && row.Record.Payload == 2, "Pre-clear work reappeared");
            pipeline.Enqueue(Record(3));
            pipeline.Enqueue(Record(4));
            Await(() => pipeline.PendingOutput == 2, "Decoder exception stopped the worker");
            Require(pipeline.TryTake(out row) && row.Record.Payload == 3 && row.Cells[5].Contains("Decode error"),
                "Failed decode did not preserve its packet as an error row");
            Require(pipeline.TryTake(out row) && row.Record.Payload == 4 && pipeline.Errors == 1
                && pipeline.LastError != null, "Decoder did not recover/report failure");
        }
        entered.Reset();
        release.Reset();
        var blocked = new PacketDisplayPipeline(record => { entered.Set(); release.WaitOne(); return Row(record); });
        blocked.Enqueue(Record(0));
        Require(entered.WaitOne(2000), "Disposal test did not start");
        Task stopping = Task.Run(() => blocked.Dispose());
        Require(stopping.Wait(1000), "Disposal blocked on a slow plugin");
        blocked.Enqueue(Record(1));
        release.Set();
    }

    private static void FiltersMetadataAndFormatting()
    {
        byte[] payload = { 1, 2 };
        var record = new PacketLogRecord(PacketLogKind.Nmt, 0, Timestamp, payload);
        payload[0] = 99;
        Require(record.CopyPayload()[0] == 1, "A producer could mutate retained packet data");
        int calls = 0;
        using (var pipeline = new PacketDisplayPipeline(item => { Interlocked.Increment(ref calls); return Row(item); }))
        {
            pipeline.Filter = new PacketDisplayFilter(nmt: false, emcy: false);
            pipeline.Enqueue(record);
            pipeline.Enqueue(new PacketLogRecord(PacketLogKind.Emcy, 0x81, Timestamp, new byte[8]));
            Await(() => pipeline.Processed == 2, "Filter did not process packets");
            Require(calls == 0 && pipeline.Filtered == 2, "Hidden traffic was formatted or used the wrong toggle");
            pipeline.Filter = new PacketDisplayFilter();
            pipeline.Enqueue(record);
            Await(() => pipeline.PendingOutput == 1, "Filter change did not take effect");
        }
        var heartbeat = new PacketDisplayFilter(heartbeat: false);
        Require(heartbeat.Matches(new PacketLogRecord(PacketLogKind.Heartbeat, 0x701, Timestamp, new byte[] { 0 }))
            && !heartbeat.Matches(new PacketLogRecord(PacketLogKind.Heartbeat, 0x701, Timestamp, new byte[] { 5 })),
            "Heartbeat suppression also suppressed boot records");
        var formatter = new PacketLogFormatter(cob => bytes => { bytes[0] = 255; return "decoded"; });
        PacketLogRecord pdo = Record(5);
        PacketLogRow decoded = formatter.Format(pdo);
        Require(decoded.Record.Payload == 5 && pdo.CopyPayload()[0] == 5 && decoded.Cells[4] == "0500000000000000",
            "A plugin could mutate the retained raw fields");
        Require(new PacketLogFormatter(cob => bytes => null).Format(pdo) == null,
            "Decoder-requested suppression was ignored");
    }

    private static void HistoryAndStateBuffer()
    {
        var history = new PacketHistory();
        for (int index = 0; index < 10000; index++)
        {
            history.Add(Row(Record(index)), 7);
        }
        Require(history.Count == 7 && history[0].Record.Payload == 9993 && history[6].Record.Payload == 9999,
            "Ring wrap/retention corrupted ordering");
        PacketLogRow[] saved = history.Snapshot();
        history.Add(Row(Record(10000)), null);
        history.RemoveExportedPrefix(new HashSet<PacketLogRow>(saved));
        Require(history.Count == 1 && history[0].Record.Payload == 10000, "Rotation removed unsaved new entries");
        history.Clear();
        for (int index = 0; index < 1000; index++) { history.Add(Row(Record(index)), 256); }
        for (int index = 1000; index < 2000; index++) { history.Add(Row(Record(index)), null); }
        PacketLogRow[] wrapped = history.Snapshot();
        for (int index = 0; index < wrapped.Length; index++)
        {
            Require(wrapped[index].Record.Payload == (ulong)(744 + index), "Growing a wrapped ring changed order");
        }
        history.Clear();
        for (int index = 0; index < 20000; index++)
        {
            history.Add(Row(Record(index)), null);
        }
        Require(history.Count == 20000 && history[19999].Record.Payload == 19999, "Unlimited retention silently capped history");
        Require(history.Snapshot()[0].Record.Payload == 0, "Growing the ring lost old entries");
        history.TrimTo(1000);
        Require(history.Count == 1000 && history[0].Record.Payload == 19000, "Changing retention broke the ring");
        history.Add(Row(Record(20000)), 0);
        Require(history.Count == 0, "Zero retained-line limit failed");

        var states = new LatestStateBuffer<int, int>();
        for (int index = 0; index < 100000; index++)
        {
            states.Put(index % 128, index);
        }
        Dictionary<int, int> latest = states.Take();
        Require(latest.Count == 128, "Heartbeat state queue grew with packet count");
        foreach (var pair in latest)
        {
            Require(pair.Value == ((99999 - pair.Key) / 128) * 128 + pair.Key, "NMT pane did not retain latest state");
        }
        states.Put(1, 123);
        Require(states.Take()[1] == 123 && latest.Count == 128, "UI snapshot shared the producer's dictionary");
        states.Put(2, 4);
        states.Clear();
        Require(states.Take().Count == 0, "State clear failed");
    }

    private static void SavedLogRoundTrip()
    {
        string filename = Path.Combine(Path.GetTempPath(), "canmonitor-display-" + Guid.NewGuid().ToString("N") + ".xml");
        CultureInfo previous = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var formatter = new PacketLogFormatter();
            PacketLogRow errorRow = null;
            using (var failed = new PacketDisplayPipeline(record => { throw new Exception("decode failed"); }))
            {
                failed.Enqueue(Record(7));
                Await(() => failed.TryTake(out errorRow), "Error row was not emitted");
            }
            var rows = new[] { formatter.Format(Record(123)),
                formatter.Format(new PacketLogRecord(PacketLogKind.Nmt, 0, Timestamp, new byte[] { 1, 2 })), errorRow };
            PacketLogFile.Save(filename, rows, new[] { "Timestamp", "Type", "COB", "Node", "Payload", "Info" }, "Black", "White");
            var loaded = new List<PacketLogRecord>();
            PacketLogFile.LoadXml(filename, loaded.Add, CancellationToken.None);
            Require(loaded.Count == 3 && loaded[2].Kind == PacketLogKind.Pdo && loaded[0].Payload == 123 && loaded[0].CobId == 0x181
                && loaded[0].Timestamp == Timestamp && loaded[1].Kind == PacketLogKind.Nmt,
                "Saved XML lost typed fields or culture-independent timestamps");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }

    private static void VirtualListMessagePump()
    {
        Exception failure = null;
        int pulses = 0;
        int drained = 0;
        int report = 0;
        var testTime = Stopwatch.StartNew();
        using (var form = new Form { Opacity = 0, ShowInTaskbar = false, Width = 800, Height = 400 })
        using (var list = new ListView { Dock = DockStyle.Fill, View = View.Details })
        using (var pipeline = new PacketDisplayPipeline(Row))
        using (var view = new VirtualPacketLog(list, pipeline))
        using (var update = new System.Windows.Forms.Timer { Interval = 10 })
        using (var pulse = new System.Windows.Forms.Timer { Interval = 15 })
        {
            for (int column = 0; column < 6; column++)
            {
                list.Columns.Add("column" + column, 100);
            }
            form.Controls.Add(list);
            pulse.Tick += (sender, args) => pulses++;
            update.Tick += (sender, args) =>
            {
                try
                {
                    drained += view.Drain(128, 4, 1024, pulses % 2 == 0);
                    if (drained / 5000 > report)
                    {
                        report = drained / 5000;
                        Console.WriteLine("Native display progress: " + drained + " rows in " + testTime.ElapsedMilliseconds + "ms");
                    }
                    Require(view.LastDrainCount <= 128, "UI work exceeded its row budget");
                    if (view.History.Count > 0)
                    {
                        Require(list.Items[view.History.Count - 1].Text == "time", "Virtual item retrieval failed");
                    }
                    if (drained == 20000)
                    {
                        Require(pulses >= 5 && view.History.Count == 1024, "Message pump starved or retention failed");
                        form.Close();
                    }
                }
                catch (Exception error) { failure = error; form.Close(); }
            };
            form.Shown += (sender, args) =>
            {
                Task.Run(() => { for (int index = 0; index < 20000; index++) { pipeline.Enqueue(Record(index)); } });
                update.Start();
                pulse.Start();
            };
            Application.Run(form);
        }
        if (failure != null)
        {
            throw failure;
        }
        Console.WriteLine("PASS: virtual WinForms list, bounded UI drain and " + pulses + " independent UI timer pulses");
    }

    private static void ApplicationDocuments(string directory, bool odEditorOnly = false)
    {
        string appDirectory = Path.GetFullPath(directory);
        ResolveEventHandler resolve = (sender, args) =>
        {
            string candidate = Path.Combine(appDirectory, new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += resolve;
        Assembly application = Assembly.LoadFrom(Path.Combine(appDirectory, "CanMonitor.exe"));
        var can = new libCanopenSimple.libCanopenSimple();
        BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        Func<string, int> subscriptions = name =>
        {
            Delegate callback = (Delegate)can.GetType().GetField(name, instance).GetValue(can);
            return callback == null ? 0 : callback.GetInvocationList().Length;
        };
        if (odEditorOnly)
        {
            SdoEditorCallbacks(application, can, appDirectory);
            return;
        }
        foreach (string document in new[] { "CanLogForm", "NMTDocument", "EmcyDocument" })
        {
            string eventName = document == "EmcyDocument" ? "emcyevent" : "nmtecevent";
            int before = subscriptions(eventName);
            object[] constructor = document == "CanLogForm" ? new object[] { can, null } : new object[] { can };
            using (var form = (Form)Activator.CreateInstance(application.GetType("CanMonitor." + document, true),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, constructor, null))
            {
                Require(subscriptions(eventName) == before + 1, "Document did not subscribe");
                form.CreateControl();
            }
            Require(subscriptions(eventName) == before, "Disposed document remained subscribed");
        }
        Type settingsType = application.GetType("CanMonitor.Properties.Settings", true);
        object settings = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        settingsType.GetProperty("showpdo").SetValue(settings, true);
        settingsType.GetProperty("AutoFileLog").SetValue(settings, false);
        Type formatterType = application.GetType("CanMonitor.PacketDisplay.PacketLogFormatter", true);
        var entered = new ManualResetEvent(false);
        var release = new ManualResetEvent(false);
        Func<ushort, Func<byte[], string>> decoder = cob => cob == 0x181
            ? (Func<byte[], string>)(bytes => { entered.Set(); release.WaitOne(); return "decoded"; }) : null;
        object formatter = Activator.CreateInstance(formatterType, instance, null, new object[] { decoder, null }, null);
        Type logType = application.GetType("CanMonitor.CanLogForm", true);
        string filename = Path.Combine(Path.GetTempPath(), "canmonitor-form-export-" + Guid.NewGuid().ToString("N") + ".xml");
        using (var form = (Form)Activator.CreateInstance(logType, instance, null, new object[] { can, formatter }, null))
        {
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.Show();
            var callback = (libCanopenSimple.libCanopenSimple.PDOEvent)can.GetType().GetField("pdoevent", instance).GetValue(can);
            Task producer = Task.Run(() =>
            {
                var batch = new canpacket[5000];
                for (int index = 0; index < batch.Length; index++)
                {
                    batch[index] = new canpacket { cob = 0x181, len = 1, data = new byte[] { 1 } };
                }
                callback(batch, Timestamp);
            });
            Require(producer.Wait(2000) && entered.WaitOne(2000), "Real document blocked capture during plugin decoding");
            logType.GetMethod("clearlist").Invoke(form, null);
            callback(new[] { new canpacket { cob = 0x182, len = 1, data = new byte[] { 2 } } }, Timestamp);
            release.Set();
            var list = (ListView)logType.GetField("listView1", instance).GetValue(form);
            Require(list.FullRowSelect, "Packet context actions require the whole row to be selectable");
            Require(list.Top >= 60, "Filter controls cover the packet column headers");
            PumpUntil(() => list.VirtualListSize == 1, "Real document failed to drain after Clear");
            Require(list.Items[0].SubItems[2].Text == "182", "Real document resurrected an old decoded row");
            logType.GetMethod("dosave").Invoke(form, new object[] { filename });
            callback(new[] { new canpacket { cob = 0x183, len = 1, data = new byte[] { 3 } } }, Timestamp);
            Task saving = (Task)logType.GetProperty("PendingSave", instance).GetValue(form);
            PumpUntil(() => saving.IsCompleted && list.VirtualListSize == 2, "Export stalled live display");
            Require(!saving.IsFaulted, "Real document export failed");
            var loaded = new List<PacketLogRecord>();
            PacketLogFile.LoadXml(filename, loaded.Add, CancellationToken.None);
            Require(loaded.Count == 1 && loaded[0].CobId == 0x182, "Export snapshot mixed in later live traffic");
            MethodInfo apply = logType.GetMethod("ApplyDisplayFilter", instance);
            Require((bool)apply.Invoke(form, new object[] { "type == PDO and cobid == 0x182" }), "Document rejected valid filter");
            PumpUntil(() => list.VirtualListSize == 1, "Document failed to filter retained rows");
            Require(list.Items[0].SubItems[2].Text == "182", "Document displayed wrong filtered row");
            Require(!(bool)apply.Invoke(form, new object[] { "cobid === 0x183" }), "Document accepted invalid filter");
            Require(list.VirtualListSize == 1, "Invalid expression replaced active filter");
            settingsType.GetProperty("showpdo").SetValue(settings, false);
            PumpUntil(() => list.VirtualListSize == 0, "Protocol checkbox failed to narrow view");
            settingsType.GetProperty("showpdo").SetValue(settings, true);
            PumpUntil(() => list.VirtualListSize == 1, "Protocol checkbox failed to restore history");
            object display = logType.GetField("display", instance).GetValue(form);
            object selected = display.GetType().GetMethod("RowAt", instance).Invoke(display, new object[] { 0 });
            logType.GetField("contextRow", instance).SetValue(form, selected);
            list.ContextMenuStrip.Items[1].PerformClick();
            Require(((TextBox)logType.GetField("filterText", instance).GetValue(form)).Text.Contains("not (type == PDO and cobid == 0x182)"),
                "Context action did not append exclusion");
            PumpUntil(() => list.VirtualListSize == 0, "Context exclusion failed");
            apply.Invoke(form, new object[] { "cobid == 0x183" });
            PumpUntil(() => list.VirtualListSize == 1, "Replacement filter failed");
            logType.GetMethod("dosave").Invoke(form, new object[] { filename });
            saving = (Task)logType.GetProperty("PendingSave", instance).GetValue(form);
            PumpUntil(() => saving.IsCompleted, "Filtered export stalled");
            loaded.Clear();
            PacketLogFile.LoadXml(filename, loaded.Add, CancellationToken.None);
            Require(loaded.Count == 1 && loaded[0].CobId == 0x183, "Manual export ignored display filter");
            apply.Invoke(form, new object[] { "" });
            PumpUntil(() => list.VirtualListSize == 2, "Clearing expression failed to restore retained history");
            form.Close();
        }
        if (File.Exists(filename)) { File.Delete(filename); }
        Require(subscriptions("pdoevent") == 0, "Real log remained subscribed after close");
        Console.WriteLine("PASS: real document capture isolation, clear race, live export and disposal");
        Console.WriteLine("PASS: actual docked documents unsubscribe and dispose their display workers");
        Console.WriteLine("PASS: actual log expression Apply, invalid input, checkboxes, context exclusion and filtered export");
        SdoEditorCallbacks(application, can, appDirectory);
    }

    private static void SdoEditorCallbacks(Assembly application, libCanopenSimple.libCanopenSimple can, string directory)
    {
        Assembly plugin = Assembly.LoadFrom(Path.Combine(directory, "plugins", "SDOEditorPlugin.dll"));
        Type editorType = plugin.GetType("SDOEditorPlugin.SDOEditor", true);
        BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var editor = (Form)Activator.CreateInstance(editorType, new object[] { can });
        try
        {
            // Create handles without showing/loading an OD or touching hardware
            // or the editor's recent-file settings.
            IntPtr handle = editor.Handle;
            var menu = (MenuStrip)editorType.GetField("menuStrip1", instance).GetValue(editor);
            Require(menu.Items.Cast<ToolStripItem>().Any(item => item.Text == "Edit PDOs..."),
                "Device OD Editor PDO menu is missing");
            var list = (ListView)editorType.GetField("listView1", instance).GetValue(editor);
            handle = list.Handle;
            var read = (Button)editorType.GetField("button_read", instance).GetValue(editor);
            var transfer = new SDO(can, 1, 0x2000, 0, SDO.direction.SDO_READ, null, null);
            foreach (string callback in new[] { "gotit", "upsucc" })
            {
                read.Enabled = false;
                // The UI deliberately waits here. Synchronous Invoke in the
                // completion callback reproduces the original lock cycle.
                Task worker = Task.Run(() => editorType.GetMethod(callback, instance).Invoke(editor, new object[] { transfer }));
                Require(worker.Wait(1000), "OD Editor callback waited synchronously for blocked UI: " + callback);
                PumpUntil(() => read.Enabled, "OD Editor posted completion was not processed: " + callback);
            }
            Type helperType = editorType.GetNestedType("sdocallbackhelper", BindingFlags.Public);
            object helper = Activator.CreateInstance(helperType);
            var failed = new SDO(can, 7, 0x2000, 0, SDO.direction.SDO_WRITE, null, new byte[] { 1 });
            failed.state = SDO.SDO_STATE.SDO_ERROR;
            failed.expitideddata = 0x06010002;
            helperType.GetField("sdo").SetValue(helper, failed);
            var row = new ListViewItem(new[] { "2000", "00", "test", "", "", "old" }) { Tag = helper };
            list.Items.Add(row);
            object writes = editorType.GetField("pendingWrites", instance).GetValue(editor);
            writes.GetType().GetMethod("Add").Invoke(writes, new object[] { failed });
            Require(Task.Run(() => editorType.GetMethod("upsucc", instance).Invoke(editor, new object[] { failed })).Wait(1000),
                "OD Editor write completion blocked UI");
            PumpUntil(() => row.SubItems[5].Text == "Write failed: 0x06010002", "OD Editor hid write failure");
            Require((int)writes.GetType().GetProperty("Count").GetValue(writes) == 0, "Write failure stranded refresh pause");

            Type odType = Assembly.LoadFrom(Path.Combine(directory, "libEDSsharp.dll")).GetType("libEDSsharp.ODentry", true);
            helperType.GetField("od").SetValue(helper, Activator.CreateInstance(odType,
                new object[] { "Test", (ushort)0x2000, (byte)0 }));
            var written = new SDO(can, 7, 0x2000, 0, SDO.direction.SDO_WRITE, null, new byte[] { 1 });
            written.state = SDO.SDO_STATE.SDO_FINISHED;
            helperType.GetField("sdo").SetValue(helper, written);
            row.Tag = helper;
            ((NumericUpDown)editorType.GetField("numericUpDown_node", instance).GetValue(editor)).Value = 9;
            Task.Run(() => editorType.GetMethod("upsucc", instance).Invoke(editor, new object[] { written })).Wait();
            PumpUntil(() => !ReferenceEquals(helperType.GetField("sdo").GetValue(row.Tag), written), "Write read-back was not queued");
            Require(((SDO)helperType.GetField("sdo").GetValue(row.Tag)).node == 7, "Write read-back used a changed node selection");
            DcfPdoEntries(editorType, odType);
            IntegerColumns(editorType, odType, can);
            RecentFileMenu(editorType, can);
            editor.Dispose();
            Require(Task.Run(() => editorType.GetMethod("gotit", instance).Invoke(editor, new object[] { transfer })).Wait(1000),
                "Closed OD Editor callback blocked");
        }
        finally
        {
            editor.Dispose();
        }
        Console.WriteLine("PASS: actual OD Editor callbacks post without blocking the UI and tolerate disposal");
    }

    private static void RecentFileMenu(Type editorType, libCanopenSimple.libCanopenSimple can)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        string directory = Path.Combine(Path.GetTempPath(), "canmonitor-mru-menu-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string filename = Path.Combine(directory, "A & B.EDS");
            File.WriteAllText(filename, "[FileInfo]\nFileName=example.eds\n[DeviceInfo]\nProductName=MRU regression device\n"
                + "[MandatoryObjects]\nSupportedObjects=1\n1=0x1000\n[1000]\nParameterName=Device type\nObjectType=0x7\n"
                + "DataType=0x7\nAccessType=ro\nDefaultValue=0\nPDOMapping=0\n");
            using (var first = (Form)Activator.CreateInstance(editorType, new object[] { can }))
            using (var second = (Form)Activator.CreateInstance(editorType, new object[] { can }))
            {
                editorType.GetField("appdatafolder", flags).SetValue(first, directory);
                editorType.GetField("appdatafolder", flags).SetValue(second, directory);
                editorType.GetMethod("LoadRecentFiles", flags).Invoke(first, null);
                var menu = (ToolStripMenuItem)editorType.GetField("mnuRecentlyUsed", flags).GetValue(first);
                Require(menu.DropDownItems.Count == 1 && !menu.DropDownItems[0].Enabled, "Empty history lacks placeholder");
                editorType.GetMethod("addtoMRU", flags).Invoke(first, new object[] { filename });
                Require(File.ReadAllLines(Path.Combine(directory, "SDOMRU.txt")).Single() == filename, "Menu did not save immediately");
                editorType.GetMethod("LoadRecentFiles", flags).Invoke(second, null);
                var restored = (ToolStripMenuItem)editorType.GetField("mnuRecentlyUsed", flags).GetValue(second);
                Require(restored.DropDownItems.Count == 1 && restored.DropDownItems[0].Enabled, "Reopened MRU menu is empty");
                ((CheckBox)editorType.GetField("checkBox_useronly", flags).GetValue(second)).Checked = false;
                ((ComboBox)editorType.GetField("comboBoxtype", flags).GetValue(second)).SelectedItem = "ALL";
                restored.DropDownItems[0].PerformClick();
                Require((string)editorType.GetField("filename", flags).GetValue(second) == filename, "Recent menu did not open the exact path");
                var list = (ListView)editorType.GetField("listView1", flags).GetValue(second);
                Require(list.Items.Count == 1 && list.Items[0].SubItems[2].Text == "Device type", "Recent file did not populate OD rows: " + list.Items.Count + " / " + string.Join(",", list.Items.Cast<ListViewItem>().Select(item => item.SubItems[2].Text)) );
                File.Delete(filename);
                editorType.GetMethod("LoadRecentFiles", flags).Invoke(second, null);
                Require(!restored.DropDownItems[0].Enabled && restored.DropDownItems[0].ToolTipText.Contains("unavailable"),
                    "Missing recent file was not marked unavailable");
            }
            Console.WriteLine("PASS: actual MRU menu reload, exact ampersand path click, OD loading and missing-file state");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void IntegerColumns(Type editorType, Type odType, libCanopenSimple.libCanopenSimple can)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        using (var editor = (Form)Activator.CreateInstance(editorType, new object[] { can }))
        {
            IntPtr handle = editor.Handle;
            var list = (ListView)editorType.GetField("listView1", flags).GetValue(editor);
            handle = list.Handle;
            object entry = Activator.CreateInstance(odType, new object[] { "Signed byte", (ushort)0x2001, (byte)0 });
            odType.GetField("datatype").SetValue(entry, Enum.Parse(odType.GetField("datatype").FieldType, "INTEGER8"));
            odType.GetField("defaultvalue").SetValue(entry, "0xFF");
            odType.GetField("actualvalue").SetValue(entry, "0x80");
            editorType.GetMethod("addtolist", flags).Invoke(editor, new object[] { entry, true, true });
            ListViewItem row = list.Items[0];
            Require(row.SubItems[4].Text == "-1" && row.SubItems[6].Text == "-128", "Initial integer columns disagree");
            var transfer = new SDO(can, 1, 0x2001, 0, SDO.direction.SDO_READ, null, null);
            transfer.state = SDO.SDO_STATE.SDO_FINISHED;
            transfer.expitideddata = 255;
            object helper = row.Tag;
            helper.GetType().GetField("sdo").SetValue(helper, transfer);
            row.Tag = helper;
            editorType.GetMethod("gotit", flags).Invoke(editor, new object[] { transfer });
            PumpUntil(() => row.SubItems[5].Text == "-1", "Signed current byte display incorrect");
            var format = (ToolStripComboBox)editorType.GetField("integerFormat", flags).GetValue(editor);
            format.SelectedIndex = 1;
            Require(row.SubItems[4].Text == "0xFF" && row.SubItems[5].Text == "0xFF" && row.SubItems[6].Text == "0x80",
                "Page hex format did not update every value column");
            row.Selected = true;
            list.ContextMenuStrip.Items[1].PerformClick();
            Require(row.SubItems[4].Text == "-1" && row.SubItems[5].Text == "-1" && row.SubItems[6].Text == "-128",
                "Per-row decimal override failed");
            list.ContextMenuStrip.Items[0].PerformClick();
            Require(row.SubItems[5].Text == "0xFF", "Use page default failed");
            list.ContextMenuStrip.Items[1].PerformClick();
            format.SelectedIndex = 0;
            format.SelectedIndex = 1;
            Require(row.SubItems[5].Text == "0xFF", "Page switch did not reset row override");
            Require((string)odType.GetField("defaultvalue").GetValue(entry) == "0xFF"
                && (string)odType.GetField("actualvalue").GetValue(entry) == "0x80", "Display changed source values");
            Require(transfer.expitideddata == 255, "Display mutated the SDO payload");
            object write = editorType.GetMethod("CreateParameterWrite", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { row.Tag, row.SubItems[5].Text });
            byte[] data = (byte[])write.GetType().GetProperty("Data").GetValue(write);
            Require(data.Length == 1 && data[0] == 255, "Hexadecimal signed value cannot be edited losslessly");
            Action<string, string, uint, string> compare = (baseline, dcf, current, expected) =>
            {
                odType.GetField("defaultvalue").SetValue(entry, baseline);
                odType.GetField("actualvalue").SetValue(entry, dcf);
                editorType.GetMethod("adddcfvalue", flags).Invoke(editor, new object[] { entry });
                transfer.expitideddata = current;
                row.SubItems[5].Text = "waiting";
                editorType.GetMethod("gotit", flags).Invoke(editor, new object[] { transfer });
                PumpUntil(() => row.SubItems[5].Text != "waiting", "Comparison read not processed");
                Require(row.SubItems[7].Text == expected, "Wrong simultaneous differences: " + row.SubItems[7].Text);
            };
            compare("0", "0x00", 0, "Comparable values match");
            compare("0", "0", 1, "Current ≠ Default; Current ≠ DCF");
            compare("0", "1", 0, "Current ≠ DCF; DCF ≠ Default");
            compare("0", "1", 1, "Current ≠ Default; DCF ≠ Default");
            compare("0", "1", 2, "Current ≠ Default; Current ≠ DCF; DCF ≠ Default");
            Require(row.SubItems[5].BackColor != list.BackColor && row.SubItems[6].BackColor != list.BackColor,
                "Independent Current/DCF highlights missing");
            transfer.state = SDO.SDO_STATE.SDO_ERROR;
            editorType.GetMethod("gotit", flags).Invoke(editor, new object[] { transfer });
            PumpUntil(() => row.SubItems[5].Text.Contains("ERROR"), "Read error not displayed");
            Require(row.SubItems[7].Text == "DCF ≠ Default", "Read error retained stale Current comparisons");
            transfer.state = SDO.SDO_STATE.SDO_FINISHED;
            compare("0", "", 1, "Current ≠ Default");
            compare("0", "0", 0, "Comparable values match");
            Require(row.SubItems[5].BackColor == list.BackColor && row.SubItems[6].BackColor == list.BackColor,
                "Matching values retained stale highlights");
            format.SelectedIndex = 0;
            Require(row.SubItems[7].Text == "Comparable values match", "Display base changed comparison result");
            Console.WriteLine("PASS: simultaneous three-way comparisons, missing DCF, read errors and cleared highlights");
        }
        Console.WriteLine("PASS: integer Default/Current/DCF columns, signed reads, page and row formats preserve values");
    }

    private static void DcfPdoEntries(Type editorType, Type odType)
    {
        Type edsType = odType.Assembly.GetType("libEDSsharp.EDSsharp", true);
        object dcf = Activator.CreateInstance(edsType);
        object dictionary = edsType.GetField("ods").GetValue(dcf);
        Func<ushort, byte, string, string, object> entry = (index, sub, type, value) =>
        {
            object od = Activator.CreateInstance(odType, new object[] { "Test", index, (byte)0 });
            odType.GetField("datatype").SetValue(od, Enum.Parse(odType.GetField("datatype").FieldType, type));
            odType.GetField("accesstype").SetValue(od, Enum.Parse(odType.GetField("accesstype").FieldType, "rw"));
            odType.GetField("actualvalue").SetValue(od, value);
            return od;
        };
        object mapping = entry(0x1A00, 0, "UNSIGNED32", "");
        odType.GetMethod("addsubobject").Invoke(mapping, new object[] { (byte)0, entry(0x1A00, 0, "UNSIGNED8", "2") });
        odType.GetMethod("addsubobject").Invoke(mapping, new object[] { (byte)1, entry(0x1A00, 1, "UNSIGNED32", "0x20000010") });
        dictionary.GetType().GetMethod("Add").Invoke(dictionary, new object[] { (ushort)0x1A00, mapping });
        object communication = entry(0x1800, 0, "UNSIGNED32", "");
        odType.GetMethod("addsubobject").Invoke(communication, new object[] { (byte)3, entry(0x1800, 3, "UNSIGNED16", "200") });
        dictionary.GetType().GetMethod("Add").Invoke(dictionary, new object[] { (ushort)0x1800, communication });
        var writes = (IEnumerable<SdoParameterWrite>)editorType.GetMethod("BuildDcfWrites", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new[] { dcf });
        SdoParameterWrite[] values = new List<SdoParameterWrite>(writes).ToArray();
        Require(Array.Exists(values, write => write.Index == 0x1A00 && write.Subindex == 0 && write.Data.Length == 1 && write.Data[0] == 2),
            "DCF planning omitted hidden mapping count");
        Require(Array.Exists(values, write => write.Index == 0x1A00 && write.Subindex == 1 && BitConverter.ToUInt32(write.Data, 0) == 0x20000010)
            && Array.Exists(values, write => write.Index == 0x1800 && write.Subindex == 3 && BitConverter.ToUInt16(write.Data, 0) == 200),
            "DCF planning lost mapping/communication parameters or their widths");
        Console.WriteLine("PASS: actual OD Editor DCF planning includes mapping count and combined communication/mapping values");
    }

    private static void Demo(string directory)
    {
        string applicationDirectory = Path.GetFullPath(directory);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string candidate = Path.Combine(applicationDirectory, new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        };
        Assembly application = Assembly.LoadFrom(Path.Combine(applicationDirectory, "CanMonitor.exe"));
        Type settingsType = application.GetType("CanMonitor.Properties.Settings", true);
        object settings = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        settingsType.GetProperty("showpdo").SetValue(settings, true);
        settingsType.GetProperty("limitlines").SetValue(settings, true);
        settingsType.GetProperty("linelimit").SetValue(settings, 1000);
        settingsType.GetProperty("autoscroll").SetValue(settings, true);
        settingsType.GetProperty("AutoFileLog").SetValue(settings, false);
        var can = new libCanopenSimple.libCanopenSimple();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Console.WriteLine("Demo: creating log view");
        using (var form = (Form)Activator.CreateInstance(application.GetType("CanMonitor.CanLogForm", true), flags,
            null, new object[] { can, null }, null))
        {
            Console.WriteLine("Demo: log view created");
            form.Text = "CanMonitor packet display load test - synthetic traffic, no hardware";
            form.Width = 1100;
            form.Height = 650;
            int run = 1;
            var callback = (libCanopenSimple.libCanopenSimple.PDOEvent)can.GetType().GetField("pdoevent", flags).GetValue(can);
            var producer = new Thread(() =>
            {
                ulong sequence = 0;
                while (Volatile.Read(ref run) != 0)
                {
                    var batch = new canpacket[500];
                    for (int index = 0; index < batch.Length; index++)
                    {
                        batch[index] = new canpacket { cob = (ushort)(0x181 + sequence % 16), len = 8,
                            data = BitConverter.GetBytes(sequence++) };
                    }
                    callback(batch, DateTime.Now);
                    Thread.Sleep(25);
                }
            }) { IsBackground = true };
            using (var host = new Form { Text = form.Text, Width = 1100, Height = 650 })
            {
                Type panelType = form.GetType().GetProperty("DockPanel").PropertyType;
                var panel = (Control)Activator.CreateInstance(panelType);
                panel.Dock = DockStyle.Fill;
                host.Controls.Add(panel);
                Assembly themeAssembly = Assembly.LoadFrom(Path.Combine(applicationDirectory, "WeifenLuo.WinFormsUI.Docking.ThemeVS2015.dll"));
                object theme = Activator.CreateInstance(themeAssembly.GetType("WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme", true));
                panelType.GetProperty("Theme").SetValue(panel, theme);
                Type stateType = panelType.Assembly.GetType("WeifenLuo.WinFormsUI.Docking.DockState", true);
                form.GetType().GetMethod("Show", new[] { panelType, stateType }).Invoke(form,
                    new[] { (object)panel, Enum.Parse(stateType, "Document") });
                host.Shown += (sender, args) =>
                {
                    Console.WriteLine("Demo: producing synthetic traffic");
                    producer.Start();
                };
                host.FormClosed += (sender, args) => Interlocked.Exchange(ref run, 0);
                Application.Run(host);
            }
        }
    }

    private static void PumpUntil(Func<bool> condition, string message)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition())
        {
            Require(timeout.ElapsedMilliseconds < 5000, message);
            Application.DoEvents();
            Thread.Sleep(1);
        }
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--demo")
        {
            Demo(args[1]);
            return 0;
        }
        using (var deadline = new System.Threading.Timer(state =>
        {
            Console.Error.WriteLine("FAIL: Display regression deadline exceeded");
            Environment.Exit(2);
        }, null, 180000, Timeout.Infinite))
        {
            try
            {
                if (args.Length == 3 && args[0] == "--bugs")
                {
                    BugRegressionTests.Run(args[1], Path.GetFullPath(args[2]));
                    return 0;
                }
                if (args.Length == 2 && args[0] == "--od-editor")
                {
                    ApplicationDocuments(args[1], true);
                    return 0;
                }
                LosslessBacklogAndIsolation();
                Console.WriteLine("PASS: 100000 ordered entries preserved while formatter and UI are blocked");
                MultipleProducers();
                Console.WriteLine("PASS: concurrent producers preserve all entries and each producer's order");
                ClearAndDecoderErrors();
                Console.WriteLine("PASS: clear generation, decoder failure recovery and nonblocking disposal");
                FiltersMetadataAndFormatting();
                Console.WriteLine("PASS: filter-before-format, independent toggles and immutable metadata");
                HistoryAndStateBuffer();
                Console.WriteLine("PASS: ring wrap, unlimited retention, rotation and bounded NMT state");
                SavedLogRoundTrip();
                Console.WriteLine("PASS: saved XML round trip under a different locale");
                VirtualListMessagePump();
                FilterExpressionTests.Run();
                if (args.Length == 2 && args[0] == "--application")
                {
                    ApplicationDocuments(args[1]);
                }
                return 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        }
    }
}
