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
using System.Xml.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Globalization;

namespace CanMonitor
{
    public partial class Form1 : Form
    {

        private libCanopenSimple.libCanopenSimple lco = new libCanopenSimple.libCanopenSimple();
      

        private Dictionary<UInt16, Func<byte[], string>> pdoprocessors = new Dictionary<ushort, Func<byte[], string>>();
        private string appdatafolder;
        private string assemblyfolder;

        private Dictionary<string, object> plugins = new Dictionary<string, object>();

        IPDOParser ipdo;

        List<ListViewItem> listitems = new List<ListViewItem>();

        List<ListViewItem> EMClistitems = new List<ListViewItem>();

        List<string> drivers = new List<string>();

        private Dictionary<UInt32, string> sdoerrormessages = new Dictionary<UInt32, string>();

        Dictionary<UInt32, List<byte>> sdotransferdata = new Dictionary<uint, List<byte>>();

        System.Windows.Forms.Timer updatetimer = new System.Windows.Forms.Timer();

        StreamWriter sw;

        private List<string> _mru = new List<string>();

        string gitVersion;

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

        Timer savetimer;

        public Form1()
        {


           // DateTime.Parse("07/14/2021 14:42:18",);


         

            lco.connectionevent += Lco_connectionevent;

            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appdatafolder = Path.Combine(folder, "CanMonitor");

                assemblyfolder = AppDomain.CurrentDomain.BaseDirectory;

                SettingsMgr.readXML(Path.Combine(appdatafolder, "settings.xml"));
            }
            catch (Exception e)
            {
            }

            InitializeComponent();

            checkBox_showSDO.Checked = Properties.Settings.Default.showsdo;
            checkBox_showPDO.Checked = Properties.Settings.Default.showpdo;
            checkBox_heartbeats.Checked = Properties.Settings.Default.showHB;
            checkBox_showNMTEC.Checked = Properties.Settings.Default.showNMTEC;
            checkBox_showNMT.Checked = Properties.Settings.Default.showNMT;
            checkBox_showEMCY.Checked = Properties.Settings.Default.showEMCY;



            textBox_info.AppendText("Searching for drivers...");
            string[] founddrivers = Directory.GetFiles("drivers\\","*.dll");

            foreach(string driver in founddrivers)
            {
                textBox_info.AppendText(string.Format("Found driver {0}", driver));
                drivers.Add(driver.Substring(0, driver.Length - 4));
            }

            enumerateports();

            if (comboBox_port.Items.Count == 0)
            {
                MessageBox.Show("No COM ports detected, if the CanUSB is connected please ensure\n that it is set to Load VCP in properties page in device manager\nOr please insert a device now and press 'R' to refresh port list");
            }

            lco.dbglevel = debuglevel.DEBUG_NONE;

            lco.nmtecevent += log_NMTEC;
            lco.nmtevent += log_NMT;
            lco.sdoevent += log_SDO;
            lco.pdoevent += log_PDO;
            lco.emcyevent += log_EMCY;

            listView1.DoubleBuffering(true);
            listView_emcy.DoubleBuffering(true);
            listView_nmt.DoubleBuffering(true);

            interror();

            listView1.ListViewItemSorter = null;

            updatetimer.Interval = 1000;
            updatetimer.Tick += updatetimer_Tick;
         

            updatetimer_Tick(null, new EventArgs());

            var autoloadPath = Path.Combine(assemblyfolder, "autoload.txt");
            if (File.Exists(autoloadPath))
            {
                string[] autoload = System.IO.File.ReadAllLines(autoloadPath);

                foreach (string plugin in autoload)
                {
                    loadplugin(plugin, false);
                }
            }

            autoloadPath = Path.Combine(appdatafolder, "autoload.txt");
            if (File.Exists(autoloadPath))
            {
                string[] autoload = System.IO.File.ReadAllLines(autoloadPath);

                foreach (string plugin in autoload)
                {
                    loadplugin(plugin, false);
                }
            }


