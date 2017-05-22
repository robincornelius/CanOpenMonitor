using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libCanopenSimple;
using PDOInterface;

namespace eeprom_plugin
{
    public class eeprom_plugin : InterfaceService, IPDOParser
    {
        libCanopenSimple.libCanopenSimple _lco;

        public eeprom_plugin()
        {
            addverb("Tools", "_root_", null);
            addverb("Reset EEprom", "Tools", showdlg);
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

        public void showdlg(object sender, System.EventArgs e)
        {

            if (_lco == null || !_lco.isopen())
                return;

            ResetEEPROM re = new ResetEEPROM(_lco);
            re.Show();
        }

    }
}
