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
    public partial class LogControls : DockContent
    {

        public event EventHandler<EventArgs> ClearLists;

        public LogControls()
        {
            InitializeComponent();

            checkBox_showSDO.Checked = Properties.Settings.Default.showsdo;
            checkBox_showPDO.Checked = Properties.Settings.Default.showpdo;
            checkBox_heartbeats.Checked = Properties.Settings.Default.showHB;
            checkBox_showNMTEC.Checked = Properties.Settings.Default.showNMTEC;
            checkBox_showNMT.Checked = Properties.Settings.Default.showNMT;
            checkBox_showEMCY.Checked = Properties.Settings.Default.showEMCY;

            checkBox_showSDO.CheckedChanged += checkBox_showSDO_CheckedChanged;
            checkBox_showPDO.CheckedChanged += checkBox_showPDO_CheckedChanged;
            checkBox_heartbeats.CheckedChanged += checkBox_heartbeats_CheckedChanged;
            checkBox_showNMTEC.CheckedChanged += checkBox_showNMTEC_CheckedChanged;
            checkBox_showNMT.CheckedChanged += checkBox_showNMT_CheckedChanged;
            checkBox_showEMCY.CheckedChanged += checkBox_showEMCY_CheckedChanged;

            checkbox_autoscroll.Checked = Properties.Settings.Default.autoscroll;
            checkbox_autoscroll.CheckedChanged += checkbox_autoscroll_CheckedChanged;

        }

     
        private void checkBox_heartbeats_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showHB = checkBox_heartbeats.Checked;
            Properties.Settings.Default.Save();

        }

        private void checkBox_showPDO_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showpdo = checkBox_showPDO.Checked;
            Properties.Settings.Default.Save();

        }

        private void checkBox_showSDO_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showsdo = checkBox_showSDO.Checked;
            Properties.Settings.Default.Save();

        }

        private void checkBox_showNMT_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showNMT = checkBox_showNMT.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox_showNMTEC_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showNMTEC = checkBox_showNMTEC.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox_showEMCY_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showEMCY = checkBox_showEMCY.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkbox_autoscroll_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoscroll = checkbox_autoscroll.Checked;
            Properties.Settings.Default.Save();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            ClearLists?.Invoke(this, new EventArgs());
        }
    }
}
