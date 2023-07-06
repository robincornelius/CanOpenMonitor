using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDOInterface;
using libCanopenSimple;
using WeifenLuo.WinFormsUI.Docking;

namespace SDOEditorPlugin
{
    public class SDOEditorPlugin : InterfaceService, IInterfaceService2, IPDOParser
    {

        private DockPanel dp;

        public SDOEditorPlugin()
        {
            addverb("Tools", "_root_", null);
            addverb("Device OD Editor", "Tools", showdlg);
        }

        public void registerPDOS()
        {

        }

        public void endsdo(int node, int index, int sub, byte[] payload)
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

      
        void showdlg(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            SDOEditor ed = new SDOEditor(_lco);
            ed.Show(dp, DockState.Document);
        }

        public void setdockmanager(DockPanel dp)
        {
            this.dp = dp;
        }
    }
}
