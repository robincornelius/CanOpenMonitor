/*
    This file is part of CanOpenMonitor.

    CanOpenMonitor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CanOpenMonitor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CanOpenMonitor.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

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

namespace eeprom_plugin
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
