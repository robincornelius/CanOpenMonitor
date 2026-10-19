using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace CanMonitor.PacketDisplay
{
    internal static class PacketLogFile
    {
        internal static void Save(string filename, PacketLogRow[] rows, string[] columns,
            string defaultForeground, string defaultBackground)
        {
            string folder = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }
            using (XmlWriter writer = XmlWriter.Create(filename,
                new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) }))
            {
                writer.WriteStartElement("CanOpenMonitor");
                foreach (PacketLogRow row in rows)
                {
                    writer.WriteStartElement("Packet");
                    writer.WriteAttributeString("backcol", row.Background.IsEmpty ? defaultBackground : row.Background.Name);
                    writer.WriteAttributeString("forcol", row.Foreground.IsEmpty ? defaultForeground : row.Foreground.Name);
                    for (int index = 0; index < columns.Length; index++)
                    {
                        writer.WriteElementString(columns[index], index < row.Cells.Length ? row.Cells[index] ?? "" : "");
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        internal static void LoadXml(string filename, Action<PacketLogRecord> accept, CancellationToken cancellation)
        {
            using (XmlReader reader = XmlReader.Create(filename, new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                reader.MoveToContent();
                bool empty = reader.IsEmptyElement;
                reader.ReadStartElement("CanOpenMonitor");
                if (empty)
                {
                    return;
                }
                while (reader.MoveToContent() == XmlNodeType.Element)
                {
                    cancellation.ThrowIfCancellationRequested();
                    XElement packet = (XElement)XNode.ReadFrom(reader);
                    if (packet.Name != "Packet")
                    {
                        continue;
                    }
                    PacketLogKind kind;
                    if (!TryKind((string)packet.Element("Type"), out kind))
                    {
                        throw new FormatException("Unknown packet type in saved log.");
                    }
                    ushort cob = ushort.Parse((string)packet.Element("COB"), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    DateTime timestamp = DateTime.ParseExact((string)packet.Element("Timestamp"),
                        "MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    accept(new PacketLogRecord(kind, cob, timestamp, Payload((string)packet.Element("Payload"))));
                }
            }
        }

        internal static void LoadCsv(string filename, Action<PacketLogRecord> accept, CancellationToken cancellation)
        {
            int lineNumber = 0;
            foreach (string line in File.ReadLines(filename))
            {
                cancellation.ThrowIfCancellationRequested();
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                try
                {
                    string[] cells = line.Split(',');
                    ushort cob = ushort.Parse(cells[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    int length = int.Parse(cells[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    byte[] data = Payload(cells[3]);
                    if (data.Length != length)
                    {
                        throw new FormatException("DLC and payload do not agree.");
                    }
                    PacketLogKind kind = cob == 0 ? PacketLogKind.Nmt
                        : cob >= 0x700 ? PacketLogKind.Heartbeat
                        : cob >= 0x580 ? PacketLogKind.Sdo
                        : cob >= 0x180 ? PacketLogKind.Pdo : PacketLogKind.Emcy;
                    accept(new PacketLogRecord(kind, cob, DateTime.Parse(cells[0], CultureInfo.InvariantCulture), data));
                }
                catch (Exception error)
                {
                    throw new FormatException("Invalid packet on CSV line " + lineNumber + ".", error);
                }
            }
        }

        private static byte[] Payload(string value)
        {
            if (value == null || value.Length % 2 != 0 || value.Length > 16)
            {
                throw new FormatException("Invalid CAN payload in saved log.");
            }
            var data = new byte[value.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                data[index] = byte.Parse(value.Substring(index * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return data;
        }

        private static bool TryKind(string value, out PacketLogKind kind)
        {
            switch (value)
            {
                case "NMT": kind = PacketLogKind.Nmt; return true;
                case "NMTEC": kind = PacketLogKind.Heartbeat; return true;
                case "SDO": kind = PacketLogKind.Sdo; return true;
                case "PDO": kind = PacketLogKind.Pdo; return true;
                case "EMCY": kind = PacketLogKind.Emcy; return true;
                default: kind = default(PacketLogKind); return false;
            }
        }
    }
}
