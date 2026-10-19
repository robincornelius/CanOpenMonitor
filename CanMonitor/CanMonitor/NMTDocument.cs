using CanMonitor.PacketDisplay;
using libCanopenSimple;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CanMonitor
{
    public partial class NMTDocument : DockContent, ICanDocument
    {
        private readonly libCanopenSimple.libCanopenSimple source;
        private struct NodeUpdate
        {
            internal DateTime Timestamp;
            internal byte State;
        }

        private readonly LatestStateBuffer<ushort, NodeUpdate> pending = new LatestStateBuffer<ushort, NodeUpdate>();
        private readonly Dictionary<ushort, ListViewItem> rows = new Dictionary<ushort, ListViewItem>();

        public NMTDocument() : this(Program.lco)
        {
        }

        internal NMTDocument(libCanopenSimple.libCanopenSimple source)
        {
            this.source = source;
            InitializeComponent();
            listView_nmt.DoubleBuffering(true);
            source.nmtecevent += Lco_nmtecevent;
            timer1.Interval = 100;
            timer1.Start();
            Disposed += Detach;
        }

        public void clearlist()
        {
            pending.Clear();
            rows.Clear();
            listView_nmt.Items.Clear();
        }

        private void Lco_nmtecevent(canpacket packet, DateTime timestamp)
        {
            if (packet.data == null || packet.data.Length == 0)
            {
                return;
            }
            ushort node = (ushort)(packet.cob & 0x7F);
            pending.Put(node, new NodeUpdate { Timestamp = timestamp, State = packet.data[0] });
        }

        private void timer1_Tick(object sender, EventArgs args)
        {
            Dictionary<ushort, NodeUpdate> updates = pending.Take();
            if (updates.Count == 0)
            {
                return;
            }
            listView_nmt.BeginUpdate();
            try
            {
                foreach (var pair in updates)
                {
                    ListViewItem row;
                    string state;
                    switch (pair.Value.State)
                    {
                        case 0:
                            state = "BOOT";
                            break;
                        case 4:
                            state = "STOPPED";
                            break;
                        case 5:
                            state = "OPERATIONAL";
                            break;
                        case 0x7F:
                            state = "PRE-OPERATIONAL";
                            break;
                        default:
                            state = "State 0x" + pair.Value.State.ToString("x2");
                            break;
                    }
                    if (!rows.TryGetValue(pair.Key, out row))
                    {
                        row = new ListViewItem(new[] { "", string.Format("0x{0:x2} ({0})", pair.Key), "" });
                        rows.Add(pair.Key, row);
                        listView_nmt.Items.Add(row);
                    }
                    row.SubItems[0].Text = pair.Value.Timestamp.ToString();
                    row.SubItems[2].Text = state;
                }
            }
            finally
            {
                listView_nmt.EndUpdate();
            }
        }

        private void Detach(object sender, EventArgs args)
        {
            source.nmtecevent -= Lco_nmtecevent;
            timer1.Stop();
            pending.Clear();
        }
    }
}
