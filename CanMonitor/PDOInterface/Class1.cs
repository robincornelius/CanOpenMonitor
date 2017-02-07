using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDOInterface
{
    public interface IPDOParser
    {
        void registerPDOS(Dictionary<UInt16, Func<byte[], string>> dic);
        string decodesdo(int node, int index, int sub, byte[] payload);

    }
}
