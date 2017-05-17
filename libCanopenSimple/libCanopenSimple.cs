using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Collections.Concurrent;
//using NNanomsg;
//using NNanomsg.Protocols;


namespace libCanopenSimple
{




    public enum BUSSPEED
    {

        BUS_10Kbit = 0,
        BUS_20Kbit,
        BUS_50Kbit,
        BUS_100Kbit,
        BUS_125Kbit,
        BUS_250Kbit,
        BUS_500Kbit,
        BUS_800Kbit,
        BUS_1Mbit,

    }

    public enum debuglevel
    {
        DEBUG_ALL,
        DEBUG_NONE
    }

    public class canpacket
    {
        public UInt16 cob;
        public byte len;
        public byte[] data;
     
        

        public string ToString()
        {
            string output;

            output = string.Format("{0:x3} {1:x1}", cob, len);

            for(int x=0;x<len;x++)
            {
                output += string.Format(" {0:x2}", data[x]);
            }


            return output;
        }

    }

    public class NMTState
    {
        public enum e_NMTState
        {
            BOOT = 0,
            STOPPED = 4,
            OPERATIONAL = 5,
            PRE_OPERATIONAL = 127,
            INVALID = 0xff,

        }

        public e_NMTState state;
        public e_NMTState laststate;
        public DateTime lastping;
        public bool compulsory;

       

        public Action<e_NMTState> NMT_boot = null;
        public Action<int> NMT_guard = null;

        public NMTState()
        {
            state = e_NMTState.INVALID;
            laststate = e_NMTState.INVALID;
        }

        public void changestate(e_NMTState newstate)
        {
            laststate = state;
            state = newstate;
            lastping = DateTime.Now;

            

            if(newstate == e_NMTState.BOOT)
                if (state != laststate && NMT_boot != null)
                {
                    NMT_boot(state);
                }
        }
    }

    public class libCanopen
    {
        SerialPort serialPort;
        //BusSocket s;
        //NanomsgListener l;

        string buf;
        public debuglevel dbglevel = debuglevel.DEBUG_NONE;
        public bool echo = true;

        public bool ipcisopen = false;

        public bool isopen()
        {

            if (ipcisopen)
                return true;

            if (serialPort == null)
                return false;

           return serialPort.IsOpen;

        }

        public libCanopen()
        {
            //preallocate all NMT guards
            for(byte x=0;x<0x80;x++)
            {
                NMTState nmt = new NMTState();
                nmtstate[x] = nmt;
            }
  
        }

        Dictionary<UInt16, NMTState> nmtstate = new Dictionary<ushort, NMTState>();

        public void open(int comport, BUSSPEED speed)
        {
            serialPort = new SerialPort();

            serialPort.PortName = string.Format("COM{0}", comport);
            serialPort.BaudRate = 115200;

            serialPort.DataReceived += SerialPort_DataReceived;

            serialPort.Open();

            serialPort.Write(String.Format("C\rS{0}\rO\r", (int)speed));

            threadrun = true;
            Thread thread = new Thread(new ThreadStart(asyncprocess));
            thread.Name = "CAN Open worker";
            thread.Start();

        }

        public void open(string ipc)
        {
            //serialPort = new SerialPort();
            //serialPort.PortName = string.Format("COM{0}", comport);
            //serialPort.BaudRate = 115200;
            //serialPort.DataReceived += SerialPort_DataReceived;
            //serialPort.Open();
            //serialPort.Write(String.Format("C\rS{0}\rO\r", (int)speed));

            //s = new BusSocket();
            //l = new NanomsgListener();

            //s.Connect(ipc);
            //l.AddSocket(s);
            //l.ReceivedMessage += L_ReceivedMessage;

            System.Threading.Thread t = new System.Threading.Thread(ipclistentask);
            t.Start();

            ipcisopen = true;

            threadrun = true;
            Thread thread = new Thread(new ThreadStart(asyncprocess));
            thread.Name = "CAN Open worker";
            thread.Start();


        }

        void ipclistentask()
        {
            //while (true)
            {
                //l.Listen(new TimeSpan(0));
                //System.Threading.Thread.Sleep(1);
            }
        }

