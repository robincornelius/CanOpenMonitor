using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;

namespace NMTPlugin
{
    public class NMTPlugin : InterfaceService, IPDOParser
    {
        libCanopenSimple.libCanopenSimple _lco;

        public NMTPlugin()
        {
            addverb("NMT", "_root_", null);
            addverb("Start Bus", "NMT", startbus);
            addverb("Pre-op Bus", "NMT", preopbus);
            addverb("Stop Bus", "NMT", stopbus);
            addverb("Reset Bus", "NMT", resetbus);
            addverb("Reset Communication", "NMT", resetcomms);
            addverb("---", "NMT", null);
            addverb("Advanced", "NMT", showdlg);

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

        void startbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_start();
        }

        void preopbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_preop();
        }

        void stopbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_stop();
        }

        void resetbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_ResetNode();
        }

        void resetcomms(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_ResetComms();
        }

        void showdlg(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            NMTFrm FRM = new NMTFrm(_lco);
            FRM.Show();
        }
    }


}
