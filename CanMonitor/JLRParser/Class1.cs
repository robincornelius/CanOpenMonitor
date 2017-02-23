using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;
using System.Windows.Forms;
using JLRParser;

namespace PDOParser
{

    public class PDO : IPDOParser
    {
        public JLRStatus status;

        public void registerPDOS(Dictionary<UInt16, Func<byte[], string>> dic)
        {
            dic.Add(0x181, PDO181);
            dic.Add(0x183, PDO183);
            //dic.Add(0x203, PDO203);
            dic.Add(0x190, PDO190);
            dic.Add(0x203, PDO203);
            dic.Add(0x303, PDO303);
            dic.Add(0x403, PDO403);
            dic.Add(0x205, PDO205);
            dic.Add(0x206, PDO206);
            dic.Add(0x185, PDO185);
            dic.Add(0x200, PDO200);
            dic.Add(0x201, PDO201);
            dic.Add(0x214, PDO214);
            dic.Add(0x215, PDO215);


            status = new JLRStatus();
            status.Show();

            for(byte x=0;x<16;x++)
            {
                status.updateGM(x, 0);
            }

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

                        int chan = 3*(node - 8);
                        chan = chan + (sub - 1);

                        status.updateGM((byte)chan,val);
                        return ret;
                    }

                }


                if (node == 0x07)
                {
                    float val = BitConverter.ToSingle(payload2, 0);
                    ret = string.Format("TEMP CHAN {0} = {1} T", sub, val);

                    status.updatetemp((byte)(sub-1), val);
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
    CTRL_BOOT = 0,
    CTRL_WAITPLC,
    CTRL_BUSALARM,
    CTRL_CHGRALARM,
    CTRL_MOTORALARM,
    CTRL_SAFEALARM,

    CTRL_OFF,

    CTRL_WAIT_ON,
    CTRL_INTERLOCK,
    CTRL_RUN,
    CTRL_WAITOUT,
    CTRL_WAITIN,
    CTRL_WAITUP,
    CTRL_WAITDOWN,
    CTRL_WAITSTART,

    CTRL_UPLOADUSER,

    CTRL_WAITUPLOADUSER,

    CTRL_WAITBARCODE,
    CTRL_GETBARCODE,
    CTRL_GETBARCODEREPLY,
    CTRL_WAITUPLOADBARCODE,

    CTRL_SETOVERCHECK,
    CTRL_WAITOVERCHECKREPLY,



    CTRL_MAGNETISE, //5
    CTRL_LIVE, //6
    CTRL_STARTCHARGE,
    CTRL_CHARGE, //7       
    CTRL_THYMUX,
    CTRL_DISCHARGE,
    CTRL_INDEXNEXT,
    CTRL_WAITCOMPLETE, //8
    CTRL_READGAUSSMETERS,

    CTRL_UPLOADGAUSS,
    CTRL_WAITUPLOADGAUSS,

    CTRL_READTHERMOCOUPLE,
    CTRL_WAITUPLOADTHERMOCOUPLE,


    FLUXMETER_SAMPLE,
    FLUXMETER_RESET,
    FLUXMETER_GETVAL,
    FLUXMETER_RANGE,
    FLUXMETER_ISOLATE,
    FLUXMETER_ZERO,

    WAIT_FLUXWRITE,
    WAIT_NEWWRITE,

    CHECK_DATAREAD,
    PUK_FAIL_OVERCHECK,
    CTRL_OVERTEMP,
    CTRL_HOME_REQUIRED,
    CTRL_GOING_HOME,
                             
 } ;

        static Estates lastcharger;
        static EController_states lastcontroller;


        UInt16 vcaps;

        public string PDO181(byte[] data)
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

            status.updatevoltage(vcaps,chargerstate.ToString(), controller_state.ToString());

            this.vcaps = (UInt16)vcaps;

            return msg;

        }

        public static string PDO183(byte[] data)
        {
            string msg = "";

            msg = String.Format("IN {0} {1}", Convert.ToString(data[0], 2), Convert.ToString(data[1], 2));

            return msg;
        }

        public static string PDO190(byte[] data)
        {

            return String.Format("Target {0} Phase {1}", BitConverter.ToInt16(data, 0), BitConverter.ToInt32(data, 2));

        }

        public static string PDO203(byte[] data)
        {
            return "saftey unlock";
        }
        public static string PDO303(byte[] data)
        {

            return string.Format("Fire inthe hole FX {0}", data[0]); ;
        }
        public string PDO403(byte[] data)
        {

            status.updatemaxvoltage(BitConverter.ToUInt16(data, 1),data[0]);
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
                msg += "POWERON";
            if ((data[0] & (byte)0x80)!=0)
                msg += "";

            msg += ")";

             
      

            return msg;
        }

        public static string PDO200(byte[] data)
        {

            return "WTF?";
            string msg;

            msg = String.Format("PHASE {0} TAR {1} OUT {2}", BitConverter.ToUInt32(data, 0),BitConverter.ToUInt16(data,4),BitConverter.ToUInt16(data,6));

            return msg;

        }

        public string PDO201(byte[] data)
        {
            string msg;

            float val = (float)BitConverter.ToSingle(data, 0);
            msg = String.Format("FLUX {0}", val);

            status.updateflux(val);
            

            Console.WriteLine(msg);

            return msg;

        }


        DateTime chargestart = new DateTime();
        DateTime chargeend = new DateTime();

        string PDO214(byte[] data)
        {

            string msg;

            UInt16 vol = (UInt16)BitConverter.ToUInt16(data, 0);
            byte flag = (byte)data[2];
            msg = String.Format("Voltage {0} chargeflag {1}", vol,flag);

            if(flag==0xff)
            {
                chargestart = DateTime.Now;
            }
            

            return msg;

        }

        string PDO215(byte[] data)
        {
            string msg;
           
            byte flag = (byte)data[0];
            msg = String.Format("end chargeflag {0}", flag);

            if (flag == 0xaa)
            {
                chargeend = DateTime.Now;

                double E = 0.5 * 80000e-6 * (double)vcaps * (double)vcaps;
                TimeSpan span = chargeend-chargestart;

                double power = 2.0 * ((double)E / (double)span.TotalSeconds);

                msg += string.Format(" Time {0} PWR {1}",span.TotalSeconds, power);
            }

            return msg;
        }

    }
}
