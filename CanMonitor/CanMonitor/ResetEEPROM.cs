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
    public partial class ResetEEPROM : Form
    {
        private libCanopenSimple.libCanopenSimple lco;

        public ResetEEPROM(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte node = (byte) numericUpDown_node.Value;

            if (MessageBox.Show("Reset eeprom?", string.Format("Really reset eeprom on node {0}", node), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                lco.SDOwrite(node, 0x1011, 0x01, 0x64616f6c, null);
                lco.SDOwrite(node, 0x1011, 0x7f, 0x64616f6c, null);
                lco.SDOwrite(node, 0x1010, 0x01, 0x65766173, null);

            }
        }

        private void button_savetoeeprom_Click(object sender, EventArgs e)
        {
            byte node = (byte)numericUpDown_node.Value;

            if (MessageBox.Show("save to eeprom?", string.Format("Really save to eeprom on node {0}", node), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                lco.SDOwrite(node, 0x1010, 0x01, 0x65766173, null);
            }
        }
    }
}
