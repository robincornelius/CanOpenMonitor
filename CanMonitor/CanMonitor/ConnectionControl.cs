using libCanopenSimple;
using N_SettingsMgr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CanMonitor
{
    public partial class ConnectionControl : DockContent
    {
        public ConnectionControl()
        {
            InitializeComponent();

            Program.driverloader.portchangedevent += Driverloader_portchangedevent;
            Program.driverloader.finddrivers();
            Program.driverloader.enumerateports();
            Program.lco.connectionevent += Lco_connectionevent;

            Properties.Settings.Default.Reload();

            //select last used port
            foreach (driverport dp in comboBox_port.Items)
            {
                if (dp.port == Properties.Settings.Default.lastport)
                {
                    comboBox_port.SelectedItem = dp;
                    break;
                }
            }

            comboBox_rate.SelectedIndex = SettingsMgr.settings.options.selectedrate;
            comboBox_port.SelectedItem = SettingsMgr.settings.options.selectedport;
        }

        private void button_open_Click(object sender, EventArgs e)
        {
            try
            {
                if (button_open.Text == "Close")
                {

                    //fixme streamwriter
                    //if (sw != null)
                    //    sw.Close();

                    Program.driverloader.Close();
                    return;
                }

                if (comboBox_port.SelectedItem == null)
                {
                    comboBox_port.Text = "";
                    return;
                }

                driverport dp = (driverport)comboBox_port.SelectedItem;

                //textBox_info.AppendText(String.Format("Trying to open port {0} using driver {1} \r\n", dp.port, dp.driver));

                int rate = comboBox_rate.SelectedIndex;

                Program.driverloader.Open(dp, (BUSSPEED)rate);

                if (Program.lco.isopen())
                {
                    Properties.Settings.Default.lastport = dp.port;
                    Properties.Settings.Default.lastdriver = dp.driver;
                    Properties.Settings.Default.lastrate = comboBox_rate.Text;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //textBox_info.AppendText("ERROR opening port " + ex.ToString() + "\r\n");
                //MessageBox.Show("Setup error " + ex.ToString());

            }

        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            //enumerateports();
            Program.driverloader.enumerateports();
        }

        private void comboBox_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsMgr.settings.options.selectedport = comboBox_port.SelectedItem.ToString();

        }

        private void comboBox_rate_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsMgr.settings.options.selectedrate = comboBox_rate.SelectedIndex;
        }

        private void Driverloader_portchangedevent(object sender, EventArgs e)
        {
            if (this.IsHandleCreated == false)
                return;

            this.Invoke(new MethodInvoker(delegate ()
            {
                comboBox_port.Text = "";
                comboBox_port.Items.Clear();

                foreach (driverport dp in Program.driverloader._driverport)
                {
                    comboBox_port.Items.Add(dp);
                }

            }));

        }

        private void Lco_connectionevent(object sender, EventArgs e)
        {
            if (Program.lco.isopen() == true)
            {
                button_open.BackColor = Color.Red;
                button_open.Text = "Close";
            }
            else
            {
                button_open.BackColor = Color.Green;
                button_open.Text = "Open";
            }


        }


    }
}