        /*
        private void L_ReceivedMessage(int socketID)
        {
            byte[] payload = s.ReceiveImmediate();
            if (payload == null)
                return;

            canpacket cp = new canpacket();
            cp.cob = (ushort)(payload[0] | (payload[1] << 8)); //actually 332 bit

            int length = cp.cob & 0xF000;

            length = (length >> 12);

            cp.len = (byte)length;

            cp.cob =(ushort)( cp.cob & 0x0fff);
            cp.data = new byte[length];

            

            for(int x=0;x<length;x++)
            {
                cp.data[x] = payload[x + 5];
            }

            packetqueue.Enqueue(cp);

            if (dbglevel == debuglevel.DEBUG_ALL)
                Console.WriteLine(String.Format("RX packet packet: {0}", cp.ToString()));

           
        }
        */


        public void close()
        {
            threadrun = false;
            if(serialPort!=null)
                serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            string receiveBuffer = serialPort.ReadExisting();

            buf += receiveBuffer;

            int idx = 0;

            int start = 0;

            while (idx != -1)
            {
                idx = buf.IndexOf('\r', start);
                if (idx == -1)
                    break;
                string meh = buf.Substring(start, idx - start);
                processCanRX(meh);
                start = idx + 1;
            }

            buf = buf.Substring(start);
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        Dictionary<UInt16, Action<byte[]>> PDOcallbacks = new Dictionary<ushort, Action<byte[]>>();
        public Dictionary<UInt16, SDO> SDOcallbacks = new Dictionary<ushort, SDO>();
        ConcurrentQueue<canpacket> packetqueue = new ConcurrentQueue<canpacket>();

        public Action<canpacket> loggercallback_SDO = null;
        public Action<canpacket> loggercallback_NMTEC = null;
        public Action<canpacket> loggercallback_NMT = null;
        public Action<canpacket[]> loggercallback_PDO = null;
        public Action<canpacket> loggercallback_EMCY = null;
        public Action<canpacket> loggercallback_LSS = null;
        public Action<canpacket> loggercallback_TIME = null;
        public Action<canpacket> loggercallback_SYNC = null;

        public delegate void NMTEvent(canpacket p);
        public event NMTEvent nmtevent;

        public delegate void NMTECEvent(canpacket p);
        public event NMTECEvent nmtecevent;

        bool threadrun = true;

        public void registerPDOhandler(UInt16 cob, Action<byte[]> handler)
        {
            PDOcallbacks[cob] = handler;
        }

        private void processCanRX(string packet,bool loopback=false)
        {
            //packet must be at least "700 0" as a min

            if (packet.Length < 4)
                return;

            if (packet[0] == 't')
                packet = packet.TrimStart('t');
            else
                return;

            byte[] bytes = StringToByteArrayFastest(packet);
            byte[] cobbytes = new byte[2];
            cobbytes[1] = bytes[0];
            cobbytes[0] = (byte)((0xF0 & bytes[1]));

            UInt16 cob = (UInt16)(BitConverter.ToUInt16(cobbytes, 0)>>4); //0,1
            byte len = (byte)(0x0F&bytes[1]);


            canpacket cp = new canpacket();
            cp.cob = cob;
            cp.len = len;
            cp.data = new byte[len];
            for (int x = 0; x < len; x++)
                cp.data[x] = bytes[2 + x];

            packetqueue.Enqueue(cp);

            if(dbglevel==debuglevel.DEBUG_ALL)
                Console.WriteLine(String.Format("RX packet packet: {0}", cp.ToString()));

        }

        

        void asyncprocess()
        {
            while (threadrun)
            {
                canpacket cp;
                List<canpacket> pdos = new List<canpacket>();
                while (packetqueue.TryDequeue(out cp))
                {
                    //Handle PDO first

                    //PDO 0x180 -- 0x57F
                    if (cp.cob >= 0x180 && cp.cob <= 0x57F)
                    {

                        if (PDOcallbacks.ContainsKey(cp.cob))
                            PDOcallbacks[cp.cob](cp.data);

                        pdos.Add(cp);

                        //if (loggercallback_PDO != null)
                        //    loggercallback_PDO(cp);
                    }

                    //SDO replies 0x601-0x67F
                    if (cp.cob >= 0x580 && cp.cob < 0x600)
                    {
                        if (cp.len != 8)
                            return;

                        if (SDOcallbacks.ContainsKey(cp.cob))
                        {
                            if (SDOcallbacks[cp.cob].SDOProcess(cp))
                            {
                                SDOcallbacks.Remove(cp.cob);
                            }
                        }

                        if (loggercallback_SDO != null)
                            loggercallback_SDO(cp);
                    }

                    if (cp.cob >= 0x600 && cp.cob < 0x680)
                    {
                        if (loggercallback_SDO != null)
                            loggercallback_SDO(cp);
                    }

                    //NMT
                    if (cp.cob > 0x700 && cp.cob <= 0x77f)
                    {
                        byte node = (byte)(cp.cob & 0x07F);

                        //Console.WriteLine(string.Format("NMT node {0:x} state {1}",cp.cob,cp.data[0]));

                        nmtstate[node].changestate((NMTState.e_NMTState)cp.data[0]);
                        nmtstate[node].lastping = DateTime.Now;

                        if (loggercallback_NMTEC != null)
                            loggercallback_NMTEC(cp);

                        if (nmtecevent != null)
                            nmtecevent(cp);
                    }

                    if(cp.cob==000)
                    {
                        if (loggercallback_NMT != null)
                            loggercallback_NMT(cp);

                        if (nmtevent != null)
                            nmtevent(cp);

                    }
                    if (cp.cob == 0x80)
                    {
                        if (loggercallback_SYNC != null)
                            loggercallback_SYNC(cp);
                    }

                    if (cp.cob > 0x080 && cp.cob <= 0xFF)
                    {
                        if (loggercallback_EMCY != null)
                            loggercallback_EMCY(cp);

                    }

                    if (cp.cob == 0x100)
                    {
                        if (loggercallback_TIME != null)
                            loggercallback_TIME(cp);
                    }

                    if (cp.cob > 0x7E4 && cp.cob <= 0x7E5)
                    {
                        if (loggercallback_LSS != null)
                            loggercallback_LSS(cp);

                    }

                }

                if(pdos.Count>0)
                {
                    if (loggercallback_PDO != null)
                        loggercallback_PDO(pdos.ToArray());
                }

                    SDO.kick_SDO();

                    if (sdo_queue.Count > 0)
                    {
                        SDO front = sdo_queue.Peek();
                        if (front != null)
                        {
                            if (!SDOcallbacks.ContainsKey((UInt16)(front.node + 0x580)))
                            {
                                front = sdo_queue.Dequeue();
                                //Listen for the reply on 0x580+node id
                                SDOcallbacks.Add((UInt16)(front.node + 0x580), front);
                                front.sendSDO();
                            }
                        }
                    }
                     

                   // System.Threading.Thread.Sleep(1);
            }

        }

        public bool checkguard(int node, TimeSpan maxspan)
        {

            if (DateTime.Now - nmtstate[(ushort)node].lastping > maxspan)
                return false;

            return true;

        }

        private Queue<SDO> sdo_queue = new Queue<SDO>();

        public SDO SDOwrite(byte node, UInt16 index, byte subindex, UInt32 udata, Action<SDO> completedcallback)
        {
            byte[] bytes = BitConverter.GetBytes(udata);
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }

        public SDO SDOwrite(byte node, UInt16 index, byte subindex, Int32 udata, Action<SDO> completedcallback)
        {
            byte[] bytes = BitConverter.GetBytes(udata);
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }

        public SDO SDOwrite(byte node, UInt16 index, byte subindex, Int16 udata, Action<SDO> completedcallback)
        {
            byte[] bytes = BitConverter.GetBytes(udata);
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }

        public SDO SDOwrite(byte node, UInt16 index, byte subindex, UInt16 udata, Action<SDO> completedcallback)
        {
            byte[] bytes = BitConverter.GetBytes(udata);
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }

        public SDO SDOwrite(byte node, UInt16 index, byte subindex, float ddata, Action<SDO> completedcallback)
        {
            byte[] bytes = BitConverter.GetBytes(ddata);
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }


        public SDO SDOwrite(byte node, UInt16 index, byte subindex, byte udata, Action<SDO> completedcallback)
        {
            byte[] bytes = new byte[1];
            bytes[0] = udata;
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }

        public SDO SDOwrite(byte node, UInt16 index, byte subindex, sbyte udata, Action<SDO> completedcallback)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte)udata;
            return SDOwrite(node, index, subindex, bytes, completedcallback);
        }


