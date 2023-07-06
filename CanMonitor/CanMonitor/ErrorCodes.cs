using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanMonitor
{
    public static class ErrorCodes
    {
        static public Dictionary<UInt16, string> errcode = new Dictionary<ushort, string>();
        static public Dictionary<UInt16, string> errbit = new Dictionary<ushort, string>();
        static public Dictionary<UInt32, string> sdoerrormessages = new Dictionary<UInt32, string>();

        static public void interror()
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



    }
}
