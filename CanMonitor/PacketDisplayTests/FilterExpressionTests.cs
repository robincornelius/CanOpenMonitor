using CanMonitor.PacketDisplay;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

internal static class FilterExpressionTests
{
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    private static PacketLogRow Row(PacketLogKind kind, ushort cob, params byte[] bytes)
    {
        var record = new PacketLogRecord(kind, cob, DateTime.UtcNow, bytes);
        return new PacketLogRow(record, new[] { "time", record.TypeName, cob.ToString("X3"), "", record.PayloadText(), "SDO abort: test" });
    }

    internal static void Run()
    {
        var pdo = Row(PacketLogKind.Pdo, 0x182, 0x80, 0x12, 0x34);
        foreach (string expression in new[] {
            "", "type=PDO and COBID=0x182", "TYPE == \"pdo\" && cob == 386",
            "nodeid == 2 and len == 3", "length >= 3 and data[0] == 0x80 and data[2] != 0",
            "not type == SDO", "!(cobid < 0x182 or cobid > 0x182)",
            "type == PDO or type == SDO and cobid == 0", "payload contains \"8012\"",
            "info contains \"ABORT\"", "data[7] != 0 or type == PDO" })
        {
            Require(PacketFilterExpression.Compile(expression)(pdo), "Valid expression failed: " + expression);
        }
        foreach (string expression in new[] { "type == SDO", "node != 2", "data[3] != 0", "sdo.index != 0", "emcy.code == 0x1280" })
        {
            Require(!PacketFilterExpression.Compile(expression)(pdo), "Absent/wrong field matched: " + expression);
        }
        Require(PacketFilterExpression.Compile("not (data[7] == 0)")(pdo), "Absent byte negation failed");
        Require(PacketFilterExpression.Compile("node == 45")(Row(PacketLogKind.Nmt, 0, 1, 45)), "NMT target node failed");
        Require(PacketFilterExpression.Compile("type == heartbeat")(Row(PacketLogKind.Heartbeat, 0x702, 0)), "Type alias failed");
        Require(PacketFilterExpression.Compile("emcy.code == 0x1234")(Row(PacketLogKind.Emcy, 0x82, 0x34, 0x12)), "EMCY endian failed");
        var sdo = Row(PacketLogKind.Sdo, 0x602, 0x40, 0x00, 0x20, 3);
        Require(PacketFilterExpression.Compile("sdo.index == 0x2000 and sdo.subindex == 3")(sdo), "SDO request fields failed");
        Require(PacketFilterExpression.Compile("sdo.index == 0x2000")(Row(PacketLogKind.Sdo, 0x582, 0x60, 0, 0x20, 3)), "SDO response fields failed");
        Require(!PacketFilterExpression.Compile("sdo.index == 0x2000")(Row(PacketLogKind.Sdo, 0x582, 0, 0, 0x20, 3)), "Segment bytes treated as index");

        foreach (string invalid in new[] { "unknown == 1", "type == FOO", "cobid == 0x800", "data[8] == 0", "data[-1] == 0",
            "length == 9", "node == 128", "type contains PDO", "info contains abort", "info == \"unfinished",
            "cobid =", "type == PDO and", "(type == PDO", "type == PDO)", "type == PDO garbage",
            "data[0] == 256", "len == -1", "cobid == 0xFFFFFFFFFFFFFFFF", "len == \"1\"",
            "type == PDO or", "\"type\" == PDO", new string('!', 65) + "type == PDO", new string(' ', 4097) })
        {
            bool rejected = false;
            try
            {
                PacketFilterExpression.Compile(invalid);
            }
            catch (FormatException)
            {
                rejected = true;
            }
            Require(rejected, "Invalid expression accepted: " + invalid);
        }
        string exclusion = PacketFilterExpression.Combine("node == 2 or node == 3", PacketFilterExpression.PacketCondition(pdo.Record), true);
        var exclude = PacketFilterExpression.Compile(exclusion);
        Require(!exclude(pdo) && exclude(Row(PacketLogKind.Pdo, 0x183, 0))
            && exclude(Row(PacketLogKind.Sdo, 0x602, 0)), "Context exclusion composition/precedence failed");
        Projection();
        Console.WriteLine("PASS: reversible expression projection, retention and Clear");
        FilteredMessagePump();
        Console.WriteLine("PASS: filter grammar, fields, invalid input, context expressions and retained-history projection");
    }

