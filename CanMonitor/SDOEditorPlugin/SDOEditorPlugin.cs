using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDOInterface;
using libCanopenSimple;

namespace SDOEditorPlugin
{
    public class SDOEditorPlugin : InterfaceService, IPDOParser
    {
        
        public SDOEditorPlugin()
        {
            addverb("Tools", "_root_", null);
            addverb("SDO Editor", "Tools", showdlg);
        }

        public void registerPDOS()
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
            ed.Show();
        }

    }
}
