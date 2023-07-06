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
    public partial class MainDockForm : Form
    {
        public MainDockForm()
        {

            InitializeComponent();
            //  dockPanel1.Theme = new WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase();
            var theme = new VS2015DarkTheme();
            dockPanel1.Theme = theme;

            CanLogForm clf = new CanLogForm();
            clf.dockpanel = dockPanel1;
            clf.Show(dockPanel1, DockState.Document);

        }
    }
}
