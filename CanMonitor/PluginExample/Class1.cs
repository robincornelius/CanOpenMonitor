using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using System.Windows.Forms;
using libCanopenSimple;


namespace PluginExample
{
    public class PluginExample : InterfaceService, IPDOParser
    {

        static private libCanopenSimple.libCanopenSimple _lco;

        public PluginExample()
        {
            addverb("hello", "Tools", sayhello);

        }


        void sayhello(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;


            MessageBox.Show("HELLO WORLD");
            
            _lco.NMT_start();

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




    }


   
}
