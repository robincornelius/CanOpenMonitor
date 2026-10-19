using CanMonitor.PacketDisplay;
using libCanopenSimple;
using N_SettingsMgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CanMonitor
{
    public partial class CanLogForm : DockContent, ICanDocument
    {
        public DockPanel dockpanel;
        private readonly libCanopenSimple.libCanopenSimple source;
        private readonly PacketDisplayPipeline pipeline;
        private readonly VirtualPacketLog display;
        private readonly System.Windows.Forms.Timer updateTimer;
        private readonly System.Windows.Forms.Timer saveTimer;
        private readonly ToolStripStatusLabel status = new ToolStripStatusLabel();
        private readonly CancellationTokenSource loading = new CancellationTokenSource();
        private Task pendingSave = Task.CompletedTask;
        private DateTime lastSaveHour = DateTime.Now;
        private string fileStatus = "";
        private bool detached;
        private HashSet<PacketLogRow> rotatingRows;

        internal Task PendingSave
        {
            get
            {
                return pendingSave;
            }
        }

        public CanLogForm() : this(Program.lco, CreateFormatter())
        {
            try
            {
                SettingsMgr.readXML(Path.Combine(Program.appdatafolder, "settings.xml"));
            }
            catch (Exception)
            {
                // Keep defaults if no previous settings exist.
            }
        }

        private static PacketLogFormatter CreateFormatter()
        {
            return new PacketLogFormatter(Program.pluginManager.GetPdoDecoder, Program.pluginManager.DecodeSdo);
        }

        internal CanLogForm(libCanopenSimple.libCanopenSimple source, PacketLogFormatter formatter)
        {
            this.source = source;
            InitializeComponent();
            listView1.DoubleBuffering(true);
            listView1.ListViewItemSorter = null;
            timer1.Stop(); // Autoscroll happens once per new UI batch, not on a second timer.

            formatter = formatter ?? new PacketLogFormatter();
            pipeline = new PacketDisplayPipeline(formatter.Format);
            display = new VirtualPacketLog(listView1, pipeline);
            UpdateFilter();
            Properties.Settings.Default.PropertyChanged += SettingsChanged;

            var strip = new StatusStrip();
            strip.Items.Add(status);
            Controls.Add(strip);
            strip.BringToFront();

            source.nmtecevent += log_NMTEC;
            source.nmtevent += log_NMT;
            source.sdoevent += log_SDO;
            source.pdoevent += log_PDO;
            source.emcyevent += log_EMCY;

            updateTimer = new System.Windows.Forms.Timer(components) { Interval = 50, Enabled = true };
            updateTimer.Tick += updatetimer_Tick;
            saveTimer = new System.Windows.Forms.Timer(components) { Interval = 1000, Enabled = true };
            saveTimer.Tick += Savetimer_Tick;
            Disposed += Detach;
            Text = "CanOpen Log";
        }

        private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            var settings = Properties.Settings.Default;
            pipeline.Filter = new PacketDisplayFilter(settings.showNMT, settings.showHB,
                settings.showNMTEC, settings.showsdo, settings.showpdo, settings.showEMCY);
        }

        private void EnqueuePacket(PacketLogKind kind, canpacket packet, DateTime timestamp)
        {
            pipeline.Enqueue(new PacketLogRecord(kind, packet.cob, timestamp, packet.data));
        }

        private void log_NMT(canpacket packet, DateTime timestamp)
        {
            EnqueuePacket(PacketLogKind.Nmt, packet, timestamp);
        }
        private void log_NMTEC(canpacket packet, DateTime timestamp)
        {
            EnqueuePacket(PacketLogKind.Heartbeat, packet, timestamp);
        }
        private void log_SDO(canpacket packet, DateTime timestamp)
        {
            EnqueuePacket(PacketLogKind.Sdo, packet, timestamp);
        }
        private void log_EMCY(canpacket packet, DateTime timestamp)
        {
            EnqueuePacket(PacketLogKind.Emcy, packet, timestamp);
        }

        private void log_PDO(canpacket[] packets, DateTime timestamp)
        {
            foreach (canpacket packet in packets)
            {
                EnqueuePacket(PacketLogKind.Pdo, packet, timestamp);
            }
        }

        public void clearlist()
        {
            rotatingRows = null;
            display.Clear();
        }

        private void updatetimer_Tick(object sender, EventArgs args)
        {
            var settings = Properties.Settings.Default;
            if (rotatingRows != null)
            {
                display.History.RemoveExportedPrefix(rotatingRows, 1000);
                listView1.VirtualListSize = display.History.Count;
                listView1.Invalidate();
                if (display.History.Count == 0 || !rotatingRows.Contains(display.History[0]))
                {
                    rotatingRows = null;
                }
            }
            display.Drain(1000, 4, settings.limitlines ? (int?)Math.Max(0, settings.linelimit) : null,
                settings.autoscroll);
            status.Text = string.Format("Retained: {0:N0}   Pending decode: {1:N0}   Pending display: {2:N0}   Filtered: {3:N0}   Decode errors: {4:N0}   {5}",
                display.History.Count, pipeline.PendingInput, pipeline.PendingOutput, pipeline.Filtered, pipeline.Errors, fileStatus);
            status.ToolTipText = pipeline.LastError == null ? "Pending entries are preserved; memory can grow if decoding or display falls behind."
                : pipeline.LastError.ToString();
        }

        private async void Savetimer_Tick(object sender, EventArgs args)
        {
            DateTime now = DateTime.Now;
            if (!Properties.Settings.Default.AutoFileLog || !pendingSave.IsCompleted
                || (now.Date == lastSaveHour.Date && now.Hour == lastSaveHour.Hour))
            {
                return;
            }
            lastSaveHour = now;
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Properties.Settings.Default.FileLogFolder);
            await SaveAsync(Path.Combine(folder, now.ToString("dd-MMM-yyyy-HH-mm-ss") + ".xml"), true);
        }

        public async void dosave(string filename)
        {
            await SaveAsync(filename, false);
        }

        private async Task SaveAsync(string filename, bool rotate)
        {
            if (!pendingSave.IsCompleted)
            {
                fileStatus = "An export is already running.";
                return;
            }
            PacketLogRow[] snapshot = display.History.Snapshot();
            var columns = new string[listView1.Columns.Count];
            for (int index = 0; index < columns.Length; index++)
            {
                columns[index] = listView1.Columns[index].Text;
            }
            string foreground = listView1.ForeColor.Name;
            string background = listView1.BackColor.Name;
            fileStatus = "Exporting retained rows...";
            Task<HashSet<PacketLogRow>> export = Task.Run(() =>
            {
                PacketLogFile.Save(filename, snapshot, columns, foreground, background);
                return rotate ? new HashSet<PacketLogRow>(snapshot) : null;
            });
            pendingSave = export;
            try
            {
                HashSet<PacketLogRow> exported = await export;
                if (IsDisposed)
                {
                    return;
                }
                if (rotate)
                {
                    // Retire saved rows in bounded batches on later UI ticks.
                    // New rows and both pending queues remain untouched.
                    rotatingRows = exported;
                }
                fileStatus = "Exported " + snapshot.Length.ToString("N0") + " retained rows.";
            }
            catch (Exception error)
            {
                fileStatus = "Export failed: " + error.Message;
            }
        }

        public async void doload()
        {
            using (var dialog = new OpenFileDialog { ShowHelp = true, Filter = "(*.xml)|*.xml" })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                clearlist();
                fileStatus = "Loading...";
                try
                {
                    await Task.Run(() => PacketLogFile.LoadXml(dialog.FileName, pipeline.Enqueue, loading.Token));
                    fileStatus = "Loaded; queued entries are being decoded.";
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception error)
                {
                    fileStatus = "Load failed: " + error.Message;
                }
            }
        }

        public async void doload2()
        {
            using (var dialog = new OpenFileDialog { ShowHelp = true, Filter = "(*.log)|*.log" })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                clearlist();
                try
                {
                    await Task.Run(() => PacketLogFile.LoadCsv(dialog.FileName, pipeline.Enqueue, loading.Token));
                    fileStatus = "Loaded; queued entries are being decoded.";
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception error)
                {
                    fileStatus = "Load failed: " + error.Message;
                }
            }
        }

        private void Detach(object sender, EventArgs args)
        {
            if (detached)
            {
                return;
            }
            detached = true;
            source.nmtecevent -= log_NMTEC;
            source.nmtevent -= log_NMT;
            source.sdoevent -= log_SDO;
            source.pdoevent -= log_PDO;
            source.emcyevent -= log_EMCY;
            Properties.Settings.Default.PropertyChanged -= SettingsChanged;
            loading.Cancel();
            updateTimer.Dispose();
            saveTimer.Dispose();
            display.Dispose();
        }

        // Retained designer event bindings; scrolling is handled by the batch drain.
        private void timer1_Tick(object sender, EventArgs args)
        {
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs args)
        {
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs args)
        {
        }
        private void Form1_Resize(object sender, EventArgs args)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "Can monitor is still running";
                notifyIcon1.BalloonTipTitle = "Can Monitor";
                notifyIcon1.ShowBalloonTip(2000);
            }
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs args)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
    }
}
