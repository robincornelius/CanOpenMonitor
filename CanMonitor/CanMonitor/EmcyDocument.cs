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
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CanMonitor
{
    public partial class EmcyDocument : DockContent, ICanDocument
    {
        List<ListViewItem> EMClistitems = new List<ListViewItem>();

        public EmcyDocument()
        {
            InitializeComponent();
            listView_emcy.DoubleBuffering(true);
            Program.lco.emcyevent += log_EMCY;

        }

        public void clearlist()
        {
            lock (EMClistitems)
            {
                EMClistitems.Clear();
                listView_emcy.Items.Clear();
            }

        }

        private void log_EMCY(canpacket payload, DateTime dt)
        {

            string[] items = new string[6];
            string[] items2 = new string[5];

            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
            items[1] = "EMCY";
            items[2] = string.Format("{0:x3}", payload.cob);
            items[3] = string.Format("{0:x3}", payload.cob - 0x080);
            items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);
            //items[4] = "EMCY";

            items2[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
            items2[1] = items[2];
            items2[2] = items[3];

            UInt16 code = (UInt16)(payload.data[0] + (payload.data[1] << 8));
            byte bits = (byte)(payload.data[3]);
            UInt32 info = (UInt32)(payload.data[4] + (payload.data[5] << 8) + (payload.data[6] << 16) + (payload.data[7] << 24));

            if (ErrorCodes.errcode.ContainsKey(code))
            {

                string bitinfo;

                if (ErrorCodes.errbit.ContainsKey(bits))
                {
                    bitinfo = ErrorCodes.errbit[bits];
                }
                else
                {
                    bitinfo = string.Format("bits 0x{0:x2}", bits);
                }

                items[5] = string.Format("Error: {0} - {1} info 0x{2:x8}", ErrorCodes.errcode[code], bitinfo, info);
            }
            else
            {
                items[5] = string.Format("Error code 0x{0:x4} bits 0x{1:x2} info 0x{2:x8}", code, bits, info);
            }

            items2[3] = items[5];

            ListViewItem i = new ListViewItem(items);
            ListViewItem i2 = new ListViewItem(items2);

            i.ForeColor = Color.White;
            i2.ForeColor = Color.White;

            if (code == 0)
            {
                i.BackColor = Color.Green;
                i2.BackColor = Color.Green;

            }
            else
            {
                i.BackColor = Color.Red;
                i2.BackColor = Color.Red;

            }

            lock (EMClistitems)
                EMClistitems.Add(i2);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (EMClistitems.Count > 0)
            {
                lock (EMClistitems)
                {
                    listView_emcy.BeginUpdate();
                    listView_emcy.Items.AddRange(EMClistitems.ToArray());
                    EMClistitems.Clear();

                    if (Properties.Settings.Default.limitlines)
                    {
                        while (listView_emcy.Items.Count > Properties.Settings.Default.linelimit)
                            listView_emcy.Items.RemoveAt(0);

                    }

                    listView_emcy.EndUpdate();
                }
            }
        }
    }
}
