using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace libCanopenSimple
{
    public class SDO 
    {
        public UInt32 expitideddata;
        UInt32 totaldata;
        libCanopen can;

        public UInt16 index;
        public byte subindex;
        public byte node;

        bool lasttoggle = false;

        DateTime timeout;
      

        ManualResetEvent finishedevent;

        public enum direction
        {
            SDO_READ = 0,
            SDO_WRITE = 1,
        }

        public direction dir;
        public bool exp = true;
        public Action<SDO> completedcallback;

        static List<SDO> activeSDO = new List<SDO>();

        public byte[] databuffer = null;

        debuglevel dbglevel;

        public enum SDO_STATE
        {
            SDO_INIT,
            SDO_SENT,
            SDO_HANDSHAKE,
            SDO_FINISHED,
            SDO_ERROR,
        }

        public SDO_STATE state;

        public SDO(libCanopen can, byte node, UInt16 index, byte subindex, direction dir, Action<SDO> completedcallback, byte[] databuffer)
        {
            this.can = can;
            this.index = index;
            this.subindex = subindex;
            this.node = node;
            this.dir = dir;
            this.completedcallback = completedcallback;
            this.databuffer = databuffer;

            finishedevent = new ManualResetEvent(false);

            state = SDO_STATE.SDO_INIT;

           // timeout = DateTime.Now + new TimeSpan(0, 0, 5);
            dbglevel = can.dbglevel;

        }

        public void sendSDO()
        {
            lock (activeSDO)
                activeSDO.Add(this);
        }

        public bool WaitOne()
        {
            return finishedevent.WaitOne();
        }

  
        //async regular poking of the SDO system
        public static void kick_SDO()
        {
            List<SDO> tokill = new List<SDO>();

            lock (activeSDO)
            {
                foreach (SDO s in activeSDO)
                {
                    s.kick_SDOp();
                    if (s.state == SDO_STATE.SDO_FINISHED || s.state == SDO_STATE.SDO_ERROR)
                    {
                        tokill.Add(s);
                    }
                }
            }

            foreach (SDO s in tokill)
            {
                lock (activeSDO)
                    activeSDO.Remove(s);
                s.SDOFinish();
            }
        }

        public void kick_SDOp()
        {

            if (state != SDO_STATE.SDO_INIT && DateTime.Now > timeout)
            {
                state = SDO_STATE.SDO_ERROR;

                Console.WriteLine("SDO Timeout Error on {0:x4}/{1:x2} {2:x8}", this.index, this.subindex, expitideddata);

                if (completedcallback != null)
                    completedcallback(this);

                return;
            }

            if (state == SDO_STATE.SDO_INIT)
            {
                timeout = DateTime.Now + new TimeSpan(0, 0, 5);
                state = SDO_STATE.SDO_SENT;

                if (dir == direction.SDO_READ)
                {
                    byte cmd = 0x40;
                    byte[] payload = new byte[4];
                    sendpacket(cmd, payload);
                }

                if (dir == direction.SDO_WRITE)
                {
                    byte cmd = 0;

                    switch (databuffer.Length)
                    {
                        case 1:
                            cmd = 0x2f;
                            break;
                        case 2:
                            cmd = 0x2b;
                            break;
                        case 3:
                            cmd = 0x27;
                            break;
                        case 4:
                            cmd = 0x23;
                            break;
                        default:
                            //Bigger than 4 bytes we use segmented transfer
                            cmd = 0x21;
                            byte[] payload = new byte[4];
                            payload[0] = (byte) databuffer.Length;
                            payload[1] = (byte) (databuffer.Length >> 8);
                            payload[2] = (byte) (databuffer.Length >> 16);
                            payload[3] = (byte) (databuffer.Length >> 24);

                            expitideddata = (UInt32)databuffer.Length;
                            totaldata = 0;

                            sendpacket(cmd, payload);
                            break;

                    }

                  
                    sendpacket(cmd, databuffer);

                }
            }
        }

        public void sendpacket(byte cmd, byte[] payload)
        {

            canpacket p = new canpacket();
            p.cob = (UInt16)(0x600 + node);
            p.len = 8;
            p.data = new byte[8];
            p.data[0] = cmd;
            p.data[1] = (byte)index;
            p.data[2] = (byte)(index >> 8);
            p.data[3] = subindex;

            int sendlength = 4;

            if (payload.Length < 4)
                sendlength = payload.Length;

            for (int x = 0; x < sendlength;x++)
            {
                p.data[4+x] = payload[x];
            }

            if (dbglevel == debuglevel.DEBUG_ALL)
                Console.WriteLine(String.Format("Sending a new SDO packet: {0}", p.ToString()));

            can.SendPacket(p);
        }

        public void SDOFinish()
        {
            can.SDOcallbacks.Remove((UInt16)(this.node + 0x580));
            finishedevent.Set();
        }

        public bool SDOProcess(byte[] data)
        {
            int SCS = data[0] >> 5; //7-5

            int n = (0x03 & (data[0] >> 2)); //3-2 data size for normal packets
            int e = (0x01 & (data[0] >> 1)); // expidited flag
            int s = (data[0] & 0x01); // data size set flag

            int sn = (0x07 & (data[0] >> 1)); //3-1 data size for segment packets
            int t = (0x01 & (data[0] >> 4));  //toggle flag

            //ERROR abort
            if (SCS == 0x04)
            {
                exp = true;

                expitideddata = (UInt32)(data[4] + (data[5] << 8) + (data[6] << 16) + (data[7] << 24));
                databuffer = BitConverter.GetBytes(expitideddata);

                state = SDO_STATE.SDO_ERROR;

                Console.WriteLine("SDO Error on {0:x4}/{1:x2} {2:x8}", this.index, this.subindex, expitideddata);
                
                if (completedcallback != null)
                    completedcallback(this);

                return true;
            }

            //Write complete
            if (SCS == 0x03)
            {
                state = SDO_STATE.SDO_FINISHED;

                if (completedcallback != null)
                    completedcallback(this);

                return true;
            }

            //Write segment complete
            if (SCS == 0x01)
            {
                if (totaldata < expitideddata)
                {
                    lasttoggle = !lasttoggle;
                    requestNextSegment(lasttoggle);

                    totaldata += 4;
                }
                else
                {
                    state = SDO_STATE.SDO_FINISHED;
                    if (completedcallback != null)
                        completedcallback(this);
                }

            }

            //if expedited just handle the data
            if (SCS == 0x02 && e == 1)
            {
                //Expidited and length are set so its a regular short transfer
                exp = true;

                expitideddata = (UInt32)(data[4] + (data[5] << 8) + (data[6] << 16) + (data[7] << 24));
                databuffer = BitConverter.GetBytes(expitideddata);
              
                state = SDO_STATE.SDO_FINISHED;

                if (completedcallback != null)
                    completedcallback(this);

                return true;
            }


            exp = false;

            if (SCS == 0x02)
            {
                UInt32 count = (UInt32)(data[4] + (data[5] << 8) + (data[6] << 16) + (data[7] << 24));
                expitideddata = count;
                databuffer = new byte[expitideddata];
                totaldata = 0;
                //Request next segment

                requestNextSegment(false); //toggle off on first request
                return false;

            }

            //segmented transfer

            UInt32 scount = (UInt32)(7 - sn);

            //Segments toggle on
            if (SCS == 0x00)
            {

                for (int x = 0; x < scount; x++)
                {
                    databuffer[totaldata + x] = data[1 + x];
                }

                totaldata += scount;

                if (totaldata < expitideddata)
                {
                    lasttoggle = !lasttoggle;
                    requestNextSegment(lasttoggle);
                }
                else
                {
                    state = SDO_STATE.SDO_FINISHED;
                    if (completedcallback != null)
                        completedcallback(this);
                }

            }

            return false;
        }

        public void requestNextSegment(bool toggle)
        {

            if (dir == direction.SDO_READ)
            {
                byte cmd = 0x61;
                if (toggle)
                    cmd |= 0x10;

                sendpacket(cmd, new byte[4]);
            }
            else
            {
                byte cmd = 0x00;
                if (toggle)
                    cmd |= 0x10;

                byte[] nextdata = new byte[4];

                nextdata[0] = databuffer[totaldata];
                if (databuffer.Length < totaldata + 1)
                    nextdata[1] = databuffer[totaldata + 1];
                if (databuffer.Length < totaldata + 2)
                    nextdata[2] = databuffer[totaldata + 2];
                if (databuffer.Length < totaldata + 3)
                    nextdata[3] = databuffer[totaldata + 3];

                sendpacket(cmd, nextdata);

            }

        }

    }
}
