using System;
using System.Drawing;

namespace CanMonitor.PacketDisplay
{
    internal enum PacketLogKind
    {
        Nmt,
        Heartbeat,
        Sdo,
        Pdo,
        Emcy
    }

    // Preserve typed fields independently of the rendered columns so display
    // filters match COB-ID/payload without parsing formatted text.
    internal sealed class PacketLogRecord
    {
        internal readonly PacketLogKind Kind;
        internal readonly ushort CobId;
        internal readonly DateTime Timestamp;
        internal readonly int Length;
        internal readonly ulong Payload;

        internal PacketLogRecord(PacketLogKind kind, ushort cobId, DateTime timestamp, byte[] data)
        {
            if (data == null || data.Length > 8)
            {
                throw new ArgumentException("A CAN payload must contain zero to eight bytes.", "data");
            }

            Kind = kind;
            CobId = cobId;
            Timestamp = timestamp;
            Length = data.Length;
            for (int index = 0; index < data.Length; index++)
            {
                Payload |= (ulong)data[index] << (8 * index);
            }
        }

        internal string TypeName
        {
            get
            {
                switch (Kind)
                {
                    case PacketLogKind.Nmt: return "NMT";
                    case PacketLogKind.Heartbeat: return "NMTEC";
                    case PacketLogKind.Sdo: return "SDO";
                    case PacketLogKind.Pdo: return "PDO";
                    case PacketLogKind.Emcy: return "EMCY";
                    default: return Kind.ToString();
                }
            }
        }

        internal string PayloadText()
        {
            const string hex = "0123456789ABCDEF";
            var text = new char[Length * 2];
            for (int index = 0; index < Length; index++)
            {
                byte value = (byte)(Payload >> (8 * index));
                text[index * 2] = hex[value >> 4];
                text[index * 2 + 1] = hex[value & 0xF];
            }
            return new string(text);
        }

        internal byte[] CopyPayload()
        {
            var bytes = new byte[Length];
            for (int index = 0; index < Length; index++)
            {
                bytes[index] = (byte)(Payload >> (8 * index));
            }

            return bytes;
        }
    }

    internal sealed class PacketLogRow
    {
        internal readonly PacketLogRecord Record;
        internal readonly string[] Cells;
        internal readonly Color Foreground;
        internal readonly Color Background;

        internal PacketLogRow(PacketLogRecord record, string[] cells,
            Color foreground = default(Color), Color background = default(Color))
        {
            Record = record;
            Cells = cells;
            Foreground = foreground;
            Background = background;
        }
    }

    internal sealed class PacketDisplayFilter
    {
        internal readonly bool ShowNmt;
        internal readonly bool ShowHeartbeat;
        internal readonly bool ShowBoot;
        internal readonly bool ShowSdo;
        internal readonly bool ShowPdo;
        internal readonly bool ShowEmcy;

        internal PacketDisplayFilter(bool nmt = true, bool heartbeat = true, bool boot = true,
            bool sdo = true, bool pdo = true, bool emcy = true)
        {
            ShowNmt = nmt;
            ShowHeartbeat = heartbeat;
            ShowBoot = boot;
            ShowSdo = sdo;
            ShowPdo = pdo;
            ShowEmcy = emcy;
        }

        // Protocol toggles compose with the compiled expression in the log's
        // retained-history projection. Other documents may still prefilter.
        internal bool Matches(PacketLogRecord record)
        {
            switch (record.Kind)
            {
                case PacketLogKind.Nmt: return ShowNmt;
                case PacketLogKind.Heartbeat: return ShowBoot && (ShowHeartbeat || (byte)record.Payload == 0);
                case PacketLogKind.Sdo: return ShowSdo;
                case PacketLogKind.Pdo: return ShowPdo;
                case PacketLogKind.Emcy: return ShowEmcy;
                default: return false;
            }
        }
    }
}
