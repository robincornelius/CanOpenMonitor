using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        
        public delegate void OnSaveValue(string file, ODentry od);
        public event OnSaveValue SaveValue;


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

            if(od.datatype == DataType.DOMAIN)
            {
                button_downloadfile.Enabled = true;
                button_uploadfile.Enabled = true;
                button1.Enabled = false;
                button2.Enabled = false;
                textBox_current.Enabled = false;

            }
            else
            {
                button_downloadfile.Enabled = false;
                button_uploadfile.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
                textBox_current.Enabled = true;
            }


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

        private void button_uploadfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            
            if(ofd.ShowDialog()== DialogResult.OK)
            {

                byte[] b = File.ReadAllBytes(ofd.FileName);

                string str = System.Text.Encoding.ASCII.GetString(b);
                UpdateValue?.Invoke(str);

            }    

        }

        private void button_downloadfile_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveValue?.Invoke(sfd.FileName, tod);
            }

        }
    }
}