    private static void Projection()
    {
        using (var list = new ListView { View = View.Details })
        using (var pipeline = new PacketDisplayPipeline(record => new PacketLogRow(record,
            new[] { "time", "PDO", record.CobId.ToString("X3"), "", record.PayloadText(), "" })))
        using (var view = new VirtualPacketLog(list, pipeline))
        {
            list.Columns.Add("time");
            for (int index = 0; index < 10000; index++)
            {
                pipeline.Enqueue(Row(PacketLogKind.Pdo, (ushort)(0x181 + index % 2), (byte)index).Record);
            }
            Require(SpinWait.SpinUntil(() => pipeline.PendingOutput == 10000, 5000), "Projection setup stalled");
            while (pipeline.PendingOutput > 0)
            {
                view.Drain(128, 4, null, false);
            }
            view.SetFilter(PacketFilterExpression.Compile("not (type == PDO and cobid == 0x182)"));
            view.Drain(128, 4, null, false);
            Require(view.IsRebuilding && view.History.Count == 10000, "Filter rebuild was not bounded or destroyed history");
            Finish(view, null);
            Require(view.VisibleCount == 5000 && view.VisibleSnapshot().Length == 5000, "Historical exclusion failed");
            for (int index = 0; index < view.VisibleCount; index++)
            {
                Require(view.RowAt(index).Record.CobId == 0x181, "Projection included hidden COB-ID");
            }
            view.SetFilter(PacketFilterExpression.Compile(""));
            Finish(view, null);
            Require(view.VisibleCount == 10000, "Clear filter did not restore excluded history");
            view.SetFilter(PacketFilterExpression.Compile("cobid == 0x182"));
            Finish(view, 1000);
            Require(view.History.Count == 1000 && view.VisibleCount == 500, "Retention invalidated projection");
            for (int index = 0; index < 5000; index++)
            {
                pipeline.Enqueue(Row(PacketLogKind.Pdo, (ushort)(0x181 + index % 2), (byte)index).Record);
            }
            Require(SpinWait.SpinUntil(() => pipeline.PendingOutput == 5000, 5000), "Live projection setup stalled");
            while (pipeline.PendingOutput > 0 || view.IsRebuilding)
            {
                view.Drain(128, 4, 1000, false);
            }
            Require(view.History.Count == 1000 && view.VisibleCount == 500, "Live retention/wrap failed");
            var exported = new HashSet<PacketLogRow>(view.History.Snapshot());
            view.History.RemoveExportedPrefix(exported, 128);
            view.Drain(128, 4, 1000, false);
            Require(view.History.Count == 872 && view.VisibleCount == 436, "Export retirement invalidated projection");
            view.Clear();
            Require(view.VisibleCount == 0 && !view.IsRebuilding, "Clear left stale projection");
            pipeline.Enqueue(Row(PacketLogKind.Pdo, 0x182, 0).Record);
            Require(SpinWait.SpinUntil(() => pipeline.PendingOutput == 1, 5000), "Post-clear setup stalled");
            view.Drain(128, 4, null, false);
            Finish(view, null);
            Require(view.VisibleCount == 1 && view.RowAt(0).Record.CobId == 0x182, "Active filter failed after Clear");
        }
    }

    private static void Finish(VirtualPacketLog view, int? limit)
    {
        int batches = 0;
        do
        {
            view.Drain(128, 4, limit, false);
            Require(++batches < 10000, "Filter rebuild stalled");
        } while (view.IsRebuilding);
    }

    private static void FilteredMessagePump()
    {
        int pulses = 0;
        int drained = 0;
        int phase = 0;
        Exception failure = null;
        using (var form = new Form { Opacity = 0, ShowInTaskbar = false })
        using (var list = new ListView { Dock = DockStyle.Fill, View = View.Details })
        using (var pipeline = new PacketDisplayPipeline(record => new PacketLogRow(record,
            new[] { "time", "PDO", record.CobId.ToString("X3"), "", "", "" })))
        using (var view = new VirtualPacketLog(list, pipeline))
        using (var timer = new System.Windows.Forms.Timer { Interval = 10 })
        using (var pulse = new System.Windows.Forms.Timer { Interval = 15 })
        {
            list.Columns.Add("time");
            form.Controls.Add(list);
            pulse.Tick += (sender, args) => pulses++;
            timer.Tick += (sender, args) =>
            {
                try
                {
                    drained += view.Drain(128, 4, 1024, false);
                    Require(view.LastDrainCount <= 128, "Filtering exceeded input row budget");
                    if (phase == 0 && drained >= 512)
                    {
                        view.SetFilter(PacketFilterExpression.Compile("cobid == 0x181"));
                        phase = 1;
                    }
                    else if (phase == 1 && drained == 20000 && !view.IsRebuilding)
                    {
                        Require(view.VisibleCount == 512 && view.History.Count == 1024, "Filter under load violated retention");
                        view.SetFilter(PacketFilterExpression.Compile("cobid == 0x182"));
                        phase = 2;
                    }
                    else if (phase == 2 && !view.IsRebuilding)
                    {
                        Require(view.VisibleCount == 512 && pulses >= 5, "Re-filtering stalled message pump or lost rows");
                        Require(view.RowAt(0).Record.CobId == 0x182 && list.Items[511].Text == "time",
                            "Filtered native virtual-item retrieval failed");
                        form.Close();
                    }
                }
                catch (Exception error)
                {
                    failure = error;
                    form.Close();
                }
            };
            form.Shown += (sender, args) =>
            {
                for (int index = 0; index < 20000; index++)
                {
                    pipeline.Enqueue(Row(PacketLogKind.Pdo, (ushort)(0x181 + index % 2), (byte)index).Record);
                }
                pulse.Start();
                timer.Start();
            };
            Application.Run(form);
        }
        if (failure != null)
        {
            throw failure;
        }
        Console.WriteLine("PASS: live filtering/re-filtering with " + pulses + " independent UI timer pulses");
    }
}
