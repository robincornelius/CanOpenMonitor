using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libCanopenSimple;


namespace CanMonitor
{
    public partial class ErrorInject : Form
    {
        libCanopen lco;

        public ErrorInject(libCanopen lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button_inject_Click(object sender, EventArgs e)
        {

            byte node = (byte)numericUpDown_node.Value;
            byte bit = (byte)numericUpDown_errorbity.Value;
            UInt32 info = (UInt32)numericUpDown_info.Value;
            UInt16 code = (UInt16)numericUpDown_code.Value;

            canpacket p = new canpacket();
            p.cob=(byte) (0x80+node);
            p.len=8;
            p.data = new byte[8];
            p.data[0] = (byte)(code & 0x00FF);
            p.data[1] = (byte)((code>>8) & 0x00FF);
            p.data[2] = 0;
            p.data[3] = bit;
            p.data[4] = (byte)info;
            p.data[5] = (byte)(info>>8);
            p.data[6] = (byte)(info >> 16);
            p.data[7] = (byte)(info >> 24);



            lco.SendPacket(p);

        }
    }
}
