using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using libCanopenSimple;

namespace IO_Node
{
    public class IONode : InterfaceService, IPDOParser
    {

        IOForm frm;
        public IONode()
        {

          

        }
    
        public void registerPDOS()
        {
            addpdohook(0x203, PDO183);
            frm = new IOForm(_lco);
            frm.Show();
        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

        string PDO183(byte[] data)
        {

            frm.updateIN(data[0]);
            return String.Format("IO In %d %d",data[0],data[1]);
        }
    }
}
