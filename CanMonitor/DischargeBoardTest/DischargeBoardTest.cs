using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;

namespace DischargeBoardTest
{
    public class DischargeBoardTest : InterfaceService, IPDOParser
    {

        libCanopenSimple.libCanopenSimple _lco;

        public DischargeBoardTest()
        {
            addverb("Tools", "_root_", null);
            addverb("Discharge Board", "Tools", showdlg);

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

            DischargeFrm frm = new DischargeFrm(_lco);
            frm.Show();

        }

    }
}
