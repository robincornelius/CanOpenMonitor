using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ConformancePlugin
{
    public sealed class ConformanceForm : DockContent
    {
        private readonly Func<libCanopenSimple.libCanopenSimple> connection;
        private readonly TextBox file = new TextBox { Width = 340, ReadOnly = true };
        private readonly NumericUpDown node = new NumericUpDown { Minimum = 1, Maximum = 127, Value = 1, Hexadecimal = true, Width = 65 };
        private readonly CheckBox dictionary = new CheckBox { Text = "Read described objects", Checked = true, AutoSize = true };
        private readonly CheckBox probes = new CheckBox { Text = "SDO abort/recovery probes", Checked = true, AutoSize = true };
        private readonly CheckBox nmt = new CheckBox { Text = "Exercise NMT states (interrupts operation)", AutoSize = true };
        private readonly Button browse = new Button { Text = "Load XDD/EDS", AutoSize = true };
        private readonly Button run = new Button { Text = "Run checks", AutoSize = true };
        private readonly Button cancel = new Button { Text = "Cancel", Enabled = false };
        private readonly Button export = new Button { Text = "Save report", Enabled = false, AutoSize = true };
        private readonly Label status = new Label { AutoSize = true, Text = "Load a description and select the target node (hex). Uses the current CanMonitor connection." };
        private readonly DataGridView results = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 150 };
        private readonly System.Collections.Concurrent.ConcurrentQueue<CheckResult> pending = new System.Collections.Concurrent.ConcurrentQueue<CheckResult>();
        private CancellationTokenSource cancellation;
        private DeviceDescription description;
        private ConformanceReport report;

        public ConformanceForm(Func<libCanopenSimple.libCanopenSimple> connection)
        {
            this.connection = connection;
            Text = "CANopen Conformance";
            Width = 1100;
            Height = 700;
            AutoScaleMode = AutoScaleMode.Dpi;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var controls = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            controls.Controls.AddRange(new Control[] { browse, file, new Label { Text = "Node 0x", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, node, run, cancel, export });
            var choices = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            choices.Controls.AddRange(new Control[] { dictionary, probes, nmt });
            layout.Controls.Add(controls, 0, 0);
            layout.Controls.Add(choices, 0, 1);
            layout.Controls.Add(new Label { Text = ConformanceReport.Scope, AutoSize = true, ForeColor = Color.DarkGoldenrod }, 0, 2);
            layout.Controls.Add(results, 0, 3);
            layout.Controls.Add(status, 0, 4);
            Controls.Add(layout);
            results.Columns.Add("Check", "Check");
            results.Columns.Add("Verdict", "Result");
            results.Columns.Add("Evidence", "Evidence");
            results.Columns[0].FillWeight = 30;
            results.Columns[1].FillWeight = 10;
            results.Columns[2].FillWeight = 60;
            browse.Click += LoadFile;
            run.Click += Run;
            cancel.Click += (s, e) => cancellation?.Cancel();
            export.Click += Save;
            timer.Tick += (s, e) => Drain();
            timer.Start();
            FormClosing += (s, e) =>
            {
                if (cancellation != null)
                {
                    e.Cancel = true;
                    cancellation.Cancel();
                    status.Text = "Cancelling; waiting for any NMT state restoration before closing. Close again when complete.";
                }
            };
        }

        private void LoadFile(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "CANopen descriptions|*.xdd;*.eds;*.dcf" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    description = DeviceDescription.Load(dialog.FileName);
                    file.Text = dialog.FileName;
                    status.Text = description.Dictionary.ods.Count + " described objects loaded. Node is hexadecimal; confirm the connected bitrate in CanMonitor.";
                }
                catch (Exception error)
                {
                    MessageBox.Show(this, error.Message, "Description load failed");
                }
            }
        }

        private async void Run(object sender, EventArgs e)
        {
            if (description == null)
            {
                MessageBox.Show(this, "Load the device XDD or EDS first.");
                return;
            }
            var can = connection();
            if (can == null || !can.isopen())
            {
                MessageBox.Show(this, "Connect CanMonitor to the device first.");
                return;
            }
            RunOptions options = new RunOptions { Node = (byte)node.Value, ReadDictionary = dictionary.Checked, AbortProbes = probes.Checked, NmtTransitions = nmt.Checked };
            CheckResult previous;
            while (pending.TryDequeue(out previous))
            {
                // A completed large run may still have rows waiting for the UI timer.
            }
            results.Rows.Clear();
            report = null;
            SetRunning(true);
            cancellation = new CancellationTokenSource();
            try
            {
                using (var transport = new CanopenTransport(can, options.Node))
                {
                    var runner = new ConformanceRunner(transport) { Progress = result => pending.Enqueue(result) };
                    report = await Task.Run(() => runner.Run(description, options, cancellation.Token));
                }
                Drain();
                status.Text = (report.Cancelled ? "Cancelled. " : "Complete. ") + string.Join("; ", Enum.GetValues(typeof(Verdict)).Cast<Verdict>().Select(v => v + ": " + report.Results.Count(r => r.Verdict == v)));
            }
            catch (Exception error)
            {
                status.Text = "Run failed: " + error.Message;
            }
            finally
            {
                cancellation.Dispose();
                cancellation = null;
                SetRunning(false);
            }
        }

        private void SetRunning(bool active)
        {
            run.Enabled = browse.Enabled = node.Enabled = dictionary.Enabled = probes.Enabled = nmt.Enabled = !active;
            cancel.Enabled = active;
            export.Enabled = !active && report != null;
            if (active)
            {
                status.Text = "Running checks on a worker thread; results update in batches.";
            }
        }

        private void Drain()
        {
            CheckResult result;
            int batch = 0;
            while (batch++ < 100 && pending.TryDequeue(out result))
            {
                int row = results.Rows.Add(result.Check, result.Verdict, result.Evidence);
                results.Rows[row].DefaultCellStyle.ForeColor = result.Verdict == Verdict.Fail || result.Verdict == Verdict.Error ? Color.DarkRed : result.Verdict == Verdict.Warning ? Color.DarkGoldenrod : SystemColors.ControlText;
            }
        }

        private void Save(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Filter = "HTML report|*.html", FileName = "CANopen-report-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".html" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        report.SaveHtml(dialog.FileName);
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(this, error.Message, "Report save failed");
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                cancellation?.Cancel();
            }
            base.Dispose(disposing);
        }
    }
}
