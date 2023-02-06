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
        public eeprom_plugin()
        {
            addverb("Tools", "_root_", null);
            addverb("Reset EEprom", "Tools", showdlg);
        }

        public void registerPDOS()
        {

        }
        public void endsdo(int node, int index, int sub, byte[] payload)
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            if(index==0x1010)
            {
                return "STORE_PARAM_FUNC";
            }

            if(index==0x1011)
            {      
                return "RESET_PARAM_FUNC";
            }

            return "";
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
