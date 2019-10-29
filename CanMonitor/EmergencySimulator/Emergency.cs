using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;

namespace EmergencySimulator
{
    public class Emergency : InterfaceService, IPDOParser
    {

        EmergencyFrm frm;

        public Emergency()
        {
            addverb("EMCY Injector", "Tools", openform);
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
            if (frm == null || frm.IsDisposed)
            {
                frm = new EmergencyFrm(_lco);
            }
            frm.Show();
        }

    }
}
