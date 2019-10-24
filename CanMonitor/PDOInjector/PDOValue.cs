using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDOInjector
{
    public partial class PDOValue : UserControl
    {
        public UInt16 cob;
        public byte len;
        public byte[] data;

        public delegate void SendPDOEvent(object sender, EventArgs e);
        public event SendPDOEvent sendpdo;

        public PDOValue()
        {
            InitializeComponent();

           
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            len = (byte)numericUpDown_bytes.Value;
            data = new byte[len];
            cob = (UInt16) numericUpDown_cob.Value;

            if(len > 0)
                data[0] = (byte)numericUpDown_byte1.Value;

            if (len > 1)
                data[1] = (byte)numericUpDown_byte2.Value;

            if (len > 2)
                data[2] = (byte)numericUpDown_byte3.Value;

            if (len > 3)
                data[3] = (byte)numericUpDown_byte4.Value;

            if (len > 4)
                data[4] = (byte)numericUpDown_byte5.Value;

            if (len > 5)
                data[5] = (byte)numericUpDown_byte6.Value;

            if (len > 6)
                data[6] = (byte)numericUpDown_byte7.Value;

            if (len > 7)
                data[7] = (byte)numericUpDown_byte8.Value;

            if(sendpdo != null)
            {
                sendpdo(this, new EventArgs());
            }

        }

        private void numericUpDown_bytes_ValueChanged(object sender, EventArgs e)
        {
            len = (byte)numericUpDown_bytes.Value;

            numericUpDown_byte1.Enabled = len > 0;
            numericUpDown_byte2.Enabled = len > 1;
            numericUpDown_byte3.Enabled = len > 2;
            numericUpDown_byte4.Enabled = len > 3;
            numericUpDown_byte5.Enabled = len > 4;
            numericUpDown_byte6.Enabled = len > 5;
            numericUpDown_byte7.Enabled = len > 6;
            numericUpDown_byte8.Enabled = len > 7;


        }
    }
}
