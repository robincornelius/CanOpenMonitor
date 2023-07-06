using libCanopenSimple;
using PFMMeasurementService.Models.Devices.Buses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CanMonitor
{
    public class DriverLoader
    {
        List<string> drivers = new List<string>();

        public List<driverport> _driverport;

        IComPortManagerInterface cpm = null;

        BUSSPEED rate = BUSSPEED.BUS_500Kbit;

        public delegate void PortChangedEvent(object sender, EventArgs e);
        public event PortChangedEvent portchangedevent;

        public DriverLoader()
        {
            cpm = new ComPortManagerModel(); ;
            cpm.DeviceListChanged += Cpm_DeviceListChanged;
        }

        public void finddrivers()
        {

            //textBox_info.AppendText("Searching for drivers...\r\n\r\n");
            string[] founddrivers = Directory.GetFiles("drivers\\", "*.dll");

            foreach (string driver in founddrivers)
            {
                //textBox_info.AppendText(string.Format("Found driver {0}\r\n", driver));
                drivers.Add(driver.Substring(0, driver.Length - 4));
            }

        }

        public void enumerateports()
        {

            _driverport = new List<driverport>();
            //if (this.IsHandleCreated == false)
            //    return;

            //this.Invoke(new MethodInvoker(delegate ()
            {

                foreach (string s in drivers)
                {
                    //textBox_info.AppendText(String.Format("Attempting to enumerate with driver {0}\r\n", s));

                    try
                    {
                        Program.lco.enumerate(s);
                    }
                    catch (Exception e)
                    {
                        //textBox_info.AppendText(e.ToString() + "\r\n");
                    }
                }

                //textBox_info.AppendText("\r\n");

                foreach (KeyValuePair<string, List<string>> kvp in Program.lco.ports)
                {
                    List<string> ps = kvp.Value;
                    foreach (string s in ps)
                    {
                        driverport dp = new driverport();
                        dp.port = s;
                        dp.driver = kvp.Key;
                        _driverport.Add(dp);
                    }
                }

                List<sComPortModel> psx = cpm.GetPorts();

                foreach (sComPortModel p in psx)
                {
                    driverport dp = new driverport();
                    dp.port = string.Format($"USB/VID_{p.vid}/PID_{p.pid}");
                    dp.PID = p.pid;
                    dp.VID = p.vid;
                    dp.driver = "drivers\\can_canusb_win32";
                    _driverport.Add(dp);
                }
            }

            portchangedevent?.Invoke(this, new EventArgs());

        }

        private void Cpm_DeviceListChanged(object sender, EventArgs e)
        {
            // enumerateports();
        }

        private void S_DeviceDisconnected(object sender, EventArgs e)
        {
            if (Program.lco.isopen())
                Program.lco.close();
        }

        private void S_DeviceConnected(object sender, EventArgs e)
        {
                sComPortModel s = (sComPortModel)sender;

                Program.lco.close();

                //FIXME hardcoded driver
                Program.lco.open(s.port, (BUSSPEED)rate, "drivers\\can_canusb_win32");
        }

        public void Open(driverport dp,BUSSPEED rate)
        {
            Program.lco.close();
            this.rate = rate;

            string port = dp.port;

            if (dp.port.Contains("USB"))
            {
                sComPortModel s = cpm.requestSerialPortById(dp.VID, dp.PID, "", "");
                s.DeviceConnected += S_DeviceConnected;
                s.DeviceDisconnected += S_DeviceDisconnected;
                port = s.port;
            }

            Program.lco.open(port, (BUSSPEED)rate, dp.driver);
  
        }

        public void Close()
        {
            Program.lco.close();
        }



    }
}
