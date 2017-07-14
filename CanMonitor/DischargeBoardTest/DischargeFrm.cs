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

namespace DischargeBoardTest
{
    public partial class DischargeFrm : Form
    {
        libCanopenSimple.libCanopenSimple lco;

        public DischargeFrm(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button_setduration_Click(object sender, EventArgs e)
        {

            byte node = (byte)numericUpDown_node.Value;
            UInt32[] duration = new UInt32[4];
            duration[0] = (UInt32)numericUpDown_duration1.Value;
            duration[1] = (UInt32)numericUpDown_duration2.Value;
            duration[2] = (UInt32)numericUpDown_duration3.Value;
            duration[3] = (UInt32)numericUpDown_duration4.Value;

            lco.SDOwrite(node, 0x2150, 1, duration[0],null);
            lco.SDOwrite(node, 0x2150, 2, duration[1], null);
            lco.SDOwrite(node, 0x2150, 3, duration[2], null);
            lco.SDOwrite(node, 0x2150, 4, duration[3], null);

        }

        private void button_setmode_Click(object sender, EventArgs e)
        {
            byte node = (byte)numericUpDown_node.Value;
            byte mode = (byte)comboBox_mode.SelectedIndex;

            lco.SDOwrite(node, 0x2151, 1, mode, null);
        }

        private void button_unlock_Click(object sender, EventArgs e)
        {
            byte node = (byte)numericUpDown_node.Value;
            UInt64 unlock = 0x554E4C4F434B0000L;
            lco.SDOwrite(node, 0x2153, 0, unlock, null);
        }

        private void button_fire_Click(object sender, EventArgs e)
        {
            byte node = (byte)numericUpDown_node.Value;
            UInt32 fire = 0x46495200;
            lco.SDOwrite(node, 0x2154, 0, fire, null);
        }
    }
}
