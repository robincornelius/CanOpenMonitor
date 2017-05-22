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
        public string newvalue;
        public ValueEditor(ODentry od,string currentval)
        {
            DialogResult = DialogResult.Cancel;

            InitializeComponent();

            label_default.Text = od.defaultvalue;
            label_index.Text = string.Format("0x{0:x4}", od.index);
            label_sub.Text = string.Format("0x{0:x2}", od.subindex);
            label_name.Text = od.parameter_name;
            textBox_desc.Text = od.Description;
            textBox_current.Text = currentval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newvalue = textBox_current.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
