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


namespace NMTPlugin
{
    public partial class NMTFrm : Form
    {
        libCanopenSimple.libCanopenSimple lco;

        public NMTFrm(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button_startbus_Click(object sender, EventArgs e)
        {
            lco.NMT_start((byte)numericUpDown1.Value);
        }

        private void button_stopbus_Click(object sender, EventArgs e)
        {
            lco.NMT_stop((byte)numericUpDown1.Value);
        }

        private void button_preop_Click(object sender, EventArgs e)
        {
            lco.NMT_preop((byte)numericUpDown1.Value);
        }

        private void button_resetcomms_Click(object sender, EventArgs e)
        {
            lco.NMT_ResetComms((byte)numericUpDown1.Value);
        }

        private void button_resetnodes_Click(object sender, EventArgs e)
        {
            lco.NMT_ResetNode((byte)numericUpDown1.Value);
        }
    }
}
