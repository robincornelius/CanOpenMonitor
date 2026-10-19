using System;
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
            if (args.ItemIndex < 0 || args.ItemIndex >= History.Count)
            {
                args.Item = new ListViewItem("");
                return;
            }

            PacketLogRow row = History[args.ItemIndex];
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
            int previousCount = History.Count;
            int previousTop = list.IsHandleCreated && previousCount > 0 && list.TopItem != null ? list.TopItem.Index : 0;
            var budget = Stopwatch.StartNew();
            PacketLogRow row;
            int added = 0;
            while (added < maxRows && (added == 0 || budget.ElapsedMilliseconds < milliseconds)
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
            if (added != 0 || History.Count != previousCount)
            {
                int evicted = previousCount + added - History.Count;
                list.VirtualListSize = History.Count;
                list.Invalidate();
                if (History.Count > 0 && list.IsHandleCreated)
                {
                    if (autoScroll)
                    {
                        list.EnsureVisible(History.Count - 1);
                    }
                    else if (evicted > 0)
                    {
                        list.TopItem = list.Items[Math.Max(0, Math.Min(History.Count - 1, previousTop - evicted))];
                    }
                }
            }
            return added;
        }

        internal void Clear()
        {
            Pipeline.Clear();
            History.Clear();
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
