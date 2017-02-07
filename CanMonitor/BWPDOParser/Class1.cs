using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;

namespace PDOParser
{
   
    public class PDO : IPDOParser
    {
        public void registerPDOS(Dictionary<UInt16, Func<byte[], string>> dic)
        {
            dic.Add(0x181, PDO181);
            dic.Add(0x183, PDO183);
            dic.Add(0x203, PDO203);
            dic.Add(0x190, PDO190);
            dic.Add(0x204, PDO204);
            dic.Add(0x304, PDO304);
            dic.Add(0x404, PDO404);
        
        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";

        }

        enum Estates
        {
            BOOT = 0,
            HW_FAULT,
            ENTER_ABORT,
            ABORT, //4
    
            SELF_CHECK, //1
            WAIT_SAFE, //2
            SAFE_RST,
            ENTER_WAIT_CONTROLS, //4
            WAIT_CONTROLS,
            IDLE,
            LIVE,
            LIVE_CONTACTOR_DELAY,
            CHARGING,
            CHARGED,
            DO_DISCHARGE,
            POST_DISHCARGE,
            DISCHARGE_VERIFY,
            DUMP,
            DUMP_VERIFY
        };

        enum EController_states
        {
            CTRL_BOOT = 0,
            CTRL_ALARM,
            CTRL_WAIT_ON,
            CTRL_INTERLOCK,
            CTRL_RUN,
            CTRL_MAGNETISE,
            CTRL_LIVE,
            CTRL_CHARGE,
            CTRL_WAITCOMPLETE,
        };


        static Estates lastcharger;
        static EController_states lastcontroller;

        public static string PDO181(byte[] data)
        {
            string msg = "";

            Estates chargerstate = (Estates)data[0];
            EController_states controller_state = (EController_states)data[1];
            Int16 vcaps = BitConverter.ToInt16(data, 2);

            if (lastcharger == chargerstate && lastcontroller == controller_state && vcaps < 50)
                return null;

            lastcharger = chargerstate;
            lastcontroller = controller_state;

            msg = string.Format("Charger = {0} - Master = {1} - Caps = {2}", chargerstate.ToString(), controller_state.ToString(),vcaps);

            return msg;

        }

        public static string PDO183(byte[] data)
        {
            string msg = "";

            msg = String.Format("IN {0} {1}", Convert.ToString(data[0], 2), Convert.ToString(data[1], 2));

            return msg;
        }

        public static string PDO203(byte[] data)
        {
            string msg = "";

            msg = String.Format("OUT {0}", Convert.ToString(data[0], 2));

            return msg;
        }

        public static string PDO190(byte[] data)
        {
            
            return String.Format("Target {0} Phase {1}", BitConverter.ToInt16(data,0),BitConverter.ToInt32(data,2));

        }

        public static string PDO204(byte[] data)
        {
            return "saftey unlock";
        }
        public static string PDO304(byte[] data)
        {

            return string.Format("Fire inthe hole FX {0}", data[0]); ;
        }
        public static string PDO404(byte[] data)
        {

            return string.Format("Select FX {0} vol {1}", data[0],BitConverter.ToUInt16(data,1));
        }

      


    }
}
