using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;
using WeifenLuo.WinFormsUI.Docking;

namespace NMTPlugin
{
    public class NMTPlugin : InterfaceService, IInterfaceService2, IPDOParser
    {
        DockPanel dp;
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

        void IInterfaceService2.setdockmanager(DockPanel _dp)
        {
            this.dp = _dp;
        }

        public void endsdo(int node, int index, int sub, byte[] payload)
        {

        }
        public void registerPDOS()
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
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
            FRM.Show(dp, DockState.DockLeft);
        }
    }


}
