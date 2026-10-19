using System;
using System.Collections.Generic;

namespace CanMonitor.PacketDisplay
{
    // Owned by the UI thread. Eviction is O(1), unlike removing the first
    // ListViewItem repeatedly. With no line limit the ring grows without a cap.
    internal sealed class PacketHistory
    {
        private PacketLogRow[] rows = new PacketLogRow[256];
        private int start;
        internal int Count { get; private set; }

        internal PacketLogRow this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return rows[(start + index) % rows.Length];
            }
        }

        internal void Add(PacketLogRow row, int? limit)
        {
            if (limit.HasValue)
            {
                TrimTo(Math.Max(0, limit.Value - 1));
                if (limit.Value <= 0)
                {
                    return;
                }
            }

            if (Count == rows.Length)
            {
                PacketLogRow[] larger = new PacketLogRow[checked(rows.Length * 2)];
                CopyTo(larger);
                rows = larger;
                start = 0;
            }

            rows[(start + Count) % rows.Length] = row;
            Count++;
        }

        internal void TrimTo(int limit)
        {
            while (Count > Math.Max(0, limit))
            {
                rows[start] = null;
                start = (start + 1) % rows.Length;
                Count--;
            }
        }

        internal int RemoveExportedPrefix(ISet<PacketLogRow> exported, int maximum = int.MaxValue)
        {
            int removed = 0;
            while (Count > 0 && removed < maximum && exported.Contains(this[0]))
            {
                rows[start] = null;
                start = (start + 1) % rows.Length;
                Count--;
                removed++;
            }
            return removed;
        }

        internal void Clear()
        {
            rows = new PacketLogRow[256];
            Count = 0;
            start = 0;
        }

        internal PacketLogRow[] Snapshot()
        {
            var result = new PacketLogRow[Count];
            CopyTo(result);
            return result;
        }

        private void CopyTo(PacketLogRow[] destination)
        {
            int first = Math.Min(Count, rows.Length - start);
            Array.Copy(rows, start, destination, 0, first);
            Array.Copy(rows, 0, destination, first, Count - first);
        }
    }

    // A state table needs the latest value for each node, not an unbounded list
    // of every heartbeat. Swapping under the lock keeps all control work outside.
    internal sealed class LatestStateBuffer<TKey, TValue>
    {
        private readonly object sync = new object();
        private Dictionary<TKey, TValue> pending = new Dictionary<TKey, TValue>();

        internal void Put(TKey key, TValue value)
        {
            lock (sync)
            {
                pending[key] = value;
            }
        }

        internal Dictionary<TKey, TValue> Take()
        {
            lock (sync)
            {
                var result = pending;
                pending = new Dictionary<TKey, TValue>();
                return result;
            }
        }

        internal void Clear()
        {
            lock (sync)
            {
                pending = new Dictionary<TKey, TValue>();
            }
        }
    }
}
