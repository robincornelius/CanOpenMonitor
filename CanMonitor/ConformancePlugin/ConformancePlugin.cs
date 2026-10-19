using System;
using System.Windows.Forms;
using PDOInterface;
using WeifenLuo.WinFormsUI.Docking;

namespace ConformancePlugin
{
    public sealed class ConformancePlugin : InterfaceService, IInterfaceService2
    {
        private DockPanel dock;
        private ConformanceForm window;

        public ConformancePlugin()
        {
            addverb("Tools", "_root_", null);
            addverb("CANopen Conformance", "Tools", ShowWindow);
        }

        public void setdockmanager(DockPanel panel)
        {
            dock = panel;
        }

        private void ShowWindow(object sender, EventArgs e)
        {
            if (window == null || window.IsDisposed)
            {
                window = new ConformanceForm(() => _lco);
            }
            if (dock == null)
            {
                window.Show();
            }
            else
            {
                window.Show(dock, DockState.Document);
            }
            window.Activate();
        }
    }
}
