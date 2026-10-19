using CanMonitor.PacketDisplay;
using libCanopenSimple;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CanMonitor
{
    public partial class EmcyDocument : DockContent, ICanDocument
    {
        private readonly libCanopenSimple.libCanopenSimple source;
        private readonly PacketDisplayPipeline pipeline;
        private readonly VirtualPacketLog display;
        private readonly ToolStripStatusLabel status = new ToolStripStatusLabel();

        public EmcyDocument() : this(Program.lco)
        {
        }

        internal EmcyDocument(libCanopenSimple.libCanopenSimple source)
        {
            this.source = source;
            InitializeComponent();
            listView_emcy.DoubleBuffering(true);
            var formatter = new PacketLogFormatter();
            pipeline = new PacketDisplayPipeline(formatter.Format);
            display = new VirtualPacketLog(listView_emcy, pipeline, new[] { 0, 2, 3, 5 });
            var strip = new StatusStrip();
            strip.Items.Add(status);
            Controls.Add(strip);
            strip.BringToFront();
            source.emcyevent += log_EMCY;
            timer1.Interval = 50;
            timer1.Start();
            Disposed += Detach;
        }

        public void clearlist()
        {
            display.Clear();
        }

        private void log_EMCY(canpacket packet, DateTime timestamp)
        {
            pipeline.Enqueue(new PacketLogRecord(PacketLogKind.Emcy, packet.cob, timestamp, packet.data));
        }

        private void timer1_Tick(object sender, EventArgs args)
        {
            var settings = Properties.Settings.Default;
            display.Drain(1000, 4, settings.limitlines ? (int?)Math.Max(0, settings.linelimit) : null, false);
            status.Text = string.Format("Retained: {0:N0}   Pending: {1:N0}   Decode errors: {2:N0}",
                display.History.Count, pipeline.PendingInput + pipeline.PendingOutput, pipeline.Errors);
            status.ToolTipText = pipeline.LastError == null ? "Pending entries are preserved." : pipeline.LastError.ToString();
        }

        private void Detach(object sender, EventArgs args)
        {
            source.emcyevent -= log_EMCY;
            timer1.Stop();
            display.Dispose();
        }
    }
}
