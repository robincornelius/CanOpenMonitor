using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CanMonitor
{
    public partial class Prefs : Form
    {

        private string appdatafolder;
        private string assemblyfolder;

        // The path to the key where Windows looks for startup applications
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        static string MyApp = "CanMonitor";


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


            if (rkApp.GetValue(MyApp) == null)
            {
                // The value doesn't exist, the application is not set to run at startup
                checkBox_startwithwindows.Checked = false;
            }
            else
            {
                // The value exists, the application is set to run at startup
                checkBox_startwithwindows.Checked = true;
            }

            textBox_lastpport.Text = Properties.Settings.Default.lastport;
            textBox_rate.Text = Properties.Settings.Default.lastrate;


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

                if (drivername.ToLower() == "guicomponents.dll")
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

            if (checkBox_startwithwindows.Checked)
            {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue(MyApp, Application.ExecutablePath);
            }
            else
            {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue(MyApp, false);
            }


            this.Close();

        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox_filelogfolder_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox_filelog_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
