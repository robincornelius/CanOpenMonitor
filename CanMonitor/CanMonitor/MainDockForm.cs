using N_SettingsMgr;
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
using WeifenLuo.WinFormsUI.Docking;

namespace CanMonitor
{
    public partial class MainDockForm : Form
    {
        private List<string> _mru = new List<string>();
        string gitVersion;

        ConnectionControl connectioncontrol = null;
        LogControls logcontrols = null;

        InfoLogDocument infolog = null;
        NMTDocument nmtdocument = null;
        EmcyDocument emcydocument = null;
        CanLogForm clf = null;


        public MainDockForm()
        {
            InitializeComponent();

            this.FormClosing += MainDockForm_FormClosing;
            this.Load += MainDockForm_Load;

            Program.pluginManager.menuStrip1 = this.menuStrip1;
 
            //  dockPanel1.Theme = new WeifenLuo.WinFormsUI.ThemeVS2015.VS2015ThemeBase();
            var theme = new VS2015DarkTheme();
            dockPanel1.Theme = theme;

            Program.pluginManager.SetDockPanel(dockPanel1);

            clf = new CanLogForm();
            clf.dockpanel = dockPanel1;
            clf.Show(dockPanel1, DockState.Document);

            connectioncontrol = new ConnectionControl();
            //connectioncontrol.Show(dockPanel1, DockState.DockTop);
            addtotopcontrols(connectioncontrol);

            logcontrols = new LogControls();
            logcontrols.ClearLists += Logcontrols_ClearLists;
            addtotopcontrols(logcontrols);
            //logcontrols.Show(dockPanel1, DockState.DockTop);

            infolog = new InfoLogDocument();
            nmtdocument = new NMTDocument();
            emcydocument = new EmcyDocument();
            
            nmtdocument.Show(dockPanel1, DockState.Document);
            emcydocument.Show(dockPanel1, DockState.Document);
            infolog.Show(dockPanel1, DockState.Document);

            connectioncontrol.Activate();
            clf.Activate();

            Program.pluginManager.autoloadplugins();



            var mruFilePath = Path.Combine(Program.appdatafolder, "PLUGINMRU.txt");
            if (System.IO.File.Exists(mruFilePath))
                _mru.AddRange(System.IO.File.ReadAllLines(mruFilePath));

            populateMRU();

        }

        private void MainDockForm_Load(object sender, EventArgs e)
        {
            //read git version string, show in title bar 
            //(https://stackoverflow.com/a/15145121)
            string gitVersion = String.Empty;
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("CanMonitor." + "version.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                gitVersion = reader.ReadToEnd();
            }
            if (gitVersion == "")
            {
                gitVersion = "Unknown";
            }
            this.Text += " -- " + gitVersion;
            this.gitVersion = gitVersion;
        }

        private void addtotopcontrols(DockContent o)
        {

            o.Show(dockPanel1, DockState.DockTop);

        }

        private void Logcontrols_ClearLists(object sender, EventArgs e)
        {
            if (clf != null && !clf.IsDisposed)
                clf.clearlist();

            if (nmtdocument != null && !nmtdocument.IsDisposed)
                nmtdocument.clearlist();

            if (emcydocument != null && !nmtdocument.IsDisposed)
                emcydocument.clearlist();

        }

        private void MainDockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsMgr.writeXML(Path.Combine(Program.appdatafolder, "settings.xml"));

            Program.driverloader.Close();

            var mruFilePath = Path.Combine(Program.appdatafolder, "PLUGINMRU.txt");
            System.IO.File.WriteAllLines(mruFilePath, _mru);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loadPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.RestoreDirectory = false;
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "plugins";

            ofd.Filter = "Libraries (*.dll)|*.dll|CSharp Files (*.cs)|*.cs";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
               // textBox_info.AppendText("Attempting to load plugin " + ofd.FileName + "\r\n");
                Program.pluginManager.loadplugin(ofd.FileName, true);
            }
        }

        private void addtoMRU(string path)
        {
            // if it already exists remove it then let it readd itsself
            // so it will be promoted to the top of the list
            if (_mru.Contains(path))
                _mru.Remove(path);

            _mru.Insert(0, path);

            if (_mru.Count > 10)
                _mru.RemoveAt(10);

            populateMRU();

        }

        private void populateMRU()
        {

            mnuRecentlyUsed.DropDownItems.Clear();

            foreach (var path in _mru)
            {
                var item = new ToolStripMenuItem(path);
                item.Tag = path;
                item.Click += OpenRecentFile;

                mnuRecentlyUsed.DropDownItems.Add(item);
            }
        }

        void OpenRecentFile(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var filepath = (string)menuItem.Tag;
            Program.pluginManager.loadplugin(filepath, true);
        }

        private void saveDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*.xml)|*.xml";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //FIXME link this up
                //dosave(sfd.FileName);
            }

        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Prefs p = new Prefs(Program.appdatafolder, Program.assemblyfolder);
            p.ShowDialog();
        }

        private void connectionControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (connectioncontrol.IsDisposed)
                connectioncontrol = new ConnectionControl();
            connectioncontrol.Show(dockPanel1, DockState.DockTop);
        }

        private void packetControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logcontrols.IsDisposed)
            {
                logcontrols = new LogControls();
                logcontrols.ClearLists += Logcontrols_ClearLists;
            }
            logcontrols.Show(dockPanel1, DockState.DockTop);
        }


        private void canLogToolStripMenuItem_Click(object sender, EventArgs e)
        {

         //TODO more work
    
        }

        private void canEmergencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(emcydocument.IsDisposed)
                emcydocument = new EmcyDocument();
            emcydocument.Show(dockPanel1, DockState.Document);
        }

        private void nMTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(nmtdocument.IsDisposed)
                nmtdocument = new NMTDocument();
            nmtdocument.Show(dockPanel1, DockState.Document);
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(infolog.IsDisposed)
                infolog = new InfoLogDocument();
            infolog.Show(dockPanel1, DockState.Document);
        }
    }
}
