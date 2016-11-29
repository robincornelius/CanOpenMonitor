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

        }

        enum Estates
        {
            BOOT = 0,
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
            ENTER_ABORT,
            ABORT, //4
            DO_DISCHARGE,
            POST_DISHCARGE,
            DISCHARGE_VERIFY,
            DUMP,
            DUMP_VERIFY
        };

        enum EController_states
        {
            CTRL_BOOT = 0,
            CTRL_WAIT_ON,
            CTRL_RUN,
            CTRL_MAGNETISE,
            CTRL_LIVE,
            CTRL_CHARGE,
            CTRL_WAITCOMPLETE,
            CTRL_ALARM,
        };


        public static string PDO181(byte[] data)
        {
            string msg = "";

            Estates chargerstate = (Estates)data[0];
            EController_states controller_state = (EController_states)data[1];

            Int16 vcaps = BitConverter.ToInt16(data, 2);

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
    }
}