            Properties.Settings.Default.Reload();

            //select last used port
            foreach (driverport dp in comboBox_port.Items)
            {
                
                if (dp.port == Properties.Settings.Default.lastport)
                {
                    comboBox_port.SelectedItem = dp;
                    break;
                }
            }


            this.Shown += Form1_Shown;

            updatetimer.Enabled = true;

            savetimer = new Timer();
            savetimer.Interval = 10;
            savetimer.Tick += Savetimer_Tick;
            savetimer.Enabled = true;

            last = DateTime.Now;
        }

        DateTime last;

        private void Savetimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            if(Properties.Settings.Default.AutoFileLog==true &&  now.Hour != last.Hour)
            {
                //Force a save

                string dtstring = now.ToString("dd-MMM-yyyy-HH-mm-ss");

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                desktopPath += Path.DirectorySeparatorChar + Properties.Settings.Default.FileLogFolder;
                dtstring = desktopPath + Path.DirectorySeparatorChar + dtstring;

                if(!Directory.Exists(desktopPath))
                {
                    Directory.CreateDirectory(desktopPath);
                }


                dtstring += ".xml";
                dosave(dtstring);
            
                listView1.Items.Clear();

            }

            last = DateTime.Now;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.autoconnect == true)
            {
                button_open_Click(this, new EventArgs());
            }
        }

        private void Lco_connectionevent(object sender, EventArgs e)
        {
           //invoked when the underlying libcanopensimple opens or closes a driver conenction
           //send this message to all plugins that care

            foreach(KeyValuePair<string,object> kvp in plugins)
            {
               // if (o is IInterfaceService)
                {
                    IInterfaceService iis = (IInterfaceService)kvp.Value;
                    iis.DriverStateChange((ConnectionChangedEventArgs)e);
                }
            }


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

            bool limit = Properties.Settings.Default.limitlines;
            int linelimit = Properties.Settings.Default.linelimit;

            if (listitems.Count != 0)
                lock (listitems)
                {
                    listView1.BeginUpdate();

                    listView1.Items.AddRange(listitems.ToArray());

                    listitems.Clear();

                    if (checkbox_autoscroll.Checked && listView1.Items.Count > 2)
                        listView1.EnsureVisible(listView1.Items.Count - 1);

                    if (limit)
                    {
                        while (listView1.Items.Count > linelimit)
                            listView1.Items.RemoveAt(0);

                    }

                    listView1.EndUpdate();

                }

            lock (NMTstate)
            {
                if (dirtyNMTstates.Count > 0)
                {
                    listView_nmt.BeginUpdate();

                    foreach (SNMTState state in dirtyNMTstates)
                    {
                        if (state.isnew)
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

                    if (limit)
                    {
                        while (listView_emcy.Items.Count > linelimit)
                            listView_emcy.Items.RemoveAt(0);

                    }

                    listView1.EndUpdate();


                    listView_emcy.EndUpdate();
                }
            }

        }

        private void log_NMT(canpacket payload, DateTime dt)
        {

            if (!checkBox_showNMT.Checked)
                return;

            string[] items = new string[6];
            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
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

            if (payload.data[1] == 0)
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

            lock (listitems)
                listitems.Add(i);

            appendfile(items);

        }

        private void log_NMTEC(canpacket payload, DateTime dt)
        {

            

            string[] items = new string[6];
            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
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

            if (checkBox_showNMTEC.Checked && (checkBox_heartbeats.Checked == true || payload.data[0] == 0))
            {
                lock (listitems)
                {
                    listitems.Add(i);
                }
            }

            lock (NMTstate)
            {
                byte node = (byte)(payload.cob & 0x0FF);

                if (NMTstate.ContainsKey(node))
                {
                    SNMTState s = NMTstate[node];
                    s.lastupdate = dt;
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
                    s.lastupdate = dt;
                    s.dirty = true;
                    s.state = payload.data[0];
                    s.statemsg = msg;
                    string[] ss = new string[3];
                    ss[0] = DateTime.Now.ToString();
                    ss[1] = string.Format("0x{0:x2} ({1})", node, node);
                    ss[2] = msg;

                    ListViewItem newitem = new ListViewItem(ss);
                    s.LVI = newitem;
                    s.isnew = true;

                    NMTstate.Add(node, s);
                    dirtyNMTstates.Add(NMTstate[node]);
                }

            }



        }

        private void log_SDO(canpacket payload, DateTime dt)
        {

            if (!checkBox_showSDO.Checked)
                return;

            string[] items = new string[6];
            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
            items[1] = "SDO";
            items[2] = string.Format("{0:x3}", payload.cob);

            if (payload.cob >= 0x580 && payload.cob < 0x600)
            {
                items[3] = string.Format("{0:x3}", ((payload.cob + 0x80) & 0x0FF));
            }
            else
            {
                items[3] = string.Format("{0:x3}", payload.cob & 0x0FF);
            }

            items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

            string msg = "";


            int SCS = payload.data[0] >> 5; //7-5

            int n = (0x03 & (payload.data[0] >> 2)); //3-2 data size for normal packets
            int e = (0x01 & (payload.data[0] >> 1)); // expidited flag
            int s = (payload.data[0] & 0x01); // data size set flag //also in block
            int c = s;

            int sn = (0x07 & (payload.data[0] >> 1)); //3-1 data size for segment packets
            int t = (0x01 & (payload.data[0] >> 4));  //toggle flag

            int cc = (0x01 & (payload.data[0] >> 2));



            UInt16 index = (UInt16)(payload.data[1] + (payload.data[2] << 8));
            byte sub = payload.data[3];


            int valid = 7;
            int validsn = 7;


            if (n != 0)
                valid = 8 - (7 - n);

            if (sn != 0)
                validsn = 8 - (7 - sn);


            if (payload.cob >= 0x580 && payload.cob <= 0x600)
            {
                string mode = "";
                string sdoproto = "";

                string setsize = "";

                switch (SCS)
                {
                    case 0:
                        mode = "upload segment response";
                        sdoproto = string.Format("{0} {1} Valid bytes = {2} {3}", mode, t == 1 ? "TOG ON" : "TOG OFF", validsn, c == 0 ? "MORE" : "END");

                        if (sdotransferdata.ContainsKey(payload.cob))
                        {

                            for (int x = 1; x <= validsn; x++)
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

                                textBox_info.Invoke(new MethodInvoker(delegate
                                {
                                    textBox_info.AppendText(String.Format("SDO UPLOAD COMPLETE for cob 0x{0:x3}\r\n", payload.cob));

                                    textBox_info.AppendText(hex.ToString() + "\r\n");
                                    textBox_info.AppendText(ascii.ToString() + "\r\n\r\n");

                                }));
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

                        if (e == 1 && s == 1)
                        {
                            //n is valid
                            nbytes = string.Format("Valid bytes = {0}", 4 - n);
                        }

                        if (e == 0 && s == 1)
                        {
                            byte[] size = new byte[4];
                            Array.Copy(payload.data, 4, size, 0, 4);
                            UInt32 isize = (UInt32)BitConverter.ToUInt32(size, 0);
                            nbytes = string.Format("Bytes = {0}", isize);

                            if (sdotransferdata.ContainsKey(payload.cob))
                                sdotransferdata.Remove(payload.cob);

                            sdotransferdata.Add(payload.cob, new List<byte>());
                        }

                        sdoproto = string.Format("{0} {1} {2} 0x{3:x4}/{4:x2}", mode, nbytes, e == 1 ? "Normal" : "Expedite", index, sub);
                        break;
                    case 3:
                        mode = "initate download response";
                        sdoproto = string.Format("{0} 0x{1:x4}/{2:x2}", mode, index, sub);



                        break;

                    case 5:
                        mode = "Block download response";

                        byte segperblock = payload.data[4];
                        sdoproto = string.Format("{0} 0x{1:x4}/{2:x2} Blksize = {3}", mode, cc == 0 ? "NO SERVER CRC" : "SERVER CRC", index, sub, segperblock);

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


                        if (sdotransferdata.ContainsKey(payload.cob))
                        {

                            for (int x = 1; x <= validsn; x++)
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

                                //sdoproto += "\nDATA = " + hex.ToString() + "(" + ascii + ")";

                                textBox_info.Invoke(new MethodInvoker(delegate
                                {
                                    textBox_info.AppendText(String.Format("SDO DOWNLOAD COMPLETE for cob 0x{0:x3}\n", payload.cob));

                                    textBox_info.AppendText(hex.ToString() + "\n");
                                    textBox_info.AppendText(ascii.ToString() + "\n");
                                }));


                                //Console.WriteLine(hex.ToString());
                                //Console.WriteLine(ascii.ToString());

                                sdotransferdata.Remove(payload.cob);
                            }
                        }


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
                        if (sdotransferdata.ContainsKey(payload.cob))
                            sdotransferdata.Remove(payload.cob);

                        sdotransferdata.Add(payload.cob, new List<byte>());

                        break;
                    case 2:
                        mode = "initate upload request";
                        sdoproto = string.Format("{0} 0x{1:x4}/{2:x2}", mode, index, sub);
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

                        sdoproto = string.Format("{0} 0x{1:x4}/{2:x2} Size = {3}", mode, cc == 0 ? "NO CLIENT CRC" : "CLIENT CRC", index, sub, isize);
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
                if (ipdo != null)
                    msg += " " + ipdo.decodesdo(payload.cob, index, sub, payload.data);
            }


            items[5] = msg;
            appendfile(items);

            ListViewItem i = new ListViewItem(items);

            if ((payload.data[0] & 0x80) != 0)
            {
                i.BackColor = Color.Orange;
            }




            i.ForeColor = Color.DarkBlue;

            lock (listitems)
                listitems.Add(i);

        }

        private void log_PDO(canpacket[] payloads, DateTime dt)
        {
            if (!checkBox_showPDO.Checked)
                return;

            foreach (canpacket payload in payloads)
            {

                string[] items = new string[6];
                items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
                items[1] = "PDO";
                items[2] = string.Format("{0:x3}", payload.cob);
                items[3] = "";
                items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                if (pdoprocessors.ContainsKey(payload.cob))
                {
                    string msg = null;
                    try
                    {
                        msg = pdoprocessors[payload.cob](payload.data);
                    }
                    catch (Exception e)
                    {
                        msg += "!! DECODE EXCEPTION !!";
                    }

                    if (msg == null)
                    {
                        continue;

                    }
                    else
                    {
                        items[5] = msg;
                    }
                }
                else
                {
                    items[5] = string.Format("Len = {0}", payload.len);
                }

                ListViewItem i = new ListViewItem(items);

                appendfile(items);


                lock (listitems)
                    listitems.Add(i);
            }

        }

        private void log_EMCY(canpacket payload, DateTime dt)
        {
            string[] items = new string[6];
            string[] items2 = new string[5];

            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
            items[1] = "EMCY";
            items[2] = string.Format("{0:x3}", payload.cob);
            items[3] = string.Format("{0:x3}", payload.cob - 0x080);
            items[4] = BitConverter.ToString(payload.data).Replace("-", string.Empty);
            //items[4] = "EMCY";

            items2[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff");
            items2[1] = items[2];
            items2[2] = items[3];

            UInt16 code = (UInt16)(payload.data[0] + (payload.data[1] << 8));
            byte bits = (byte)(payload.data[3]);
            UInt32 info = (UInt32)(payload.data[4] + (payload.data[5] << 8) + (payload.data[6] << 16) + (payload.data[7] << 24));



            if (errcode.ContainsKey(code))
            {

                string bitinfo;

                if (errbit.ContainsKey(bits))
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

            items2[3] = items[5];

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

            if (checkBox_showSDO.Checked)
            {
                lock (listitems)
                    listitems.Add(i);
            }

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

                if (button_open.Text == "Close")
                {

                    if (sw != null)
                        sw.Close();
                    button_open.Text = "Open";

                    textBox_info.AppendText("PORT CLOSED\r\n");
                    return;
                }

                if (comboBox_port.SelectedItem == null)
                {
                    comboBox_port.Text = "";
                    return;
                }

                driverport dp = (driverport)comboBox_port.SelectedItem;

                textBox_info.AppendText(String.Format("Trying to open port {0} using driver {1} \r\n", dp.port,dp.driver));

                int rate = comboBox_rate.SelectedIndex;
                lco.open(dp.port, (BUSSPEED)rate, dp.driver);

                if (lco.isopen())
                {
                    button_open.Text = "Close";

                    //fixme make this user selectable from GUI
                    //sw = new StreamWriter("canlog.txt", true);

                    textBox_info.AppendText("Success port open\r\n");

                    Properties.Settings.Default.lastport = dp.port;
                    Properties.Settings.Default.lastdriver = dp.driver;
                    Properties.Settings.Default.Save();

                }
                else
                {
                    button_open.Text = "Open";
                    textBox_info.AppendText("ERROR opening port\r\n");
                }
            }
            catch (Exception ex)
            {
                textBox_info.AppendText("ERROR opening port " + ex.ToString() + "\r\n");
                MessageBox.Show("Setup error " + ex.ToString());

            }

            button_open.Enabled = true;
        }


  

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsMgr.writeXML(Path.Combine(appdatafolder, "settings.xml"));
            lco.close();

            var mruFilePath = Path.Combine(appdatafolder, "PLUGINMRU.txt");
            System.IO.File.WriteAllLines(mruFilePath, _mru);

        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            lock (listitems)
            {
                listitems.Clear();
                listView1.Items.Clear();
            }

            lock (EMClistitems)
            {
                EMClistitems.Clear();
                listView_emcy.Items.Clear();
            }

            lock (dirtyNMTstates)
            {
                NMTstate.Clear();
                dirtyNMTstates.Clear();
                listView_nmt.Items.Clear();
            }

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

            //read git version string, show in title bar 
            //(https://stackoverflow.com/a/15145121)
            string gitVersion = String.Empty;
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("CanMonitor." + "version.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                gitVersion = reader.ReadToEnd();
            }
            if (gitVersion == "")
            {
                gitVersion = "Unknown";
            }
            this.Text += " -- " + gitVersion;
            this.gitVersion = gitVersion;

            var mruFilePath = Path.Combine(appdatafolder, "PLUGINMRU.txt");
            if (System.IO.File.Exists(mruFilePath))
                _mru.AddRange(System.IO.File.ReadAllLines(mruFilePath));

            populateMRU();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkbox_autoscroll.Checked)
            {
                if (listView1.Items.Count > 1)
                    listView1.EnsureVisible(listView1.Items.Count - 1);
            }
        }



        #region pluginloader

        List<string> loadedplugins = new List<string>();
        private void loadplugin(String pfilename, bool addmru)
        {

            if (plugins.ContainsKey(pfilename))
            {
                try
                {
                    IInterfaceService iis = (IInterfaceService)plugins[pfilename];
                    iis.deregisterplugin();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error deregistering plugin --- \n" + e.ToString());
                }

                plugins.Remove(pfilename);
                loadedplugins.Remove(pfilename);
            }

            if (loadedplugins.Contains(pfilename))
                return;

            try
            {

                string filename = pfilename;

                if (!File.Exists(filename))
                {

                    filename = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + pfilename;

                    if (!File.Exists(filename))
                    {
                        filename = appdatafolder + Path.DirectorySeparatorChar + "plugins" + pfilename;

                        if (!File.Exists(filename))
                        {
                            MessageBox.Show(string.Format("Could not find plugin {0}", pfilename));
                            return;
                        }

                    }
                }


                string ext = Path.GetExtension(filename);

                Assembly assembly;


                if (ext == ".cs")
                {
                    CSharpCodeProvider provider = new CSharpCodeProvider();
                    CompilerParameters parameters = new CompilerParameters();

                    // Reference to System.Drawing library
                    parameters.ReferencedAssemblies.Add("System.dll");
                    parameters.ReferencedAssemblies.Add("System.Core.dll");
                    parameters.ReferencedAssemblies.Add("System.Data.dll");
                    parameters.ReferencedAssemblies.Add("PDOInterface.dll");
                    parameters.ReferencedAssemblies.Add("libCanopenSimple.dll");
                    parameters.ReferencedAssemblies.Add("System.Windows.Forms");
                    parameters.ReferencedAssemblies.Add("System.Drawing");




                    // True - memory generation, false - external file generation
                    parameters.GenerateInMemory = true;
                    // True - exe file generation, false - dll file generation
                    parameters.GenerateExecutable = false;

                    string code = File.ReadAllText(filename);

                    CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                    if (results.Errors.HasErrors)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (CompilerError error in results.Errors)
                        {
                            sb.AppendLine(String.Format("{0}: Error ({1}): {2}", error.Line, error.ErrorNumber, error.ErrorText));
                        }

                        MessageBox.Show(sb.ToString());
                        return;

                    }

                    assembly = results.CompiledAssembly;

                }
                else
                {
                    assembly = Assembly.LoadFrom(filename);
                }

                Type[] types = assembly.GetExportedTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    object obj = null;

                    Type type = assembly.GetType(types[i].FullName);
                    if (type.GetInterface("PDOInterface.IInterfaceService") != null)
                    {
                        obj = Activator.CreateInstance(type);
                        if (obj != null)
                        {
                            plugins.Add(filename, obj);
                            IInterfaceService iis = (IInterfaceService)obj;
                            ipdo = (IPDOParser)obj;

                            Dictionary<UInt16, Func<byte[], string>> dictemp = new Dictionary<ushort, Func<byte[], string>>();

                            iis.setlco(lco);

                            iis.preregisterPDOS(pdoprocessors);
                            ipdo.registerPDOS();


                            textBox_info.AppendText(string.Format("SUCCESS loading plugin {0}\r\n", filename));
                        }

                    }

                    if (type.GetInterface("PDOInterface.IInterfaceService") != null)
                    {
                        if (obj != null && obj is PDOInterface.IInterfaceService)
                        {
                            // do nothing use the object to save recreate
                        }
                        else
                        {
                            obj = Activator.CreateInstance(type);
                        }

                        if (obj != null)
                        {
                            IInterfaceService iss = (IInterfaceService)obj;

                            IVerb[] verbsroot = iss.GetVerbs("_root_");

                            if (verbsroot != null)
                            {
                                foreach (IVerb v in verbsroot)
                                {
                                    bool found = false;
                                    foreach (ToolStripItem ii in menuStrip1.Items)
                                    {
                                        if (ii.Text == v.Name)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (found == false)
                                    {

                                        menuStrip1.Items.Add(v.Name);
                                    }

                                }
                            }

                            foreach (ToolStripMenuItem ii in menuStrip1.Items)
                            {
                                IVerb[] verbs = iss.GetVerbs(ii.Text);

                                if (verbs != null)
                                {
                                    foreach (IVerb v in verbs)
                                    {
                                        if (v.Name == "---")
                                        {
                                            ToolStripSeparator item = new ToolStripSeparator();
                                            ii.DropDownItems.Add(item);
                                        }
                                        else
                                        {
                                            ToolStripMenuItem item = new ToolStripMenuItem(v.Name, null, v.Action);
                                            ii.DropDownItems.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                loadedplugins.Add(filename);

                if (addmru)
                {
                    addtoMRU(filename);
                }
            }
            catch (Exception ex)
            {
                textBox_info.AppendText("Failed loading plugi \r\n" + ex.ToString() + "\r\n");
                MessageBox.Show("Error loading plugin :"+pfilename);
            }


        }


        private void loadPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.RestoreDirectory = false;
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "plugins";

            ofd.Filter = "Libraries (*.dll)|*.dll|CSharp Files (*.cs)|*.cs";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox_info.AppendText("Attempting to load plugin " + ofd.FileName + "\r\n");
                loadplugin(ofd.FileName, true);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void addtoMRU(string path)
        {
            // if it already exists remove it then let it readd itsself
            // so it will be promoted to the top of the list
            if (_mru.Contains(path))
                _mru.Remove(path);

            _mru.Insert(0, path);

            if (_mru.Count > 10)
                _mru.RemoveAt(10);

            populateMRU();

        }

        private void populateMRU()
        {

            mnuRecentlyUsed.DropDownItems.Clear();

            foreach (var path in _mru)
            {
                var item = new ToolStripMenuItem(path);
                item.Tag = path;
                item.Click += OpenRecentFile;

                mnuRecentlyUsed.DropDownItems.Add(item);
            }
        }

        void OpenRecentFile(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var filepath = (string)menuItem.Tag;
            loadplugin(filepath, true);
        }

        private void saveDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*.xml)|*.xml";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                dosave(sfd.FileName);
            }

        }

        private void dosave(string filename)
        {
            XElement xeRoot = new XElement("CanOpenMonitor");

            foreach (ListViewItem i in listView1.Items)
            {
                XElement xeRow = new XElement("Packet", new XAttribute("backcol", i.BackColor.Name), new XAttribute("forcol", i.ForeColor.Name));
                int x = 0;

                foreach (ListViewItem.ListViewSubItem subItem in i.SubItems)
                {
                    XElement xeCol = new XElement(listView1.Columns[x].Text);
                    xeCol.Value = subItem.Text;
                    xeRow.Add(xeCol);
                    // To add attributes use XAttributes
                    x++;
                }
                xeRoot.Add(xeRow);

            }

            xeRoot.Save(filename);


        }

        private void loaddata2()
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.ShowHelp = true;
            sfd.Filter = "(*.log)|*.log";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(sfd.FileName);

                foreach(string line in lines)
                {
                    try
                    {

                        string[] bits = line.Split(',');
                        UInt16 cob = Convert.ToUInt16(bits[1], 16);
                        byte len = Convert.ToByte(bits[2], 16);
                        

                        canpacket[] p = new canpacket[1];
                        p[0] = new canpacket();
                        p[0].cob = cob;


                        p[0].data = new byte[len];
                        for(int x=0;x<len;x++)
                        {
                            p[0].data[x] = Convert.ToByte(bits[3].Substring(x * 2, 2), 16);
                        }

                        p[0].len = len;

                        DateTime dt = DateTime.Parse(bits[0]);

                        if (cob < 0x80)
                            log_NMT(p[0], dt);

                        if (cob >= 0x80 && cob < 0x100)
                            log_EMCY(p[0], dt);

                        if (cob >= 0x180 && cob < 0x580)
                            log_PDO(p, dt);

                        if (cob >= 0x580 && cob < 0x700)
                            log_PDO(p, dt);

                        if (cob >= 0x700)
                            log_NMTEC(p[0], dt);


                    }
                    catch(Exception e)
                    {

                    }

                }
            }
        }


        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //loaddata2();
           // return;

            OpenFileDialog sfd = new OpenFileDialog();
            sfd.ShowHelp = true;
            sfd.Filter = "(*.xml)|*.xml";

            try
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {

                    listView1.Items.Clear();

                    XElement xeRoot = XElement.Load(sfd.FileName);
                    XName Packet = XName.Get("Packet");


                    foreach (var packetelement in xeRoot.Elements(Packet))
                    {
                        XName XTimestamp = XName.Get("Timestamp");
                        XName XType = XName.Get("Type");
                        XName XCob = XName.Get("COB");
                        XName XNodeT = XName.Get("Node");
                        XName XPayload = XName.Get("Payload");
                        XName XInfo = XName.Get("Info");

                        string[] bits = new string[6];

                        bits[0] = packetelement.Element(XTimestamp).Value;
                        bits[1] = packetelement.Element(XType).Value;
                        bits[2] = packetelement.Element(XCob).Value;
                        bits[3] = packetelement.Element(XNodeT).Value;
                        bits[4] = packetelement.Element(XPayload).Value;
                        bits[5] = packetelement.Element(XInfo).Value;

                        string cobx = bits[2].ToUpper();
                        UInt16 cob = Convert.ToUInt16(cobx, 16);

                        byte[] b = new byte[bits[4].Length / 2];
                        for (int x = 0; x < bits[4].Length / 2; x++)
                        {
                            string s = bits[4].Substring(x * 2, 2);
                            b[x] = byte.Parse(s, System.Globalization.NumberStyles.HexNumber);
                        }

                        canpacket[] p = new canpacket[1];
                        p[0] = new canpacket();
                        p[0].cob = cob;
                        p[0].data = b;
                        p[0].len = (byte)b.Length;

                        //DateTime dt = DateTime.Parse(bits[0]);

                        DateTime dt = DateTime.ParseExact(bits[0], "MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);

                        switch (bits[1])
                        {
                            case "PDO":
                                log_PDO(p, dt);
                                break;

                            case "SDO":
                                log_SDO(p[0], dt);
                                break;

                            case "NMT":
                                log_NMT(p[0], dt);
                                break;

                            case "NMTEC":
                                log_NMTEC(p[0], dt);
                                break;

                            case "EMCY":
                                log_EMCY(p[0], dt);
                                break;
                        }

                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
                listView1.EndUpdate();

            }

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            enumerateports();
        }


        private void enumerateports()
        {
            comboBox_port.Text = "";
            comboBox_port.Items.Clear();


            textBox_info.AppendText("Enumerating ports....");

            foreach (string s in drivers)
            {
                textBox_info.AppendText(String.Format("Attempting to enumerate with driver {0}",s));

                lco.enumerate(s);
            }

            foreach (KeyValuePair<string, List<string>> kvp in lco.ports)
            {
                List<string> ps = kvp.Value;

                foreach (string s in ps)
                {

                    textBox_info.AppendText(string.Format("Found port {0}", s));

                    driverport dp = new driverport();
                    dp.port = s;
                    dp.driver = kvp.Key;

                    comboBox_port.Items.Add(dp);
                }
            }

        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Prefs p = new Prefs();
            p.ShowDialog();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
               // Hide();
                notifyIcon1.Visible = true;

                notifyIcon1.BalloonTipText = "Can monitor is still running";
                notifyIcon1.BalloonTipTitle = "Can Monitor";
                notifyIcon1.ShowBalloonTip(2000);
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void checkBox_heartbeats_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showHB = checkBox_heartbeats.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox_showPDO_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showpdo = checkBox_showPDO.Checked;
            Properties.Settings.Default.Save();

        }

        private void checkBox_showSDO_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showsdo = checkBox_showSDO.Checked;
            Properties.Settings.Default.Save();

        }

        private void checkBox_nmt_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showNMT = checkBox_showNMT.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox_NMTEC_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showNMTEC = checkBox_showNMTEC.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox_emcy_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.showEMCY = checkBox_showEMCY.Checked;
            Properties.Settings.Default.Save();
        }
    }


    #endregion

    public static class ControlExtensions
    {
        public static void DoubleBuffering(this Control control, bool enable)
        {
            var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
        }
    }

    public class driverport : Object
    {
        public string driver;
        public string port;

        public override string ToString()
        {
            return port;
        }

    }

}