        public SDO SDOwrite(byte node, UInt16 index, byte subindex, byte[] data, Action<SDO> completedcallback)
        {

            SDO sdo = new SDO(this, node, index, subindex, SDO.direction.SDO_WRITE, completedcallback, data);
            sdo_queue.Enqueue(sdo);
            return sdo;
        }

     
        public SDO SDOread(byte node, UInt16 index, byte subindex,Action<SDO> completedcallback)
        {
            SDO sdo = new SDO(this, node, index, subindex, SDO.direction.SDO_READ, completedcallback,null);
            sdo_queue.Enqueue(sdo);
            return sdo;
        }


        public void SendPacket(canpacket p)
        {

            string canframe = string.Format("t{0:x3}{1:x1}", p.cob, p.len);

            for (int x = 0; x < p.len; x++)
            {
                canframe += string.Format("{0:x2}", p.data[x]);
            }

            canframe += "\r";

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write(canframe);
            }

           /* if(ipcisopen)
            {
                byte[] payload = new byte[15];

                payload[0] = (byte)p.cob;
                payload[1] = (byte)(p.cob >> 8);
               // payload[1] |= (byte) (p.len << 4);

                payload[4] = p.len;

                for(int x=0;x<p.len;x++)
                {
                    payload[5 + x] = p.data[x];
                }

                s.Send(payload);
            }*/

