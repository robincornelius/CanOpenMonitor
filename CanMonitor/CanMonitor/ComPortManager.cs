using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Management;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PFMMeasurementService.Models.Devices.Buses
{

    public class sComPortModel
    {
        public string name;
        public string vid;
        public string pid;
        public string serial;
        public string description;
        public string wmipath;
        bool connected;
        public bool present = true;

        public string port { get; set; }

        public event EventHandler<EventArgs> DeviceDisconnected;
        public event EventHandler<EventArgs> DeviceConnected;

        public void onDisconnect()
        {
            DeviceDisconnected?.Invoke(this, new EventArgs());
        }

        public void onConnect(string name = null)
        {
            if (name == null)
                name = string.Empty;

            if (present)
            {
                DeviceConnected?.Invoke(this, new EventArgs());
            }
            else
            {
            }
        }

    }

    public class ComPortManagerModel : IComPortManagerInterface, IDisposable
    {

        public event EventHandler<EventArgs> DeviceListChanged;

        private readonly dynamic _status;

        private const string vidPattern = @"VID_([0-9A-F]{4})";
        private const string pidPattern = @"PID_([0-9A-F]{4})";
        private const string portPattern = @"(COM[0-9]*)";

        private List<sComPortModel> ports = new List<sComPortModel>();

        ManagementEventWatcher watcherArrive = null;
        ManagementEventWatcher watcherDepart = null;

        readonly Guid ClassUSBSerial = new Guid("4d36e978-e325-11ce-bfc1-08002be10318");
        readonly Guid ClassUSBSerialFTDIMode = new Guid("36fc9e60-c465-11cf-8056-444553540000");
        readonly Guid ClassUSB = new Guid("a503e2d3-a031-49dc-b684-c99085dbfe92");

        public ComPortManagerModel()
        {
            ports = EnumerateSerialPorts();
            initWatcher();
        }

        public List<sComPortModel> GetPorts()
        {
            return ports;
        }

        private List<sComPortModel> EnumerateSerialPorts()
        {

            using (var searcher = new ManagementObjectSearcher("root\\CIMV2",
                string.Format("SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{{{0}}}\" OR ClassGuid=\"{{{1}}}\" ", ClassUSBSerial, ClassUSB)))
            {
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();

                List<sComPortModel> allports = ports.Select(p =>
                {
                    sComPortModel c = adddeviceToManagementTable(p);
                    return c;
                }).ToList();

                updateDeviceStatus();

                foreach (var p in allports)
                {
                  //  _logger.LogInformation($"ComPortManager found serial port : {p.name} {p.description} PID={p.vid} VID={p.pid}");
                }

                return allports;
            }
        }


        public List<sComPortModel> findByID(string vid, string pid)
        {
            //if we want to find one device
            sComPortModel com = ports.FindLast(c => c.vid.Equals(vid) && c.pid.Equals(pid));
            //or if we want to extract all devices with specified values:
            List<sComPortModel> coms = ports.FindAll(c => c.vid.Equals(vid) && c.pid.Equals(pid));
            return coms;
        }

        private void updateDeviceStatus()
        {
        }

        private void initWatcher()
        {
            WqlEventQuery query;
            ManagementOperationObserver observer = new ManagementOperationObserver();

            var scope = new ManagementScope("root\\CIMV2") { Options = { EnablePrivileges = true } };
            const string plugInSql = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'";
            const string unpluggedSql = "SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'";

            scope.Options.EnablePrivileges = true;
            try
            {

                var pluggedInQuery = new WqlEventQuery(plugInSql);
                watcherArrive = new ManagementEventWatcher(scope, pluggedInQuery);
                watcherArrive.EventArrived += OnUsbConnected;
                watcherArrive.Start();

                var unPluggedQuery = new WqlEventQuery(unpluggedSql);
                watcherDepart = new ManagementEventWatcher(scope, unPluggedQuery);
                watcherDepart.EventArrived += OnUsbDisconnected;
                watcherDepart.Start();

            }
            catch (Exception)
            {
                //handle1 exception
            }

        }

        private sComPortModel adddeviceToManagementTable(ManagementBaseObject TargetInstanceObject)
        {
            try
            {
                string vid = TargetInstanceObject.GetPropertyValue("PNPDeviceID").ToString();
                string caption = TargetInstanceObject.GetPropertyValue("Caption").ToString();

                Match mVID = Regex.Match(vid, vidPattern, RegexOptions.IgnoreCase);
                Match mPID = Regex.Match(vid, pidPattern, RegexOptions.IgnoreCase);
                Match mPORT = Regex.Match(caption, portPattern, RegexOptions.IgnoreCase);

                //TODO add extended support for FTDI matching etc....

                Match mSer = Regex.Match(vid, "FTDIBUS\\\\VID_[0-9A-Z]*.PID_[0-9A-Z]*.([A-Z0-9]*)", RegexOptions.IgnoreCase);
                Match mSer2 = Regex.Match(vid, "USB\\\\VID_[0-9A-Z]*.PID_[0-9A-Z]*\\\\([A-Z0-9]*)", RegexOptions.IgnoreCase);

                lock (ports)
                {
                    foreach (sComPortModel port in ports)
                    {
                       
                        if (port.description == null)
                        {
                            if (mVID.Success && mPID.Success)
                            {
                                if ((port.pid == mPID.Groups[1].Value) && (port.vid == mVID.Groups[1].Value))
                                {
                                    if (mPORT.Success)
                                        port.port = mPORT.Groups[1].Value;

                                    port.description = TargetInstanceObject.GetPropertyValue("Caption").ToString();
                                    port.name = TargetInstanceObject.GetPropertyValue("DeviceID").ToString();
                                    port.wmipath = TargetInstanceObject.ToString();

                                    port.present = true;
                                    port.onConnect(port.name);
                                    return port;
                                }
                            }
                        }

                    }
                }

                sComPortModel c = new sComPortModel();

                c.name = TargetInstanceObject.GetPropertyValue("DeviceID").ToString();
                c.vid = TargetInstanceObject.GetPropertyValue("PNPDeviceID").ToString();
                c.description = TargetInstanceObject.GetPropertyValue("Caption").ToString();
                c.wmipath = TargetInstanceObject.ToString();

                if (mVID.Success)
                    c.vid = mVID.Groups[1].Value;
                if (mPID.Success)
                    c.pid = mPID.Groups[1].Value;
                if (mPORT.Success)
                    c.port = mPORT.Groups[1].Value;
                if (mSer.Success)
                    c.serial = mSer.Groups[1].Value;
                if (mSer2.Success)
                    c.serial = mSer2.Groups[1].Value;

                lock (ports)
                    ports.Add(c);
                c.onConnect(c.description);


                return c;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void removedeviceFromManagementTable(ManagementBaseObject TargetInstanceObject)
        {
            string vid = TargetInstanceObject.GetPropertyValue("PNPDeviceID").ToString();
            string caption = TargetInstanceObject.GetPropertyValue("Caption").ToString();

            Match mVID = Regex.Match(vid, vidPattern, RegexOptions.IgnoreCase);
            Match mPID = Regex.Match(vid, pidPattern, RegexOptions.IgnoreCase);
            Match mPORT = Regex.Match(caption, portPattern, RegexOptions.IgnoreCase);

            Match mSer = Regex.Match(vid, "FTDIBUS\\\\VID_[0-9A-Z]*.PID_[0-9A-Z]*.([A-Z0-9]*)", RegexOptions.IgnoreCase);
            Match mSer2 = Regex.Match(vid, "USB\\\\VID_[0-9A-Z]*.PID_[0-9A-Z]*.([A-Z0-9]*)", RegexOptions.IgnoreCase);


            foreach (sComPortModel port in ports)
            {
                if (port.description == caption)
                {

                    if (port.serial != null)
                    {
                        if (mSer.Success && port.serial == mSer.Groups[1].Value)
                        {
                            port.present = false;
                            port.onDisconnect();
                            break;
                        }

                        if (mSer2.Success && port.serial == mSer2.Groups[1].Value)
                        {
                            port.present = false;
                            port.onDisconnect();
                            break;
                        }

                    }
                    else
                    {
                        port.present = false;
                        port.onDisconnect();
                        break;
                    }
                }
            }
        }

        private void OnUsbConnected(object Sender, EventArrivedEventArgs Arguments)
        {
            PropertyData TargetInstanceData = Arguments.NewEvent.Properties["TargetInstance"];

            if (TargetInstanceData != null)
            {
                ManagementBaseObject TargetInstanceObject = (ManagementBaseObject)TargetInstanceData.Value;
                if (TargetInstanceObject != null)
                {
                    string deviceId = TargetInstanceObject.Properties["DeviceId"].Value.ToString();
                    var description = TargetInstanceObject.Properties["Description"].Value.ToString();

                    adddeviceToManagementTable(TargetInstanceObject);
                    updateDeviceStatus();

                }
            }

            DeviceListChanged?.Invoke(this,Arguments);
        }

        private void OnUsbDisconnected(object Sender, EventArrivedEventArgs Arguments)
        {
            PropertyData TargetInstanceData = Arguments.NewEvent.Properties["TargetInstance"];

            if (TargetInstanceData != null)
            {
                ManagementBaseObject TargetInstanceObject = (ManagementBaseObject)TargetInstanceData.Value;
                if (TargetInstanceObject != null)
                {
                    var description = TargetInstanceObject.Properties["Description"].Value.ToString();
                    string deviceId = TargetInstanceObject.Properties["DeviceId"].Value.ToString();
                    //_logger.LogInformation("USB Device departure {Description} {DeviceId}", description, deviceId);
                    removedeviceFromManagementTable(TargetInstanceObject);
                    updateDeviceStatus();
                }
            }

            DeviceListChanged?.Invoke(this, Arguments);
        }

        public sComPortModel requestSerialPortById(string vid, string pid, string ser, string portname = null)
        {

            foreach (sComPortModel port in ports)
            {

                if (portname != null)
                {
                    if (portname == port.port)
                    {
                        return port;
                    }
                }

                if (port.vid == vid && port.pid == pid)
                {
                    //Partial match
                    if (ser == null || ser == string.Empty)
                    {
                        return port;
                    }
                    else
                    {
                        if (port.serial != null)
                        {
                            // we also are matching the serial in this case for FTDI type devices
                            if (port.serial == ser)
                            {
                                return port;
                            }
                        }
                    }

                }

            }

            sComPortModel p = new sComPortModel();
            p.pid = pid;
            p.vid = vid;
            p.present = false;

            lock (ports)
                ports.Add(p);

            return p;

        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                //_optionsMonitor?.Dispose();
                //_optionsMonitor = null;
            }
        }

        #endregion

    }

}
