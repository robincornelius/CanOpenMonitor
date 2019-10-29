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

namespace EmergencySimulator
{
    public partial class EmergencyFrm : Form
    {
        libCanopenSimple.libCanopenSimple _lco;

        Dictionary<UInt16, string> errcode = new Dictionary<ushort, string>();
        Dictionary<UInt16, string> errbit = new Dictionary<ushort, string>();

        Dictionary<string,UInt16> errcoder = new Dictionary<string,ushort>();
        Dictionary<string,UInt16> errbitr = new Dictionary<string,ushort>();

        byte[] emcymsg = new byte[8];
        UInt16 node;

        public EmergencyFrm(libCanopenSimple.libCanopenSimple lco)
        {
            _lco = lco;
           
            InitializeComponent();
            init();

           
        }

        public void init()
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
            errbit.Add(0x09, "(unused 09)");
            errbit.Add(0x0A, "(unused 0a)");
            errbit.Add(0x0B, "(unused 0b)");
            errbit.Add(0x0C, "(unused 0c)");
            errbit.Add(0x0D, "(unused 0d)");
            errbit.Add(0x0E, "(unused 0e)");
            errbit.Add(0x0F, "(unused 0f)");

            errbit.Add(0x10, "(unused 10)");
            errbit.Add(0x11, "(unused 11)");
            errbit.Add(0x12, "CAN transmit bus is off");
            errbit.Add(0x13, "CAN module receive buffer has overflowed");
            errbit.Add(0x14, "CAN transmit buffer has overflowed");
            errbit.Add(0x15, "TPDO is outside SYNC window");
            errbit.Add(0x16, "(unused 16)");
            errbit.Add(0x17, "(unused 17)");
            errbit.Add(0x18, "SYNC message timeout");
            errbit.Add(0x19, "Unexpected SYNC data length");
            errbit.Add(0x1A, "Error with PDO mapping");
            errbit.Add(0x1B, "Heartbeat consumer timeout");
            errbit.Add(0x1C, "Heartbeat consumer detected remote node reset");
            errbit.Add(0x1D, "(unused 1d)");
            errbit.Add(0x1E, "(unused 1e)");
            errbit.Add(0x1F, "(unused 1f)");

            errbit.Add(0x20, "Emergency message wasn't sent");
            errbit.Add(0x21, "(unused 21)");
            errbit.Add(0x22, "Microcontroller has just started");
            errbit.Add(0x23, "(unused 23)");
            errbit.Add(0x24, "(unused 24)");
            errbit.Add(0x25, "(unused 25)");
            errbit.Add(0x26, "(unused 26)");
            errbit.Add(0x27, "(unused 27)");

            errbit.Add(0x28, "Wrong parameters to CO_errorReport() function");
            errbit.Add(0x29, "Timer task has overflowed");
            errbit.Add(0x2A, "Unable to allocate memory for objects");
            errbit.Add(0x2B, "test usage");
            errbit.Add(0x2C, "Software error");
            errbit.Add(0x2D, "Object dictionary does not match the software");
            errbit.Add(0x2E, "Error in calculation of device parameters");
            errbit.Add(0x2F, "Error with access to non volatile device memory");

            foreach(KeyValuePair<ushort,string> kvp in errcode)
            {
                comboBox_errorcode.Items.Add(kvp.Value);
                errcoder.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<ushort, string> kvp in errbit)
            {
                comboBox_errorbits.Items.Add(kvp.Value);
                errbitr.Add(kvp.Value, kvp.Key);
            }


        }

        private void comboBox_errorbits_SelectedIndexChanged(object sender, EventArgs e)
        {

            string val = comboBox_errorbits.SelectedItem.ToString();

            uint code = errbitr[val];

            numericUpDown_errorbits.Value = code;
            


        }

        private void comboBox_errorcode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string val = comboBox_errorcode.SelectedItem.ToString();

            uint code = errcoder[val];

            numericUpDown_errorcode.Value = code;

        }

        private void numericUpDown_node_ValueChanged(object sender, EventArgs e)
        {
            node =(UInt16) (0x80 + numericUpDown_node.Value);
            updatemsgstring();
        }

        private void updatemsgstring()
        {
            textBox_msg.Text = string.Format("0x{0:x2} {1:x2}{2:x2} {3:x2}{4:x2} {5:x2}{6:x2}{7:x2}{8:x2}", node,emcymsg[0], emcymsg[1], emcymsg[2], emcymsg[3], emcymsg[4], emcymsg[5], emcymsg[6], emcymsg[7]);

        }

        private void numericUpDown_errorbits_ValueChanged(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numericUpDown_errorbits.Value;
            emcymsg[3] = (byte)val;
            emcymsg[2] = 0;

            if(errbit.ContainsKey(val))
            {
                comboBox_errorbits.SelectedItem = errbit[val];
            }

            updatemsgstring();
        }

        private void numericUpDown_errorcode_ValueChanged(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numericUpDown_errorcode.Value;

           emcymsg[0] = (byte)val;
           emcymsg[1] = (byte)(val>>8);

            if (errcode.ContainsKey(val))
            {
                comboBox_errorcode.SelectedItem = errcode[val];
            }



            updatemsgstring();
        }

        private void numericUpDown_additional_ValueChanged(object sender, EventArgs e)
        {

            UInt64 val = (UInt64) numericUpDown_additional.Value;
            emcymsg[4] = (byte)(val >> 0);
            emcymsg[5] = (byte)(val >> 8);
            emcymsg[6] = (byte)(val >> 16);
            emcymsg[7] = (byte)(val >> 24);

            updatemsgstring();
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            canpacket p = new canpacket();
            p.cob = node;
            p.data = emcymsg;
            p.len = 8;

            _lco.SendPacket(p);

        }
    }
}