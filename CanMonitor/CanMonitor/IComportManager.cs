using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace PFMMeasurementService.Models.Devices.Buses
{
    public interface IComPortManagerInterface
    {
        sComPortModel requestSerialPortById(string vid, string pid, string ser, string portname);

        List<sComPortModel> GetPorts();

        event EventHandler<EventArgs> DeviceListChanged;

    }
}
