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
using libEDSsharp;

namespace SDOEditorPlugin
{
    public partial class ValueEditor : Form
    {

        public delegate void OnUpdateValue(string value);
        public event OnUpdateValue UpdateValue;

        public string newvalue;
        public ValueEditor(ODentry od,string currentval)
        {
            DialogResult = DialogResult.Cancel;

            InitializeComponent();

            label_default.Text = od.defaultvalue;
            
            label_index.Text = string.Format("0x{0:x4}", od.Index);
            label_sub.Text = string.Format("0x{0:x2}", od.Subindex);
            label_name.Text = od.parameter_name;
            textBox_desc.Text = od.Description;
            textBox_current.Text = currentval;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            newvalue = textBox_current.Text;
            try
            {
                UpdateValue?.Invoke(textBox_current.Text);
            }
            catch (Exception ex)
            {

            }

            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            newvalue = textBox_current.Text;

            try
            {
                UpdateValue?.Invoke(textBox_current.Text);
            }
            catch(Exception ex)
            {

            }
        }

        private void buttondown_Click(object sender, EventArgs e)
        {

            int val = Convert.ToUInt16(textBox_current.Text);
            val -= 10;

            textBox_current.Text = val.ToString();

            UpdateValue?.Invoke(textBox_current.Text);
        }
    }
}
