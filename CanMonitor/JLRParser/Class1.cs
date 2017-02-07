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
            dic.Add(0x205, PDO205);
            dic.Add(0x206, PDO206);
            dic.Add(0x185, PDO185);
            dic.Add(0x200, PDO200);

        }

        public string decodesdo(int node,int index,int sub ,byte[] payload)
        {
            string ret = "";

            if (node >= 0x580 && node < 0x600)
            {
                node = node - 0x580;

                //expidited handler
                byte[] payload2 = new byte[4];

                payload2[0] = payload[4];
                payload2[1] = payload[5];
                payload2[2] = payload[6];
                payload2[3] = payload[7];


                if (node >= 0x08 && node <= 0x0d)
                {
                    //gaussmeter

                    if (index == 0x6413)
                    {
                        float val = BitConverter.ToSingle(payload2, 0);
                        ret = string.Format(" GM {0} CHAN {1} = {2} T", node, sub, val);
                        return ret;
                    }

                }


                if (node == 0x07)
                {
                    float val = BitConverter.ToSingle(payload2, 0);
                    ret = string.Format("TEMP CHAN {0} = {1} T", sub, val);
                    return ret;
                }
            }


            return ret;
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
        CTRL_BOOT=0,
        CTRL_ALARM, //1
        CTRL_WAIT_ON, //2
        CTRL_INTERLOCK, //3
        CTRL_RUN, //4
        CTRL_WAITOUT,
        CTRL_WAITIN,
        CTRL_WAITUP,
        CTRL_WAITDOWN,
        CTRL_WAITSTART,
        
        CTRL_MAGNETISE, //5
        CTRL_LIVE, //6
        CTRL_STARTCHARGE,
        CTRL_CHARGE, //7       
        CTRL_THYMUX,
        CTRL_DISCHARGE,
        CTRL_INDEXNEXT,
        CTRL_WAITCOMPLETE, //8
        CTRL_READGAUSSMETERS,
        
 } ;

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

            msg = string.Format("Charger = {0} - Master = {1} - Caps = {2}", chargerstate.ToString(), controller_state.ToString(), vcaps);

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

            return String.Format("Target {0} Phase {1}", BitConverter.ToInt16(data, 0), BitConverter.ToInt32(data, 2));

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

            return string.Format("Select FX {0} vol {1}", data[0], BitConverter.ToUInt16(data, 1));
        }

        public static string PDO206(byte[] data)
        {
            return string.Format("WRITE DI24");

        }

        public static string PDO205(byte[] data)
        {
            string msg = "OUT (";
            if ((data[0] & (byte)0x01) != 0)
                msg += "UP ";
            if ((data[0] & (byte)0x02) != 0)
                msg += "DOWN ";
            if ((data[0] & (byte)0x04) != 0)
                msg += "IN ";
            if ((data[0] & (byte)0x08) != 0)
                msg += "PASS ";
            if ((data[0] & (byte)0x10) != 0)
                msg += "FAIL ";
            if ((data[0] & (byte)0x20) != 0)
                msg += "READY";
            if ((data[0] & (byte)0x40) != 0)
                msg += "FAULT";
            if ((data[0] & (byte)0x80) != 0)
                msg += "";

            msg += ")";

            return msg;
      
        }

        public static string PDO185(byte[] data)
        {
            string msg="IN (";

            if ((data[0] & (byte)0x01) !=0)
                msg += "UP ";
            
            if ((data[0] & (byte)0x02) != 0)
                msg += "DOWN ";
            
            if ((data[0] & (byte)0x04)!=0)
                msg += "IN ";
            
            if ((data[0] & (byte)0x08)!=0)
                msg += "";
            
            if ((data[0] & (byte)0x10)!=0)
                msg += "OUT ";
            
            if ((data[0] & (byte)0x20)!=0)
                msg += "START";
            
            if ((data[0] & (byte)0x40)!=0)
                msg += "";
            if ((data[0] & (byte)0x80)!=0)
                msg += "";

            msg += ")";

             
      

            return msg;
        }

        public static string PDO200(byte[] data)
        {
            string msg;

            msg = String.Format("PHASE {0} TAR {1} OUT {2}", BitConverter.ToUInt32(data, 0),BitConverter.ToUInt16(data,4),BitConverter.ToUInt16(data,6));

            return msg;

        }

    }
}
