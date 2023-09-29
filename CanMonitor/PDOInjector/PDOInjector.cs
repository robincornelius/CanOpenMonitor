using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;

namespace PDOInjector
{
    public class PDOInjector : InterfaceService, IPDOParser, IInterfaceService2
    {

        PDOForm frm;
        DockPanel dp;

        void IInterfaceService2.setdockmanager(DockPanel _dp)
        {
            this.dp = _dp;
        }


        public PDOInjector()
        {
            addverb("PDO Injector", "Tools", openform);
        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

        public void endsdo(int node, int index, int sub, byte[] payload)
        {

        }

        public void registerPDOS()
        {

        }

        public void openform(object sender, System.EventArgs e)
        {
            if(frm==null || frm.IsDisposed)
            {
                frm = new PDOForm(_lco); 
            }
            frm.Show(dp,DockState.DockRight);
        }
    }
}
