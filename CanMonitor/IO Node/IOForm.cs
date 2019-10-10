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

namespace IO_Node
{

    
    public partial class IOForm : Form
    {

        libCanopenSimple.libCanopenSimple _lco;

        byte out0;
        byte out1;

        byte in0;
        byte in1;

        public IOForm(libCanopenSimple.libCanopenSimple lco)
        {
            _lco = lco;
            InitializeComponent();

        

        }

        private void IOForm_Load(object sender, EventArgs e)
        {

        }

        private void checkBox_out0_CheckedChanged(object sender, EventArgs e)
        {

          
        }

        public void updateIN(byte in0)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                checkBox_out0.Checked = ((in0 & 0x01) == 0x01);
                checkBox_out1.Checked = ((in0 & 0x02) == 0x02);
                checkBox_out2.Checked = ((in0 & 0x04) == 0x04);
                checkBox_out3.Checked = ((in0 & 0x08) == 0x08);
                checkBox_out4.Checked = ((in0 & 0x10) == 0x10);
                checkBox_out5.Checked = ((in0 & 0x20) == 0x20);
                checkBox_out6.Checked = ((in0 & 0x40) == 0x40);
                checkBox_out7.Checked = ((in0 & 0x80) == 0x80);

            }));

        }

        private void checkBox_In0_CheckedChanged(object sender, EventArgs e)
        {

            out0 = 0;
            out0 |= (byte)(checkBox_In0.Checked == true ? 0x01 : 0x00);
            out0 |= (byte)(checkBox_in1.Checked == true ? 0x02 : 0x00);
            out0 |= (byte)(checkBox_in2.Checked == true ? 0x04 : 0x00);
            out0 |= (byte)(checkBox_in3.Checked == true ? 0x08 : 0x00);
            out0 |= (byte)(checkBox_in4.Checked == true ? 0x10 : 0x00);
            out0 |= (byte)(checkBox_in5.Checked == true ? 0x20 : 0x00);
            out0 |= (byte)(checkBox_in6.Checked == true ? 0x40 : 0x00);
            out0 |= (byte)(checkBox_in7.Checked == true ? 0x80 : 0x00);

            out1 = 0;
            out1 |= (byte)(checkBox_in8.Checked == true ? 0x01 : 0x00);
            out1 |= (byte)(checkBox_in9.Checked == true ? 0x02 : 0x00);
            out1 |= (byte)(checkBox_in10.Checked == true ? 0x04 : 0x00);
            out1 |= (byte)(checkBox_in11.Checked == true ? 0x08 : 0x00);
            out1 |= (byte)(checkBox_in12.Checked == true ? 0x10 : 0x00);
            out1 |= (byte)(checkBox_in13.Checked == true ? 0x20 : 0x00);
            out1 |= (byte)(checkBox_in14.Checked == true ? 0x40 : 0x00);
            out1 |= (byte)(checkBox_in15.Checked == true ? 0x80 : 0x00);


            byte[] payload = new byte[2];
            payload[0] = out0;
            payload[1] = out1;
            _lco.writePDO(0x183, payload);


        }
    }
}
