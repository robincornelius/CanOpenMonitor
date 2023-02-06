using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;

namespace FlashLoader
{
    public class FlashLoader : InterfaceService, IPDOParser
    {
        public FlashLoader()
        {
            addverb("---", "File", null);
            addverb("Flash node", "File", showdlg);
            addverb("---", "File", null);
        }

        public void registerPDOS()
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

        public void endsdo(int node, int index, int sub, byte[] payload)
        {

        }

        public void showdlg(object sender, System.EventArgs e)
        {

            if (_lco == null || !_lco.isopen())
                return;

            Flasher f = new Flasher(_lco);
            f.ShowDialog();
        }
    }

}

