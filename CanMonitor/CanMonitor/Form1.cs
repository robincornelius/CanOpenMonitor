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


            foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBox_port.Items.Add(portName);
            }


          


            lco.loggercallback_NMT = log_NMT;
            lco.loggercallback_NMTEC = log_NMTEC;
            lco.loggercallback_SDO = log_SDO;
            lco.loggercallback_PDO = log_PDO;
            lco.loggercallback_EMCY = log_EMCY;

            listView1.DoubleBuffering(true);


           // Assembly assembly = Assembly.LoadFrom("..\\..\\..\\BWPDOParser\\bin\\Debug\\BWPDOParser.dll");
             Assembly assembly = Assembly.LoadFrom("..\\..\\..\\JLRParser\\bin\\Debug\\JLRParser.dll");
            
            Type[] types = assembly.GetExportedTypes();

            for (int i = 0; i < types.Length; i++)
            {
                Type type = assembly.GetType(types[i].FullName);
                if (type.GetInterface("PDOInterface.IPDOParser") != null)
                {
                    object obj = Activator.CreateInstance(type);
                    if (obj != null)
                    {
                        IPDOParser ipdo = (IPDOParser)obj;
                        ipdo.registerPDOS(pdoprocessors);
                    }
                }
 
             }

        }

        private void log_NMT(canpacket payload)
        {
            listView1.BeginInvoke(new MethodInvoker(delegate
            {
                string[] items = new string[5];
                items[0] = "NMT";
                items[1] = string.Format("{0:x3}", payload.cob);
                items[2] = "";
                items[3] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                string msg = "";
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

                items[4] = msg;

                ListViewItem i = new ListViewItem(items);

                i.ForeColor = Color.Red;

                listView1.BeginUpdate();
                listView1.Items.Add(i);
                listView1.EndUpdate();
              
            }));

           

        }

        private void log_NMTEC(canpacket payload)
        {
            listView1.BeginInvoke(new MethodInvoker(delegate
            {
                string[] items = new string[5];
                items[0] = "NMTEC";
                items[1] = string.Format("{0:x3}", payload.cob);
                items[2] = string.Format("{0:x3}", payload.cob & 0x0FF);
                items[3] = BitConverter.ToString(payload.data).Replace("-", string.Empty);


                if (checkBox_heartbeats.Checked == false && payload.data[0] != 0)
                    return;


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

                items[4] = msg;

                ListViewItem i = new ListViewItem(items);

                i.ForeColor = Color.DarkGreen;
                listView1.BeginUpdate();
                listView1.Items.Add(i);
                listView1.EndUpdate();
                //if (checkbox_autoscroll.Checked)
                //    listView1.EnsureVisible(listView1.Items.Count - 1);
            }));
        }

        private void log_SDO(canpacket payload)
        {
            listView1.BeginInvoke(new MethodInvoker(delegate
            {
                string[] items = new string[5];
                items[0] = "SDO";
                items[1] = string.Format("{0:x3}", payload.cob);
                items[2] = string.Format("{0:x3}", payload.cob & 0x0FF);
                items[3] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                string msg="";

                if(payload.cob >=0x600)
                {
                    msg += "TX";
                }
                else
                {
                    msg += "RX";
                }


                int SCS = payload.data[0] >> 5; //7-5

                int n = (0x03 & (payload.data[0] >> 2)); //3-2 data size for normal packets
                int e = (0x01 & (payload.data[0] >> 1)); // expidited flag
                int s = (payload.data[0] & 0x01); // data size set flag

                int sn = (0x07 & (payload.data[0] >> 1)); //3-1 data size for segment packets
                int t = (0x01 & (payload.data[0] >> 4));  //toggle flag

                switch(SCS)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;

                }


                UInt16 index =(UInt16)(payload.data[1] + (payload.data[2] << 8));
                byte sub = payload.data[3];

                string.Format("0x{0:x4}/{1:x2}", index, sub);
                msg += string.Format("SCS {0} size {1} expidited {2} size set {3} seg size {4} tottle {5}", SCS, n, e, s, sn, t);

                items[4] = msg;

                ListViewItem i = new ListViewItem(items);

                if ((payload.data[0] & 0x80) != 0)
                {
                    i.BackColor = Color.Orange;
                }

                i.ForeColor = Color.DarkBlue;
                listView1.BeginUpdate();
                listView1.Items.Add(i);
                listView1.EndUpdate();

                if (checkbox_autoscroll.Checked)
                    listView1.EnsureVisible(listView1.Items.Count - 1);


            }));
        }

        private void log_PDO(canpacket payload)
        {
            listView1.BeginInvoke(new MethodInvoker(delegate
            {
                string[] items = new string[5];
                items[0] = "PDO";
                items[1] = string.Format("{0:x3}", payload.cob);
                items[2] = "";
                items[3] = BitConverter.ToString(payload.data).Replace("-", string.Empty);

                if (pdoprocessors.ContainsKey(payload.cob))
                {
                    string msg = pdoprocessors[payload.cob](payload.data);

                    if (msg == null)
                        return;

                    items[4] = msg;
                }
                else
                {
                    items[4] = string.Format("Len = {0}", payload.len);
                }

                ListViewItem i = new ListViewItem(items);

                listView1.BeginUpdate();
                listView1.Items.Add(i);
                listView1.EndUpdate();
                if (checkbox_autoscroll.Checked)
                    listView1.EnsureVisible(listView1.Items.Count - 1);
            }));
        }

        private void log_EMCY(canpacket payload)
        {
            listView1.BeginInvoke(new MethodInvoker(delegate
            {
                string[] items = new string[5];
                items[0] = "EMCY";
                items[1] = string.Format("{0:x3}", payload.cob);
                items[2] = string.Format("{0:x3}", payload.cob - 0x080);
                items[3] = BitConverter.ToString(payload.data).Replace("-", string.Empty);
                //items[4] = "EMCY";

                UInt16 code = (UInt16)(payload.data[0] + (payload.data[1]<<8));
                byte bits = (byte)(payload.data[3]);
                UInt32 info = (UInt32)(payload.data[4] + (payload.data[5] << 8) + (payload.data[6] << 16) + (payload.data[7] << 24));

                items[4] = string.Format("Error code 0x{0:x4} bits 0x{1:x2} info 0x{2:x8}", code, bits, info);

                ListViewItem i = new ListViewItem(items);

                i.ForeColor = Color.White;

                if (code == 0)
                {
                    i.BackColor = Color.Green;
                }
                else
                {
                    i.BackColor = Color.Red;
                } 

                listView1.BeginUpdate();
                listView1.Items.Add(i);
                listView1.EndUpdate();
                if (checkbox_autoscroll.Checked)
                    listView1.EnsureVisible(listView1.Items.Count - 1);
            }));
        }


        private void button_open_Click(object sender, EventArgs e)
        {
            try
            {
                lco.close();

                if(button_open.Text == "Close")
                {
                    button_open.Text = "Open";
                    return;
                }

                string port = comboBox_port.SelectedItem.ToString();
                int iport = int.Parse(port.Substring(3));

                int rate = comboBox_rate.SelectedIndex;

                lco.open(iport, (BUSSPEED)rate);

                button_open.Text = "Close";


            }
            catch(Exception ex)
            {
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
            listView1.Items.Clear();
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
