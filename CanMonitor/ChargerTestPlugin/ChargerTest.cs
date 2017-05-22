using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDOInterface;
using libCanopenSimple;


namespace ChargerTestPlugin
{
    public class ChargerTest : InterfaceService, IPDOParser
    {

        libCanopenSimple.libCanopenSimple _lco;

        public ChargerTest()
        {
            addverb("Tools", "_root_", null);
            addverb("Charger Test", "Tools", showdlg);

        }

        public void registerPDOS(Dictionary<UInt16, Func<byte[], string>> dic)
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

        public void setlco(libCanopenSimple.libCanopenSimple lco)
        {
            _lco = lco;
        }

        void showdlg(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            ChgrFrm frm = new ChgrFrm(_lco);
            frm.Show();
            
        }

    }
}
