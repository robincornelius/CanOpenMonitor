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

namespace CanMonitor
{
    public partial class Prefs : Form
    {

        private string appdatafolder;
        private string assemblyfolder;
        public Prefs(string _appdatafolder, string _assemblyfolder)
        {
            InitializeComponent();

            this.appdatafolder = _appdatafolder;
            this.assemblyfolder = _assemblyfolder;


            checkBox_autostart.Checked = Properties.Settings.Default.autoconnect;

            checkBox_limitlines.Checked = Properties.Settings.Default.limitlines;
            textBox_linelimit.Text = Properties.Settings.Default.linelimit.ToString();

            textBox_filelogfolder.Text = Properties.Settings.Default.FileLogFolder;
            checkBox_filelog.Checked = Properties.Settings.Default.AutoFileLog;

            loadplugins();
        }

        string[] autoloadfixed = new string[0];
        string[] autoload = new string[0];
        public void loadplugins()
        {

            listView_plugins.Items.Clear();

            string autoloadPath = Path.Combine(appdatafolder, "autoload.txt");
            string autoloadFixedPath = Path.Combine(assemblyfolder, "autoload.txt");

            if (File.Exists(autoloadPath))
            {
                autoload = System.IO.File.ReadAllLines(autoloadPath);
            }

            if (File.Exists(autoloadFixedPath))
            {
                autoloadfixed = System.IO.File.ReadAllLines(autoloadFixedPath);
            }

            adddrivers(Directory.GetFiles(Path.Combine(assemblyfolder, "plugins"), "*.dll"));
            adddrivers(Directory.GetFiles(Path.Combine(assemblyfolder, "privateplugins"), "*.dll"));

        }

        private void adddrivers(string[] founddrivers)
        {
            foreach (string driver in founddrivers)
            {

                ListViewItem i = new ListViewItem();

                string drivername = Path.GetFileName(driver);

                if (drivername.ToLower() == "pdointerface.dll")
                    continue;

                if (autoloadfixed.Contains(driver))
                    continue;

                i.SubItems.Add(new ListViewItem.ListViewSubItem(i, drivername));

                if (autoload.Contains(drivername))
                {
                    i.Checked = true;
                }


                listView_plugins.Items.Add(i);

            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button_saveplugins_Click(object sender, EventArgs e)
        {

            List<string> active = new List<string>();

            foreach (ListViewItem i in listView_plugins.Items)
            {
                if (i.Checked == true)
                {
                    active.Add(i.SubItems[1].Text);
                }

            }

            string autoloadPath = Path.Combine(appdatafolder, "autoload.txt");

            System.IO.File.WriteAllLines(autoloadPath, active);

            Properties.Settings.Default.autoconnect = checkBox_autostart.Checked;

            Properties.Settings.Default.limitlines = checkBox_limitlines.Checked;


            int result;

            if (int.TryParse(textBox_linelimit.Text, out result))
            {
                Properties.Settings.Default.linelimit = result;
            }

            Properties.Settings.Default.FileLogFolder = textBox_filelogfolder.Text;
            Properties.Settings.Default.AutoFileLog = checkBox_filelog.Checked;


            Properties.Settings.Default.Save();
            this.Close();

        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
