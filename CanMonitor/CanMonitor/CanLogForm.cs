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
        private readonly TextBox filterText = new TextBox { Dock = DockStyle.Fill };
        private readonly Label filterMessage = new Label { AutoSize = true, Dock = DockStyle.Bottom };
        private string appliedExpression = "";
        private Func<PacketLogRow, bool> expression = row => true;
        private PacketLogRow contextRow;

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
            listView1.FullRowSelect = true;
            timer1.Stop(); // Autoscroll happens once per new UI batch, not on a second timer.

            formatter = formatter ?? new PacketLogFormatter();
            pipeline = new PacketDisplayPipeline(formatter.Format);
            display = new VirtualPacketLog(listView1, pipeline);
            CreateFilterControls();
            UpdateFilter();
            Properties.Settings.Default.PropertyChanged += SettingsChanged;

            var strip = new StatusStrip();
            strip.Items.Add(status);
            Controls.Add(strip);
            strip.BringToFront();
            // Dock Fill must be laid out after the top/bottom bars; otherwise
            // the filter bar covers the native ListView column headers.
            listView1.BringToFront();

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
            switch (args.PropertyName)
            {
                case "showNMT": case "showHB": case "showNMTEC":
                case "showsdo": case "showpdo": case "showEMCY":
                    UpdateFilter();
                    break;
            }
        }

        private void UpdateFilter()
        {
            var settings = Properties.Settings.Default;
            var protocols = new PacketDisplayFilter(settings.showNMT, settings.showHB,
                settings.showNMTEC, settings.showsdo, settings.showpdo, settings.showEMCY);
            var compiled = expression;
            // Retain decoded rows independently of the display selection. Changing
            // a filter can reveal history without recapture or plugin re-decoding.
            display.SetFilter(row => protocols.Matches(row.Record) && compiled(row));
        }

        private void CreateFilterControls()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(4) };
            var line = new Panel { Dock = DockStyle.Top, Height = 27 };
            var apply = new Button { Text = "Apply", Dock = DockStyle.Right, Width = 65 };
            var clear = new Button { Text = "Clear filter", Dock = DockStyle.Right, Width = 85 };
            var help = new Button { Text = "Help", Dock = DockStyle.Right, Width = 55 };
            line.Controls.Add(filterText);
            line.Controls.Add(apply);
            line.Controls.Add(clear);
            line.Controls.Add(help);
            panel.Controls.Add(line);
            panel.Controls.Add(filterMessage);
            Controls.Add(panel);
            panel.BringToFront();
            filterText.AccessibleName = "Display filter";
            filterMessage.Text = "Enter a display filter, or right-click a packet to build an expression. Protocol checkboxes also apply.";
            apply.Click += (sender, args) => ApplyDisplayFilter(filterText.Text);
            clear.Click += (sender, args) => ApplyDisplayFilter("");
            help.Click += (sender, args) => MessageBox.Show(this,
                "Examples:\nnot (type == PDO and cobid == 0x182)\nnode == 2 and length >= 4\n" +
                "type == SDO and sdo.index == 0x2000\ndata[0] == 0x80 or info contains \"abort\"\n\n" +
                "Fields: type, cobid (cob), node (nodeid), length (len), data[0] through data[7], " +
                "sdo.index, sdo.subindex, emcy.code, payload, info.\n" +
                "Types: PDO, SDO, NMT, NMTEC (HEARTBEAT), EMCY.\n" +
                "Numbers: decimal or 0x hex. Comparisons: ==, =, !=, <, <=, >, >=.\n" +
                "Text: ==, !=, contains; quote info/payload values. Text matches ignore case.\n" +
                "Combine with and, or, not (also &&, ||, !), and parentheses.\n" +
                "Absent fields do not match, including !=. SDO index fields apply only to initiate/abort packets.\n\n" +
                "Apply updates retained history and live traffic in batches. Checkboxes narrow the view too. " +
                "Plugin-suppressed rows remain suppressed. Clear filter preserves protocol checkboxes.\n" +
                "Save exports the matching retained rows; automatic logging exports all retained rows.",
                "Packet display filters");
            filterText.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Enter)
                {
                    ApplyDisplayFilter(filterText.Text);
                    args.SuppressKeyPress = true;
                }
            };
            filterText.TextChanged += (sender, args) =>
            {
                if (filterText.Text != appliedExpression)
                {
                    filterMessage.Text = "Edited filter — press Enter or Apply. The previous filter is still active.";
                    filterText.BackColor = System.Drawing.SystemColors.Window;
                }
            };

            var menu = new ContextMenuStrip(components);
            menu.Items.Add("Show only this type and COB-ID", null, (sender, args) => ContextFilter(false, true));
            menu.Items.Add("Exclude this type and COB-ID", null, (sender, args) => ContextFilter(true, false));
            menu.Items.Add("Also match this type and COB-ID", null, (sender, args) => ContextFilter(false, false));
            menu.Items.Add("Exclude this COB-ID (all types)", null, (sender, args) =>
            {
                if (contextRow != null)
                {
                    ApplyDisplayFilter(PacketFilterExpression.Combine(appliedExpression,
                        "cobid == 0x" + contextRow.Record.CobId.ToString("X3"), true));
                }
            });
            menu.Opening += (sender, args) => args.Cancel = contextRow == null;
            listView1.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Apps || (args.Shift && args.KeyCode == Keys.F10))
                {
                    contextRow = listView1.SelectedIndices.Count == 0 ? null : display.RowAt(listView1.SelectedIndices[0]);
                }
            };
            listView1.MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButtons.Right)
                {
                    ListViewItem item = listView1.GetItemAt(args.X, args.Y);
                    contextRow = item == null ? null : display.RowAt(item.Index);
                }
            };
            listView1.ContextMenuStrip = menu;
        }

        internal bool ApplyDisplayFilter(string text)
        {
            try
            {
                var compiled = PacketFilterExpression.Compile(text);
                expression = compiled;
                appliedExpression = text.Trim();
                filterText.Text = appliedExpression;
                filterText.BackColor = System.Drawing.Color.Honeydew;
                filterMessage.Text = appliedExpression.Length == 0 ? "Expression cleared; protocol checkboxes still apply."
                    : "Applied: " + appliedExpression;
                UpdateFilter();
                return true;
            }
            catch (FormatException error)
            {
                filterText.BackColor = System.Drawing.Color.MistyRose;
                filterMessage.Text = error.Message + " Previous filter remains active.";
                return false;
            }
        }

        private void ContextFilter(bool exclude, bool replace)
        {
            if (contextRow != null)
            {
                string condition = PacketFilterExpression.PacketCondition(contextRow.Record);
                ApplyDisplayFilter(PacketFilterExpression.Combine(replace ? "" : appliedExpression, condition, exclude));
            }
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
                if (display.History.Count == 0 || !rotatingRows.Contains(display.History[0]))
                {
                    rotatingRows = null;
                }
            }
            display.Drain(1000, 4, settings.limitlines ? (int?)Math.Max(0, settings.linelimit) : null,
                settings.autoscroll);
            status.Text = string.Format("Retained: {0:N0}   Pending decode: {1:N0}   Pending display: {2:N0}   Plugin suppressed: {3:N0}   Decode errors: {4:N0}   Shown: {5:N0} {6}   {7}",
                display.History.Count, pipeline.PendingInput, pipeline.PendingOutput, pipeline.Filtered, pipeline.Errors,
                display.VisibleCount, display.IsRebuilding ? "(filtering...)" : "", fileStatus);
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
            if (!rotate && display.IsRebuilding)
            {
                fileStatus = "Filter is updating; retry Save when filtering completes.";
                return;
            }
            PacketLogRow[] snapshot = rotate ? display.History.Snapshot() : display.VisibleSnapshot();
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
