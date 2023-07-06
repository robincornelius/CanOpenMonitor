using libCanopenSimple;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static CanMonitor.CanLogForm;

namespace CanMonitor
{
    public partial class NMTDocument : DockContent, ICanDocument
    {
        public struct SNMTState
        {
            public byte state;
            public DateTime lastupdate;
            public bool dirty;
            public ListViewItem LVI;
            public bool isnew;
            public string statemsg;
        }

        Dictionary<UInt16, SNMTState> NMTstate = new Dictionary<ushort, SNMTState>();
        List<SNMTState> dirtyNMTstates = new List<SNMTState>();


        public NMTDocument()
        {
            InitializeComponent();
            listView_nmt.DoubleBuffering(true);

            Program.lco.nmtecevent += Lco_nmtecevent;

        }

        public void clearlist()
        {
            lock (dirtyNMTstates)
            {
                NMTstate.Clear();
                dirtyNMTstates.Clear();
                listView_nmt.Items.Clear();
            }

        }

        private void Lco_nmtecevent(libCanopenSimple.canpacket payload, DateTime dt)
        {

            string msg = "";

            switch (payload.data[0])
            {
                case 0x01:
                    msg = "Enter operational";
                    break;
                case 0x02:
                    msg = "Enter stop";
                    break;
                case 0x80:
                    msg = "Enter pre-operational";
                    break;
                case 0x81:
                    msg = "Reset node";
                    break;
                case 0x82:
                    msg = "Reset communications";
                    break;

            }

            lock (NMTstate)
            {
                byte node = (byte)(payload.cob & 0x0FF);

                if (NMTstate.ContainsKey(node))
                {
                    SNMTState s = NMTstate[node];
                    s.lastupdate = dt;
                    s.dirty = true;
                    s.state = payload.data[0];
                    s.isnew = false;
                    s.statemsg = msg;
                    NMTstate[node] = s;
                    dirtyNMTstates.Add(NMTstate[node]);
                }
                else
                {
                    SNMTState s = new SNMTState();
                    s.lastupdate = dt;
                    s.dirty = true;
                    s.state = payload.data[0];
                    s.statemsg = msg;
                    string[] ss = new string[3];
                    ss[0] = DateTime.Now.ToString();
                    ss[1] = string.Format("0x{0:x2} ({1})", node, node);
                    ss[2] = msg;

                    ListViewItem newitem = new ListViewItem(ss);
                    s.LVI = newitem;
                    s.isnew = true;

                    NMTstate.Add(node, s);
                    dirtyNMTstates.Add(NMTstate[node]);
                }

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (NMTstate)
            {
                if (dirtyNMTstates.Count > 0)
                {
                    listView_nmt.BeginUpdate();

                    foreach (SNMTState state in dirtyNMTstates)
                    {
                        if (state.isnew)
                        {
                            listView_nmt.Items.Add(state.LVI);
                        }
                        else
                        {
                            state.LVI.SubItems[0].Text = state.lastupdate.ToString();
                            state.LVI.SubItems[2].Text = state.statemsg;
                        }

                    }

                    dirtyNMTstates.Clear();

                    listView_nmt.EndUpdate();
                }
            }
        }
    }
}
