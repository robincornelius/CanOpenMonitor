using libCanopenSimple;
using PFMMeasurementService.Models.Devices.Buses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CanMonitor
{
    public class DriverLoader
    {
        public event EventHandler<EventArgs> DeviceListChanged;

        List<string> drivers = new List<string>();
        private readonly string driverDirectory;
        private readonly List<string> discoveryErrors = new List<string>();

        public IEnumerable<string> DiscoveryErrors
        {
            get
            {
                lock (discoveryErrors)
                {
                    return discoveryErrors.ToArray();
                }
            }
        }

        public List<driverport> _driverport;

        IComPortManagerInterface cpm = null;

        BUSSPEED rate = BUSSPEED.BUS_500Kbit;

        public delegate void PortChangedEvent(object sender, EventArgs e);
        public event PortChangedEvent portchangedevent;

        public DriverLoader() : this(Path.GetDirectoryName(typeof(DriverLoader).Assembly.Location),
            () => new ComPortManagerModel())
        {
        }

        internal DriverLoader(string applicationDirectory, Func<IComPortManagerInterface> createPortManager)
        {
            driverDirectory = Path.Combine(applicationDirectory, "drivers");
            try
            {
                // WMI hot-plug discovery is optional; native drivers can still enumerate without it.
                cpm = createPortManager();
                if (cpm != null)
                {
                    cpm.DeviceListChanged += Cpm_DeviceListChanged;
                }
            }
            catch (Exception error)
            {
                ReportDiscoveryError("WMI port discovery unavailable: " + error.Message);
            }
        }

        private void ReportDiscoveryError(string message)
        {
            lock (discoveryErrors)
            {
                if (discoveryErrors.Contains(message))
                {
                    return;
                }
                discoveryErrors.Add(message);
            }
            Trace.TraceWarning(message);
        }

        public void finddrivers()
        {

            drivers.Clear();
            if (!Directory.Exists(driverDirectory))
            {
                ReportDiscoveryError("Driver directory is missing: " + driverDirectory);
                return;
            }
            try
            {
                foreach (string driver in Directory.GetFiles(driverDirectory, "*.dll"))
                {
                    drivers.Add(Path.Combine(driverDirectory, Path.GetFileNameWithoutExtension(driver)));
                }
            }
            catch (Exception error)
            {
                ReportDiscoveryError("Cannot discover drivers in " + driverDirectory + ": " + error.Message);
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
                    catch (Exception error)
                    {
                        ReportDiscoveryError("Cannot enumerate driver " + s + ": " + error.Message);
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

                List<sComPortModel> psx = cpm == null ? new List<sComPortModel>() : cpm.GetPorts();

                foreach (sComPortModel p in psx)
                {
                    driverport dp = new driverport();

                    if (p.vid == null || p.pid == null)
                        continue;


                    //fixme this was a hack for JLR to autodetect the HMI Can Boards using their
                    //VID PID but it only keeps adding new items to the list
                    //dp.port = string.Format($"USB/VID_{p.vid}/PID_{p.pid}");
                    //dp.PID = p.pid;
                    //dp.VID = p.vid;
                    //dp.driver = "drivers\\can_canusb_win32";
                    
                    
              //      _driverport.Add(dp);
                }
            }

            portchangedevent?.Invoke(this, new EventArgs());

        }

        private void Cpm_DeviceListChanged(object sender, EventArgs e)
        {
            enumerateports();
            DeviceListChanged?.Invoke(this, new EventArgs());
           
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

        public bool Open(driverport dp,BUSSPEED rate)
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

            return Program.lco.open(port, (BUSSPEED)rate, dp.driver);
  
        }

        public void Close()
        {
            Program.lco.close();
        }



    }
}
