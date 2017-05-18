using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libCanopenSimple;
using System.Reflection;
using PDOInterface;
using N_SettingsMgr;
using System.IO;

namespace CanMonitor
{
    public partial class Form1 : Form
    {

        private libCanopen lco = new libCanopen();
        private Dictionary<UInt16, Func<byte[], string>> pdoprocessors = new Dictionary<ushort, Func<byte[], string>>();
        private string appdatafolder;
        IPDOParser ipdo;

        List<ListViewItem> listitems = new List<ListViewItem>();

        List<ListViewItem> EMClistitems = new List<ListViewItem>();

        private Dictionary<UInt32, string> sdoerrormessages = new Dictionary<UInt32, string>();

        Dictionary<UInt32, List<byte>> sdotransferdata = new Dictionary<uint, List<byte>>();

        System.Windows.Forms.Timer updatetimer = new System.Windows.Forms.Timer();

        StreamWriter sw;

        public struct SNMTState
        {
            public byte state;
            public DateTime lastupdate;
            public bool dirty;
            public ListViewItem LVI;
            public bool isnew;
            public string statemsg;
        }

        Dictionary<UInt16, SNMTState> NMTstate = new Dictionary<ushort, SNMTState>();
        List<SNMTState> dirtyNMTstates = new List<SNMTState>();

        bool NMTstateupdate = false;

        

        public Form1()
        {

            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appdatafolder = Path.Combine(folder, "CanMonitor");
                SettingsMgr.readXML(Path.Combine(appdatafolder, "settings.xml"));
            }
            catch (Exception e)
            {
            }

            InitializeComponent();


            comboBox_port.Items.Add("ipc://can_id0");

            foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBox_port.Items.Add(portName);
            }



            lco.dbglevel = debuglevel.DEBUG_NONE;

            lco.nmtecevent += log_NMT;
            lco.nmtevent += log_NMTEC;

            lco.sdoevent += log_SDO;
            lco.pdoevent += log_PDO;
            lco.emcyevent += log_EMCY;

            listView1.DoubleBuffering(true);
            listView_emcy.DoubleBuffering(true);
            listView_nmt.DoubleBuffering(true);

            interror();

            listView1.ListViewItemSorter = null;

            updatetimer.Interval = 1000;
            updatetimer.Tick+=updatetimer_Tick;
            updatetimer.Enabled =true;

        }

     
        void appendfile(string[] ss)
        {

            if (sw == null)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (string s in ss)
            {
                sb.AppendFormat("{0}\t", s);
            }

            sw.WriteLine(sb.ToString());

        }

