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
    public partial class ChgrFrm : Form
    {
        libCanopen lco;

        public ChgrFrm(libCanopen lco)
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

        private void button_on_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[2];
            data[0] = 0x04; //on bit 2 off bit 1
            data[1] = 0x00;

            lco.writePDO(0x182, data);


            System.Threading.Thread.Sleep(500);

            data[0] = 0x00; //on bit 2 off bit 1
            data[1] = 0x00;

            lco.writePDO(0x182, data);


        }

        private void button_off_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[3];
            data[0] = 0x02;
            data[1] = 0x00;

            lco.writePDO(0x182, data);

            System.Threading.Thread.Sleep(500);
            data[0] = 0x00; //on bit 2 off bit 1
            data[1] = 0x00;

            lco.writePDO(0x182, data);

        }
    }
}