            if(echo==true)
            {
                processCanRX(canframe.Substring(0,canframe.Length-1));
            }
        }

        public void NMT_start(byte nodeid = 0)
        {
            canpacket p = new canpacket();
            p.cob = 000;
            p.len = 2;
            p.data = new byte[2];
            p.data[0] = 0x01;
            p.data[1] = nodeid;
            SendPacket(p);
        }

        public void NMT_preop(byte nodeid = 0)
        {
            canpacket p = new canpacket();
            p.cob = 000;
            p.len = 2;
            p.data = new byte[2];
            p.data[0] = 0x80;
            p.data[1] = nodeid;
            SendPacket(p);
        }

        public void NMT_stop(byte nodeid = 0)
        {
            canpacket p = new canpacket();
            p.cob = 000;
            p.len = 2;
            p.data = new byte[2];
            p.data[0] = 0x80;
            p.data[1] = nodeid;
            SendPacket(p);
        }

        public void NMT_ResetNode(byte nodeid = 0)
        {
            canpacket p = new canpacket();
            p.cob = 000;
            p.len = 2;
            p.data = new byte[2];
            p.data[0] = 0x81;
            p.data[1] = nodeid;

            SendPacket(p);
        }

        public void NMT_ResetComms(byte nodeid = 0)
        {
            canpacket p = new canpacket();
            p.cob = 000;
            p.len = 2;
            p.data = new byte[2];
            p.data[0] = 0x82;
            p.data[1] = nodeid;

            SendPacket(p);
        }

        public void NMT_SetStateTransitionCallback(byte node,Action<NMTState.e_NMTState> callback)
        {
            nmtstate[node].NMT_boot = callback;
        }

        public bool NMT_isNodeFound(byte node)
        {
            return nmtstate[node].state != NMTState.e_NMTState.INVALID;
        }

        public void NMT_ReseCommunication(byte nodeid = 0)
        {
            canpacket p = new canpacket();
            p.cob = 000;
            p.len = 2;
            p.data = new byte[2];
            p.data[0] = 0x81;
            p.data[1] = nodeid;

            SendPacket(p);
        }

        public void writePDO(UInt16 cob,byte[] payload)
        {
            canpacket p = new canpacket();
            p.cob = cob;
            p.len = (byte) payload.Length;
            p.data = new byte[p.len];
            for (int x = 0; x < payload.Length; x++)
                p.data[x] = payload[x];


  
            SendPacket(p);
        }

    }
}
