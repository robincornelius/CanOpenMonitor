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

        ODentry tod;

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

            tod = od;

            updatestring();

        }

        private void updatestring()
        {
            if (tod == null)
                return;

            if(tod.datatype == DataType.VISIBLE_STRING)
            {
                label_length.Text = String.Format("{0}/{1}", textBox_current.Text.Length,tod.defaultvalue.Length);
            }
            else
            {
                label_length.Text = "";
            }
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

        private void textBox_current_TextChanged(object sender, EventArgs e)
        {
            updatestring();
                
        }
    }
}
