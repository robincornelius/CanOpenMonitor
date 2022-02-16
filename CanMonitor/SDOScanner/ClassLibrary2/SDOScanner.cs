using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;

namespace ClassLibrary2
{
    public class SDOScanner : InterfaceService, IPDOParser
    {
        public SDOScanner()
        {
            //addverb("SDO", "_root_", null);
           
        }

        public void registerPDOS()
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

    


    }

  
}
