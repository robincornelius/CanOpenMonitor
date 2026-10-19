using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace CanMonitor.PacketDisplay
{
    internal sealed class VirtualPacketLog : IDisposable
    {
        private readonly ListView list;
        private readonly int[] columns;
        internal readonly PacketDisplayPipeline Pipeline;
        internal readonly PacketHistory History = new PacketHistory();
        internal int LastDrainCount { get; private set; }
        private List<long> visible = new List<long>();
        private int visibleStart;
        private long scanned;
        private Func<PacketLogRow, bool> predicate;

        internal bool IsRebuilding
        {
            get
            {
                return predicate != null && scanned < History.EndSequence;
            }
        }

        internal int VisibleCount
        {
            get
            {
                return predicate == null ? History.Count : visible.Count - visibleStart;
            }
        }

        internal void SetFilter(Func<PacketLogRow, bool> match)
        {
            predicate = match;
            visible = new List<long>();
            visibleStart = 0;
            scanned = History.FirstSequence;
            list.VirtualListSize = match == null ? History.Count : 0;
            list.Invalidate();
        }

        internal PacketLogRow RowAt(int index)
        {
            long offset = predicate == null ? index : visible[visibleStart + index] - History.FirstSequence;
            return offset < 0 || offset >= History.Count ? null : History[(int)offset];
        }

        internal PacketLogRow[] VisibleSnapshot()
        {
            var rows = new PacketLogRow[VisibleCount];
            for (int index = 0; index < rows.Length; index++)
            {
                rows[index] = RowAt(index);
            }
            return rows;
        }

        internal VirtualPacketLog(ListView list, PacketDisplayPipeline pipeline, int[] columns = null)
        {
            this.list = list;
            this.columns = columns;
            Pipeline = pipeline;
            list.RetrieveVirtualItem += Retrieve;
            list.VirtualMode = true;
        }

        private void Retrieve(object sender, RetrieveVirtualItemEventArgs args)
        {
            if (args.ItemIndex < 0 || args.ItemIndex >= VisibleCount)
            {
                args.Item = new ListViewItem("");
                return;
            }

            PacketLogRow row = RowAt(args.ItemIndex);
            if (row == null)
            {
                args.Item = new ListViewItem("");
                return;
            }
            string[] cells = row.Cells;
            if (columns != null)
            {
                cells = new string[columns.Length];
                for (int index = 0; index < columns.Length; index++)
                {
                    cells[index] = row.Cells[columns[index]];
                }
            }
            args.Item = new ListViewItem(cells) { ForeColor = row.Foreground, BackColor = row.Background };
        }

        internal int Drain(int maxRows, int milliseconds, int? retainedLimit, bool autoScroll)
        {
            int previousCount = list.VirtualListSize;
            int previousTop = list.IsHandleCreated && previousCount > 0 && list.TopItem != null ? list.TopItem.Index : 0;
            var budget = Stopwatch.StartNew();
            PacketLogRow row;
            int added = 0;
            int inputBudget = predicate == null ? milliseconds : Math.Max(1, milliseconds / 2);
            while (added < maxRows && (added == 0 || budget.ElapsedMilliseconds < inputBudget)
                && Pipeline.TryTake(out row))
            {
                History.Add(row, retainedLimit);
                added++;
            }

            if (retainedLimit.HasValue)
            {
                History.TrimTo(retainedLimit.Value);
            }
            LastDrainCount = added;
            int evictedVisible = 0;
            if (predicate != null)
            {
                while (visibleStart < visible.Count && visible[visibleStart] < History.FirstSequence)
                {
                    visibleStart++;
                    evictedVisible++;
                }
                // Compact occasionally, rather than shifting every time the
                // retained-history ring evicts its oldest row.
                if (visibleStart > 4096 && visibleStart > visible.Count / 2)
                {
                    visible.RemoveRange(0, visibleStart);
                    visibleStart = 0;
                }
                scanned = Math.Max(scanned, History.FirstSequence);
                int examined = 0;
                while (scanned < History.EndSequence && examined < (long)maxRows * 4
                    && (examined == 0 || budget.ElapsedMilliseconds < milliseconds))
                {
                    if (predicate(History[(int)(scanned - History.FirstSequence)]))
                    {
                        visible.Add(scanned);
                    }
                    scanned++;
                    examined++;
                }
            }
            int count = VisibleCount;
            if (added != 0 || count != previousCount || evictedVisible != 0)
            {
                int evicted = predicate == null ? previousCount + added - count : evictedVisible;
                if (list.VirtualListSize != count)
                {
                    list.VirtualListSize = count;
                }
                list.Invalidate();
                if (count > 0 && list.IsHandleCreated)
                {
                    if (autoScroll)
                    {
                        list.EnsureVisible(count - 1);
                    }
                    else if (evicted > 0)
                    {
                        list.TopItem = list.Items[Math.Max(0, Math.Min(count - 1, previousTop - evicted))];
                    }
                }
            }
            return added;
        }

        internal void Clear()
        {
            Pipeline.Clear();
            History.Clear();
            visible = new List<long>();
            visibleStart = 0;
            scanned = 0;
            list.VirtualListSize = 0;
            list.Invalidate();
        }

        public void Dispose()
        {
            list.RetrieveVirtualItem -= Retrieve;
            Pipeline.Dispose();
        }
    }
}
