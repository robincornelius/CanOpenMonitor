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

namespace ChargerTestPlugin
{
    public partial class ChgrFrm : Form
    {
        libCanopenSimple.libCanopenSimple lco;

        public ChgrFrm(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button_charge_Click(object sender, EventArgs e)
        {

            UInt16 vol = (UInt16)numericUpDown1.Value;
          
            byte[] data = new byte[3];
            data[0] = (byte)vol;
            data[1] = (byte)(vol >> 8);
            data[2] = 0xFF;

            lco.writePDO(0x214, data);

        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[3];
            data[0] = 0x00;
            data[1] = 0x00;
            data[2] = 0x00;

            lco.writePDO(0x214, data);
        }


    }
}
