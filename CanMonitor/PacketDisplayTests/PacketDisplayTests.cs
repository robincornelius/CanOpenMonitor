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

    private static void ApplicationDocuments(string directory)
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
            form.Close();
        }
        if (File.Exists(filename)) { File.Delete(filename); }
        Require(subscriptions("pdoevent") == 0, "Real log remained subscribed after close");
        Console.WriteLine("PASS: real document capture isolation, clear race, live export and disposal");
        Console.WriteLine("PASS: actual docked documents unsubscribe and dispose their display workers");
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
        }, null, 90000, Timeout.Infinite))
        {
            try
            {
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
