using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;

namespace PDOInjector
{
    public class PDOInjector : InterfaceService, IPDOParser
    {

        PDOForm frm;

        public PDOInjector()
        {
            addverb("PDO Injector", "Tools", openform);
        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
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
            frm.Show();
        }
    }
}