void updatetimer_Tick(object sender, EventArgs e)
{

    if (listitems.Count != 0)
        lock(listitems)
        {
            listView1.BeginUpdate();

            listView1.Items.AddRange(listitems.ToArray());

            listitems.Clear();

            if (checkbox_autoscroll.Checked && listView1.Items.Count>2)
                listView1.EnsureVisible(listView1.Items.Count - 1);

           


            listView1.EndUpdate();

        }

    lock (NMTstate)
    {
        if (dirtyNMTstates.Count>0)
        {
            listView_nmt.BeginUpdate();

            foreach(SNMTState state in dirtyNMTstates)
            {
                if(state.isnew)
                {
                    listView_nmt.Items.Add(state.LVI);
                }
                else
                {
                    state.LVI.SubItems[0].Text = state.lastupdate.ToString();
                    state.LVI.SubItems[2].Text = state.statemsg;
                }

            }

            dirtyNMTstates.Clear();
          
            listView_nmt.EndUpdate();
        }
    }

    if (EMClistitems.Count > 0)
    {
        lock (EMClistitems)
        {
            listView_emcy.BeginUpdate();
            listView_emcy.Items.AddRange(EMClistitems.ToArray());
            EMClistitems.Clear();
            listView_emcy.EndUpdate();
        }
    }

}

        private void log_NMT(canpacket payload)
        {
            //listView1.BeginInvoke(new MethodInvoker(delegate
            //{
                string[] items = new string[6];
                items[0] = DateTime.Now.ToString();
                items[1] = "NMT";
                items[2] = string.Format("{0:x3}", payload.cob);
                items[3] = "";
                items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                string msg = "";

                if (payload.data.Length != 2)
                    return;

                switch (payload.data[0])
                {
                    case 0x01:
                        msg = "Enter operational";
                        break;
                    case 0x02:
                        msg = "Enter stop";
                        break;
                    case 0x80:
                        msg = "Enter pre-operational";
                        break;
                    case 0x81:
                        msg = "Reset node";
                        break;
                    case 0x82:
                        msg = "Reset communications";
                        break;

                }

                if(payload.data[1]==0)
                {
                    msg += " - All nodes";
                }
                else
                {
                    msg += string.Format(" - Node 0x{0:x2}", payload.data[1]);
                }

                items[5] = msg;

                ListViewItem i = new ListViewItem(items);

                i.ForeColor = Color.Red;

                //listView1.BeginUpdate();
                //listView1.Items.Add(i);
                //listView1.EndUpdate();
              
                lock(listitems)
                    listitems.Add(i);

                appendfile(items);


            //}));

           

        }

        private void log_NMTEC(canpacket payload)
        {
            //listView1.BeginInvoke(new MethodInvoker(delegate
           // {
                string[] items = new string[6];
                items[0] = DateTime.Now.ToString();
                items[1] = "NMTEC";
                items[2] = string.Format("{0:x3}", payload.cob);
                items[3] = string.Format("{0:x3}", payload.cob & 0x0FF);
                items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                string msg = "";
                switch (payload.data[0])
                {
                    case 0:
                        msg = "BOOT";
                        break;
                    case 4:
                        msg = "STOPPED";
                        break;
                    case 5:
                        msg = "Heart Beat";
                    break;
                    case 0x7f:
                        msg = "Heart Beat (Pre op)";
                    break;

                }

                items[5] = msg;

                ListViewItem i = new ListViewItem(items);

                i.ForeColor = Color.DarkGreen;

                appendfile(items);

                if (checkBox_heartbeats.Checked == true || payload.data[0] == 0)
                {
                    lock (listitems)
                    {
                        listitems.Add(i);
                    }
                }

                lock(NMTstate)
                {
                    byte node = (byte)(payload.cob & 0x0FF);

                    if(NMTstate.ContainsKey(node))
                    {
                        SNMTState s = NMTstate[node];
                        s.lastupdate = DateTime.Now;
                        s.dirty = true;
                        s.state = payload.data[0];
                        s.isnew = false;
                        s.statemsg = msg;
                        NMTstate[node] = s;
                        dirtyNMTstates.Add(NMTstate[node]);
                    }
                    else
                    {
                        SNMTState s = new SNMTState();
                        s.lastupdate = DateTime.Now;
                        s.dirty = true;
                        s.state = payload.data[0];
                        s.statemsg = msg;
                        string[] ss = new string[3];
                        ss[0] = DateTime.Now.ToString();
                        ss[1] = node.ToString();
                        ss[2] = msg;

                        ListViewItem newitem = new ListViewItem(ss);
                        s.LVI = newitem;
                        s.isnew = true;

                        NMTstate.Add(node,s);
                        dirtyNMTstates.Add(NMTstate[node]);
                    }
                                
                }



        }

        private void log_SDO(canpacket payload)
        {
//            listView1.BeginInvoke(new MethodInvoker(delegate
//            {
                string[] items = new string[6];
                items[0] = DateTime.Now.ToString();
                items[1] = "SDO";
                items[2] = string.Format("{0:x3}", payload.cob);
                items[3] = string.Format("{0:x3}", payload.cob & 0x0FF);
                items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                string msg="";


                int SCS = payload.data[0] >> 5; //7-5

                int n = (0x03 & (payload.data[0] >> 2)); //3-2 data size for normal packets
                int e = (0x01 & (payload.data[0] >> 1)); // expidited flag
                int s = (payload.data[0] & 0x01); // data size set flag //also in block
                int c = s;
       
                int sn = (0x07 & (payload.data[0] >> 1)); //3-1 data size for segment packets
                int t = (0x01 & (payload.data[0] >> 4));  //toggle flag

                int cc = (0x01 & (payload.data[0] >> 2));



                UInt16 index =(UInt16)(payload.data[1] + (payload.data[2] << 8));
                byte sub = payload.data[3];


                int valid = 7;
                int validsn = 7;


                if (n != 0)
                    valid = 8 - (7 - n);

                if (sn != 0)
                    validsn = 8 - (7 - sn);


            if (payload.cob>=0x580 && payload.cob<=0x600)
                {
                    string mode = "";
                    string sdoproto = "";

                    string setsize = "";

                    switch (SCS)
                    {
                        case 0:
                            mode = "upload segment response";
                            sdoproto = string.Format("{0} {1} Valid bytes = {2} {3}", mode, t == 1 ? "TOG ON" : "TOG OFF",validsn,c==0?"MORE":"END");
                          
                            if(sdotransferdata.ContainsKey(payload.cob))
                            {

                                for(int x=1;x<=validsn;x++)
                                {
                                    sdotransferdata[payload.cob].Add(payload.data[x]);
                                }

                                if (c == 1)
                                {

                                    StringBuilder hex = new StringBuilder(sdotransferdata[payload.cob].Count * 2);
                                    StringBuilder ascii = new StringBuilder(sdotransferdata[payload.cob].Count * 2);
                                    foreach (byte b in sdotransferdata[payload.cob])
                                    {
                                        hex.AppendFormat("{0:x2} ", b);
                                        ascii.AppendFormat("{0}", (char)Convert.ToChar(b));
                                    }

                                    sdoproto += "\nDATA = "+hex.ToString() +"("+ascii+")";

                                    //Console.WriteLine(hex.ToString());
                                    //Console.WriteLine(ascii.ToString());

                                    sdotransferdata.Remove(payload.cob);
                                }

                            }

                            break;
                        case 1:
                            mode = "download segment response";
                            sdoproto = string.Format("{0} {1}", mode, t == 1 ? "TOG ON" : "TOG OFF");
                            break;
                        case 2:
                            mode = "initate upload response";
                            string nbytes = "";

                            if(e==1 && s==1)
                            {
                                //n is valid
                                nbytes = string.Format("Valid bytes = {0}", 4 - n);
                            }

                            if(e==0 && s==1)
                            {
                                byte[] size = new byte[4];
                                Array.Copy(payload.data, 4, size, 0, 4);
                                UInt32 isize = (UInt32)BitConverter.ToUInt32(size, 0);
                                nbytes = string.Format("Bytes = {0}", isize);

                                if (sdotransferdata.ContainsKey(payload.cob))
                                    sdotransferdata.Remove(payload.cob);

                                sdotransferdata.Add(payload.cob, new List<byte>());
                            }

                            sdoproto = string.Format("{0} {1} {2} 0x{3:x4}/{4:x2}", mode,nbytes,e==1?"Normal":"Expedite", index, sub);
                            break;
                        case 3:
                            mode = "initate download response";
                            sdoproto = string.Format("{0} 0x{1:x4}/{2:x2}", mode, index, sub);
                            break;

                        case 5:
                            mode = "Block download response";

                            byte segperblock = payload.data[4];
                            sdoproto = string.Format("{0} 0x{1:x4}/{2:x2} Blksize = {3}", mode, cc == 0 ? "NO SERVER CRC" : "SERVER CRC", index, sub,segperblock);

                            break;


                        default:
                            mode = string.Format("SCS {0}", SCS);
                            break;

                    }



                    msg = sdoproto;
  

                }
                else
                {
                    //Client to server

                    string mode = "";
                    string sdoproto = "";

                    switch (SCS)
                    {
                        case 0:
                            mode = "download segment request";
                            sdoproto = string.Format("{0} {1} Valid bytes = {2} {3}", mode, t == 1 ? "TOG ON" : "TOG OFF", validsn, c == 0 ? "MORE" : "END");

                            break;
                        case 1:
                            string nbytes = "";

                            if (e == 1 && s == 1)
                            {
                                //n is valid
                                nbytes = string.Format("Valid bytes = {0}", 4 - n);
                            }

                            if (e == 0 && s == 1)
                            {
                                byte[] size2 = new byte[4];
                                Array.Copy(payload.data, 4, size2, 0, 4);
                                UInt32 isize2 = (UInt32)BitConverter.ToUInt32(size2, 0);
                                nbytes = string.Format("Bytes = {0}", isize2);
                            }

                            mode = "initate download request";
                            sdoproto = string.Format("{0} {1} {2} 0x{3:x4}/{4:x2}", mode, nbytes, e == 1 ? "Normal" : "Expedite", index, sub);
                            break;
                        case 2:
                            mode = "initate upload request";
                            sdoproto = string.Format("{0} 0x{1:x4}/{2:x2}",mode, index, sub);
                            break;
                        case 3:
                            mode = "upload segment request";
                            sdoproto = string.Format("{0} {1}", mode, t == 1 ? "TOG ON" : "TOG OFF");
                            break;

                        case 5:
                            mode = "Block download";
                            sdoproto = string.Format("{0}", mode);
                            break;

                        case 6:
                            mode = "Initate Block download request";

                            byte[] size = new byte[4];
                            Array.Copy(payload.data, 4, size, 0, 4);
                            UInt32 isize = (UInt32)BitConverter.ToUInt32(size, 0);

                            sdoproto = string.Format("{0} 0x{1:x4}/{2:x2} Size = {3}", mode,cc==0?"NO CLIENT CRC":"CLIENT CRC", index, sub,isize);
                            break;


                        default:
                            mode = string.Format("CSC {0}", SCS);
                            break;

                    }


                    msg = sdoproto;

                }


                if ((payload.data[0] & 0x80) != 0)
                {
                    byte[] errorcode = new byte[4];
                    errorcode[0] = payload.data[4];
                    errorcode[1] = payload.data[5];
                    errorcode[2] = payload.data[6];
                    errorcode[3] = payload.data[7];

                    UInt32 err = BitConverter.ToUInt32(errorcode, 0);

                    if (sdoerrormessages.ContainsKey(err))
                    {

                        msg += " " + sdoerrormessages[err];
                    }

                }
                else
                {
                    if(ipdo!=null)
                        msg += " "+ipdo.decodesdo(payload.cob, index, sub, payload.data);
                }


                items[5] = msg;
                appendfile(items);

                ListViewItem i = new ListViewItem(items);
                
                if ((payload.data[0] & 0x80) != 0)
                {
                    i.BackColor = Color.Orange;                
                }

               


                i.ForeColor = Color.DarkBlue;
  
                lock(listitems)
                    listitems.Add(i);

        }

        private void log_PDO(canpacket[] payloads)
        {
            //return;
 
  //          listView1.BeginInvoke(new MethodInvoker(delegate
   //         {

                //listView1.BeginUpdate();
                
                foreach (canpacket payload in payloads)
                {

                    string[] items = new string[6];
                    items[0] = DateTime.Now.ToString();
                    items[1] = "PDO";
                    items[2] = string.Format("{0:x3}", payload.cob);
                    items[3] = "";
                    items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                    if (pdoprocessors.ContainsKey(payload.cob))
                    {
                        string msg = pdoprocessors[payload.cob](payload.data);

                        if (msg == null)
                            continue;

                        items[5] = msg;
                    }
                    else
                    {
                        items[5] = string.Format("Len = {0}", payload.len);
                    }

                    ListViewItem i = new ListViewItem(items);

                    appendfile(items);


                    lock(listitems)
                        listitems.Add(i);

     //               listView1.Items.Add(i);
                
                }

         }

        private void log_EMCY(canpacket payload)
        {
           // listView1.BeginInvoke(new MethodInvoker(delegate
           // {
                string[] items = new string[6];
                string[] items2 = new string[5];

                items[0] = DateTime.Now.ToString();
                items[1] = "EMCY";
                items[2] = string.Format("{0:x3}", payload.cob);
                items[3] = string.Format("{0:x3}", payload.cob - 0x080);
                items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);
                //items[4] = "EMCY";

                items2[0] = DateTime.Now.ToString();
                items2[1] = items[2];
                items2[2] = items[3];

                UInt16 code = (UInt16)(payload.data[0] + (payload.data[1]<<8));
                byte bits = (byte)(payload.data[3]);
                UInt32 info = (UInt32)(payload.data[4] + (payload.data[5] << 8) + (payload.data[6] << 16) + (payload.data[7] << 24));



                if(errcode.ContainsKey(code))
                {

                    string bitinfo;

                    if(errbit.ContainsKey(bits))
                    {
                        bitinfo = errbit[bits];
                    }
                    else
                    {
                        bitinfo = string.Format("bits 0x{0:x2}", bits);
                    }

                    items[5] = string.Format("Error: {0} - {1} info 0x{2:x8}", errcode[code], bitinfo, info);
                }
                else
                {
                    items[5] = string.Format("Error code 0x{0:x4} bits 0x{1:x2} info 0x{2:x8}", code, bits, info);
                }

                items2[3] = items[4];

                ListViewItem i = new ListViewItem(items);
                ListViewItem i2 = new ListViewItem(items2);

                i.ForeColor = Color.White;
                i2.ForeColor = Color.White;

                if (code == 0)
                {
                    i.BackColor = Color.Green;
                    i2.BackColor = Color.Green;

                }
                else
                {
                    i.BackColor = Color.Red;
                    i2.BackColor = Color.Red;

                } 

                

 //               listView1.BeginUpdate();
 //               listView1.Items.Add(i);
 //               listView1.EndUpdate();
 //               if (checkbox_autoscroll.Checked)
 //                   listView1.EnsureVisible(listView1.Items.Count - 1);
            //}));
 
              lock(listitems)
                    listitems.Add(i);

              appendfile(items);


              lock (EMClistitems)
                  EMClistitems.Add(i2);

        }


        Dictionary<UInt16, string> errcode = new Dictionary<ushort, string>();
        Dictionary<UInt16, string> errbit = new Dictionary<ushort, string>();

        private void interror()
        {

            errcode.Add(0x0000, "error Reset or No Error");
            errcode.Add(0x1000, "Generic Error");
            errcode.Add(0x2000, "Current");
            errcode.Add(0x2100, "device input side");
            errcode.Add(0x2200, "Current inside the device");
            errcode.Add(0x2300, "device output side");
            errcode.Add(0x3000, "Voltage");
            errcode.Add(0x3100, "Mains Voltage");
            errcode.Add(0x3200, "Voltage inside the device");
            errcode.Add(0x3300, "Output Voltage");
            errcode.Add(0x4000, "Temperature");
            errcode.Add(0x4100, "Ambient Temperature");
            errcode.Add(0x4200, "Device Temperature");
            errcode.Add(0x5000, "Device Hardware");
            errcode.Add(0x6000, "Device Software");
            errcode.Add(0x6100, "Internal Software");
            errcode.Add(0x6200, "User Software");
            errcode.Add(0x6300, "Data Set");
            errcode.Add(0x7000, "Additional Modules");
            errcode.Add(0x8000, "Monitoring");
            errcode.Add(0x8100, "Communication");
            errcode.Add(0x8110, "CAN Overrun (Objects lost)");
            errcode.Add(0x8120, "CAN in Error Passive Mode");
            errcode.Add(0x8130, "Life Guard Error or Heartbeat Error");
            errcode.Add(0x8140, "recovered from bus off");
            errcode.Add(0x8150, "CAN-ID collision");
            errcode.Add(0x8200, "Protocol Error");
            errcode.Add(0x8210, "PDO not processed due to length error");
            errcode.Add(0x8220, "PDO length exceeded");
            errcode.Add(0x8230, "destination object not available");
            errcode.Add(0x8240, "Unexpected SYNC data length");
            errcode.Add(0x8250, "RPDO timeout");
            errcode.Add(0x9000, "External Error");
            errcode.Add(0xF000, "Additional Functions");
            errcode.Add(0xFF00, "Device specific");

            errcode.Add(0x2310, "Current at outputs too high (overload)");
            errcode.Add(0x2320, "Short circuit at outputs");
            errcode.Add(0x2330, "Load dump at outputs");
            errcode.Add(0x3110, "Input voltage too high");
            errcode.Add(0x3120, "Input voltage too low");
            errcode.Add(0x3210, "Internal voltage too high");
            errcode.Add(0x3220, "Internal voltage too low");
            errcode.Add(0x3310, "Output voltage too high");
            errcode.Add(0x3320, "Output voltage too low");

            errbit.Add(0x00, "Error Reset or No Error");
            errbit.Add(0x01, "CAN bus warning limit reached");
            errbit.Add(0x02, "Wrong data length of the received CAN message");
            errbit.Add(0x03, "Previous received CAN message wasn't processed yet");
            errbit.Add(0x04, "Wrong data length of received PDO");
            errbit.Add(0x05, "Previous received PDO wasn't processed yet");
            errbit.Add(0x06, "CAN receive bus is passive");
            errbit.Add(0x07, "CAN transmit bus is passive");
            errbit.Add(0x08, "Wrong NMT command received");
            errbit.Add(0x09, "(unused)");
            errbit.Add(0x0A, "(unused)");
            errbit.Add(0x0B, "(unused)");
            errbit.Add(0x0C, "(unused)");
            errbit.Add(0x0D, "(unused)");
            errbit.Add(0x0E, "(unused)");
            errbit.Add(0x0F, "(unused)");

            errbit.Add(0x10, "(unused)");
            errbit.Add(0x11, "(unused)");
            errbit.Add(0x12, "CAN transmit bus is off");
            errbit.Add(0x13, "CAN module receive buffer has overflowed");
            errbit.Add(0x14, "CAN transmit buffer has overflowed");
            errbit.Add(0x15, "TPDO is outside SYNC window");
            errbit.Add(0x16, "(unused)");
            errbit.Add(0x17, "(unused)");
            errbit.Add(0x18, "SYNC message timeout");
            errbit.Add(0x19, "Unexpected SYNC data length");
            errbit.Add(0x1A, "Error with PDO mapping");
            errbit.Add(0x1B, "Heartbeat consumer timeout");
            errbit.Add(0x1C, "Heartbeat consumer detected remote node reset");
            errbit.Add(0x1D, "(unused)");
            errbit.Add(0x1E, "(unused)");
            errbit.Add(0x1F, "(unused)");

            errbit.Add(0x20, "Emergency message wasn't sent");
            errbit.Add(0x21, "(unused)");
            errbit.Add(0x22, "Microcontroller has just started");
            errbit.Add(0x23, "(unused)");
            errbit.Add(0x24, "(unused)");
            errbit.Add(0x25, "(unused)");
            errbit.Add(0x26, "(unused)");
            errbit.Add(0x27, "(unused)");

            errbit.Add(0x28, "Wrong parameters to CO_errorReport() function");
            errbit.Add(0x29, "Timer task has overflowed");
            errbit.Add(0x2A, "Unable to allocate memory for objects");
            errbit.Add(0x2B, "test usage");
            errbit.Add(0x2C, "Software error");
            errbit.Add(0x2D, "Object dictionary does not match the software");
            errbit.Add(0x2E, "Error in calculation of device parameters");
            errbit.Add(0x2F, "Error with access to non volatile device memory");

            sdoerrormessages.Add(0x05030000, "Toggle bit not altered");
            sdoerrormessages.Add(0x05040000, "SDO protocol timed out");
            sdoerrormessages.Add(0x05040001, "Command specifier not valid or unknown");
            sdoerrormessages.Add(0x05040002, "Invalid block size in block mode");
            sdoerrormessages.Add(0x05040003, "Invalid sequence number in block mode");
            sdoerrormessages.Add(0x05040004, "CRC error (block mode only)");
            sdoerrormessages.Add(0x05040005, "Out of memory");
            sdoerrormessages.Add(0x06010000, "Unsupported access to an object");
            sdoerrormessages.Add(0x06010001, "Attempt to read a write only object");
            sdoerrormessages.Add(0x06010002, "Attempt to write a read only object");
            sdoerrormessages.Add(0x06020000, "Object does not exist");
            sdoerrormessages.Add(0x06040041, "Object cannot be mapped to the PDO");
            sdoerrormessages.Add(0x06040042, "Number and length of object to be mapped exceeds PDO length");
            sdoerrormessages.Add(0x06040043, "General parameter incompatibility reasons");
            sdoerrormessages.Add(0x06040047, "General internal incompatibility in device");
            sdoerrormessages.Add(0x06060000, "Access failed due to hardware error");
            sdoerrormessages.Add(0x06070010, "Data type does not match, length of service parameter does not match");
            sdoerrormessages.Add(0x06070012, "Data type does not match, length of service parameter too high");
            sdoerrormessages.Add(0x06070013, "Data type does not match, length of service parameter too short");
            sdoerrormessages.Add(0x06090011, "Sub index does not exist");
            sdoerrormessages.Add(0x06090030, "Invalid value for parameter (download only).");
            sdoerrormessages.Add(0x06090031, "Value range of parameter written too high");
            sdoerrormessages.Add(0x06090032, "Value range of parameter written too low");
            sdoerrormessages.Add(0x06090036, "Maximum value is less than minimum value.");
            sdoerrormessages.Add(0x060A0023, "Resource not available: SDO connection");
            sdoerrormessages.Add(0x08000000, "General error");
            sdoerrormessages.Add(0x08000020, "Data cannot be transferred or stored to application");
            sdoerrormessages.Add(0x08000021, "Data cannot be transferred or stored to application because of local control");
            sdoerrormessages.Add(0x08000022, "Data cannot be transferred or stored to application because of present device state");
            sdoerrormessages.Add(0x08000023, "Object dictionary not present or dynamic generation fails");
            sdoerrormessages.Add(0x08000024, "No data available");


        }



        private void button_open_Click(object sender, EventArgs e)
        {
            try
            {
                lco.close();

                if(button_open.Text == "Close")
                {

                    if (sw != null)
                        sw.Close();
                    button_open.Text = "Open";

                    textBox_info.AppendText("PORT CLOSED\r\n");
                    return;
                }

                if(comboBox_port.SelectedItem==null)
                {
                    comboBox_port.Text = "";
                    return;
                }

                string port = comboBox_port.SelectedItem.ToString();

                textBox_info.AppendText("Trying to open port .. "+port+"\r\n");

                if (port == "ipc://can_id0")
                {

                    lco.open(port);
                }
                else
                {

                    int iport = int.Parse(port.Substring(3));

                    int rate = comboBox_rate.SelectedIndex;

                    lco.open(iport, (BUSSPEED)rate);
                }

                button_open.Text = "Close";

                sw = new StreamWriter("canlog.txt", true);

                textBox_info.AppendText("Success port open\r\n");

            }
            catch(Exception ex)
            {
                textBox_info.AppendText("ERROR opening port "+ex.ToString()+"\r\n");
                MessageBox.Show("Setup error " + ex.ToString());

            }

            button_open.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsMgr.writeXML(Path.Combine(appdatafolder, "settings.xml"));
            lco.close();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            lock (listitems)
            {
                listitems.Clear();
                listView1.Items.Clear();
            }

            lock(EMClistitems)
            {
                EMClistitems.Clear();
                listView_emcy.Items.Clear();
            }

            lock(dirtyNMTstates)
            {
                NMTstate.Clear();
                dirtyNMTstates.Clear();
                listView_nmt.Items.Clear();
            }

        }

        /*
        private void button_sendsdo_Click(object sender, EventArgs e)
        {

            UInt16 index = Convert.ToUInt16(textBox_sdoindex.Text,16);
            byte sub = Convert.ToByte(textBox_sdosub.Text,16);
            byte node = Convert.ToByte(textBox_node.Text,16);
            UInt32 udata;

            switch(comboBox_type.SelectedItem.ToString())
            {
                case "U8":
                    {
                        byte data = Convert.ToByte(textBox_value.Text);
                        lco.SDOwrite(node, index, sub, data, SDOWriteCallback);
                    }
                    break;

                case "U16":
                    {
                        UInt16 data = Convert.ToUInt16(textBox_value.Text);
                        lco.SDOwrite(node, index, sub, data, SDOWriteCallback);
                    }
                    break;

                case "U32":
                    {
                        UInt32 data = Convert.ToUInt32(textBox_value.Text);
                        lco.SDOwrite(node, index, sub, data, SDOWriteCallback);
                    }
                    break;

                case "Float":
                    {
                        
                        float data = Convert.ToSingle(textBox_value.Text);
                        lco.SDOwrite(node, index, sub, data, SDOWriteCallback);
                    }
                    break;

            }


            
        }

        public void SDOWriteCallback(SDO sdo)
        {

        }
         * 

        private void button_eepromreset_Click(object sender, EventArgs e)
        {
            byte node = Convert.ToByte(textBox_node.Text, 16);

            lco.SDOwrite(node, 0x1011, 0x01, 0x64616f6c, null);
            lco.SDOwrite(node, 0x1011, 0x7f, 0x64616f6c, null);
            lco.SDOwrite(node, 0x1010, 0x01, 0x65766173, null);

        }

        private void button_readAD_Click(object sender, EventArgs e)
        {
            byte node = Convert.ToByte(textBox_node.Text, 16);

            lco.SDOread(node,0x6401,0x03,ADcallback);

        }

        private void ADcallback(SDO sdo)
        {

            textBox_ad.Invoke(new MethodInvoker(delegate
            {
                textBox_ad.Text = string.Format("{0:0.00}", (Int16)sdo.expitideddata);

            }));


        }
         * */

        private void button_sdo_Click(object sender, EventArgs e)
        {
            SDOEditor sdo = new SDOEditor(lco);
            sdo.Show();
        }

        private void sDOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SDOEditor sdo = new SDOEditor(lco);
            sdo.Show();
        }

        private void eEPROMResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetEEPROM re = new ResetEEPROM(lco);
            re.ShowDialog();         
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsMgr.settings.options.selectedport = comboBox_port.SelectedItem.ToString();
            
        }

        private void comboBox_rate_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsMgr.settings.options.selectedrate = comboBox_rate.SelectedIndex;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox_rate.SelectedIndex = SettingsMgr.settings.options.selectedrate;
            comboBox_port.SelectedItem = SettingsMgr.settings.options.selectedport;

           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkbox_autoscroll.Checked)
            {
                if (listView1.Items.Count>1)
                    listView1.EnsureVisible(listView1.Items.Count - 1);
            }
        }

        private void nMTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NMTFrm frm = new NMTFrm(lco);
            frm.Show();
        }

        private void errorInjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorInject ei = new ErrorInject(lco);
            ei.Show();

        }

        private void flashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Flasher f = new Flasher(lco);
            f.ShowDialog();

        }

        private void ManualSDOToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Encoding u8 = Encoding.UTF8;

            byte[] data = new byte[8];
            data[0] = 0xfe;
            data[1] = 18;
            data[2] = 0x00;
            data[3] = 0x00;
            data[4] = 0x00;
            data[5] = 0x00;
            data[6] = 0x00;
            data[7] = 0xfd;



            lco.SDOwrite(0x04, 0x2010, 0x01, data, null);

        }

        private void sDOUPLOADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lco.SDOread(0x04, 0x2010, 0x02, null);
        }

        private void sTARTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //0x185 bit0x20
            byte[] data = new byte[2];
            data[0] = 0x20 | 0x10 | 0x02;
            lco.writePDO(0x185, data);
        }

        private void pDOTESTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1];
            data[0] = 0xff;
            lco.writePDO(0x202, data);
        }

        private void charge100vToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ChgrFrm frm = new ChgrFrm(lco);

            frm.Show();

        }

        private void stopChargeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            byte[] data = new byte[2];
            data[0] = 0x00;
            data[1] = 0x00;

            lco.writePDO(0x214, data);
        }

       
      

        private void loadPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Libraries (*.dll)|*.dll";
            ofd.Multiselect = false;

            if(ofd.ShowDialog()==DialogResult.OK)
            {
                textBox_info.AppendText("Attempting to load plugin " + ofd.FileName+"\r\n");
                try
                {
                    Assembly assembly = Assembly.LoadFrom(ofd.FileName);

                    Type[] types = assembly.GetExportedTypes();

                    for (int i = 0; i < types.Length; i++)
                    {
                        Type type = assembly.GetType(types[i].FullName);
                        if (type.GetInterface("PDOInterface.IPDOParser") != null)
                        {
                            object obj = Activator.CreateInstance(type);
                            if (obj != null)
                            {
                                ipdo = (IPDOParser)obj;
                                ipdo.registerPDOS(pdoprocessors);
                                ipdo.setlco(lco);

                                textBox_info.AppendText("SUCCESS loading plugin \r\n");
                            }
                        }

                        if (type.GetInterface("PDOInterface.IInterfaceService") != null)
                        {
                            object obj = Activator.CreateInstance(type);
                            if (obj != null)
                            {
                                IInterfaceService iss = (IInterfaceService)obj;
                                IVerb[] verbs = iss.GetVerbs("Tools");

                                if (verbs != null)
                                {
                                    foreach (IVerb v in verbs)
                                    {
                                        ToolStripMenuItem item = new ToolStripMenuItem(v.Name, null, v.Action);
                                        toolsToolStripMenuItem.DropDownItems.Add(item);
                                        ;
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    textBox_info.AppendText("Failed loading plugi \r\n"+ex.ToString()+"\r\n");
                    MessageBox.Show("Very unawesome when tryng to load plugin..");
                }

            }
        }
    }



    public static class ControlExtensions
    {
        public static void DoubleBuffering(this Control control, bool enable)
        {
            var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
        }
    }
}
