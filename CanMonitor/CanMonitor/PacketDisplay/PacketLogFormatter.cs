using System;
using System.Drawing;
using System.Globalization;
using libCanopenSimple;

namespace CanMonitor.PacketDisplay
{
    // All plugin decoding and string formatting runs on the display worker.
    // No control or ListViewItem is created here or on the capture thread.
    internal sealed class PacketLogFormatter
    {
        private readonly Func<ushort, Func<byte[], string>> getPdoDecoder;
        private readonly Func<ushort, ushort, byte, byte[], string> decodeSdo;
        private PacketLogRecord current;
        private PacketLogRow result;

        internal PacketLogFormatter(Func<ushort, Func<byte[], string>> getPdoDecoder = null,
            Func<ushort, ushort, byte, byte[], string> decodeSdo = null)
        {
            this.getPdoDecoder = getPdoDecoder;
            this.decodeSdo = decodeSdo;
        }

        internal PacketLogRow Format(PacketLogRecord record)
        {
            current = record;
            result = null;
            var packet = new canpacket { cob = record.CobId, len = (byte)record.Length, data = record.CopyPayload() };
            switch (record.Kind)
            {
                case PacketLogKind.Nmt: FormatNmt(packet, record.Timestamp); break;
                case PacketLogKind.Heartbeat: FormatHeartbeat(packet, record.Timestamp); break;
                case PacketLogKind.Sdo: FormatSdo(packet, record.Timestamp); break;
                case PacketLogKind.Pdo: FormatPdo(packet, record.Timestamp); break;
                case PacketLogKind.Emcy: FormatEmcy(packet, record.Timestamp); break;
            }
            return result;
        }

        private void Emit(string[] cells, Color foreground = default(Color), Color background = default(Color))
        {
            result = new PacketLogRow(current, cells, foreground, background);
        }

        private void FormatPdo(canpacket packet, DateTime timestamp)
        {
            Func<byte[], string> decoder = getPdoDecoder == null ? null : getPdoDecoder(packet.cob);
            string info = decoder == null ? string.Format("Len = {0}", packet.len) : decoder(packet.data);
            if (info == null)
            {
                return; // A registered decoder may intentionally suppress a row.
            }
            Emit(new[] { timestamp.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture), "PDO", packet.cob.ToString("x3"), "",
                current.PayloadText(), info });
        }

        private void FormatNmt(canpacket payload, DateTime dt)
        {

            string[] items = new string[6];
            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
            items[1] = "NMT";
            items[2] = string.Format("{0:x3}", payload.cob);
            items[3] = "";
            items[4] = current.PayloadText();

            string msg = "";

            if (payload.data.Length != 2)
            {
                throw new FormatException("An NMT command must contain two bytes.");
            }

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
            Emit(items, Color.Red);
        }

        private void FormatHeartbeat(canpacket payload, DateTime dt)
        {

            string[] items = new string[6];
            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
            items[1] = "NMTEC";
            items[2] = string.Format("{0:x3}", payload.cob);
            items[3] = string.Format("{0:x3}", payload.cob & 0x0FF);
            items[4] = current.PayloadText();

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
            Emit(items, Color.DarkGreen);
        }

        private void FormatSdo(canpacket payload, DateTime dt)
        {
            if (payload.len != 8)
            {
                throw new FormatException("An SDO frame must contain eight bytes.");
            }

            string[] items = new string[6];
            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
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

            items[4] = current.PayloadText();

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


                switch (SCS)
                {
                    case 0:
                        mode = "upload segment response";
                        sdoproto = string.Format("{0} {1} Valid bytes = {2} {3}", mode, t == 1 ? "TOG ON" : "TOG OFF", validsn, c == 0 ? "MORE" : "END");

                        if (c == 1)
                        {
                            //ipdo.endsdo(payload.cob, index, sub, null);
                            //END
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

                if (ErrorCodes.sdoerrormessages.ContainsKey(err))
                {

                    msg += " " + ErrorCodes.sdoerrormessages[err];

                }

            }
            else
            {
                if (decodeSdo != null)
                {
                    msg += " " + decodeSdo(payload.cob, index, sub, payload.data);
                }
            }

            items[5] = msg;
            Emit(items, Color.DarkBlue, (payload.data[0] & 0x80) != 0 ? Color.Orange : Color.Empty);
        }

        private void FormatEmcy(canpacket payload, DateTime dt)
        {
            string[] items = new string[6];

            items[0] = dt.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
            items[1] = "EMCY";
            items[2] = string.Format("{0:x3}", payload.cob);
            items[3] = string.Format("{0:x3}", payload.cob - 0x080);
            items[4] = current.PayloadText();
            //items[4] = "EMCY";

            UInt16 code = (UInt16)(payload.data[0] + (payload.data[1] << 8));
            byte bits = (byte)(payload.data[3]);
            UInt32 info = (UInt32)(payload.data[4] + (payload.data[5] << 8) + (payload.data[6] << 16) + (payload.data[7] << 24));

            if (ErrorCodes.errcode.ContainsKey(code))
            {

                string bitinfo;

                if (ErrorCodes.errbit.ContainsKey(bits))
                {
                    bitinfo = ErrorCodes.errbit[bits];
                }
                else
                {
                    bitinfo = string.Format("bits 0x{0:x2}", bits);
                }

                items[5] = string.Format("Error: {0} - {1} info 0x{2:x8}", ErrorCodes.errcode[code], bitinfo, info);
            }
            else
            {
                items[5] = string.Format("Error code 0x{0:x4} bits 0x{1:x2} info 0x{2:x8}", code, bits, info);
            }

            Emit(items, Color.White, code == 0 ? Color.Green : Color.Red);
        }

    }
}
