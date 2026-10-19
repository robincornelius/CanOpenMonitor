using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using libCanopenSimple;
using libEDSsharp;

namespace SDOEditorPlugin
{
    internal sealed class PdoSelection
    {
        internal readonly ODentry Record;
        internal ushort Index
        {
            get
            {
                return Record.Index;
            }
        }
        internal bool Transmit
        {
            get
            {
                return Index >= 0x1800;
            }
        }
        internal ushort MappingIndex
        {
            get
            {
                return (ushort)(Index + 0x200);
            }
        }

        internal PdoSelection(ODentry record)
        {
            Record = record;
        }

        public override string ToString()
        {
            int number = Index - (Transmit ? 0x1800 : 0x1400) + 1;
            return string.Format("{0} PDO {1} — 0x{2:X4} — {3}", Transmit ? "TX" : "RX", number, Index, Record.parameter_name);
        }
    }

    internal sealed class PdoMappingOption
    {
        public uint Descriptor { get; private set; }
        public string Label { get; private set; }

        internal PdoMappingOption(uint descriptor, string label)
        {
            Descriptor = descriptor;
            Label = label;
        }
    }

    // Metadata and draft validation are independent of WinForms and CAN I/O.
    internal static class PdoEditorModel
    {
        internal static PdoSelection[] Selections(EDSsharp dictionary)
        {
            return dictionary.ods.Values.Where(record => !record.prop.CO_disabled &&
                ((record.Index >= 0x1400 && record.Index <= 0x15FF) || (record.Index >= 0x1800 && record.Index <= 0x19FF)))
                .OrderBy(record => record.Index).Select(record => new PdoSelection(record)).ToArray();
        }

        internal static IEnumerable<ODentry> Objects(EDSsharp dictionary)
        {
            return dictionary.ods.Values.Where(parent => !parent.prop.CO_disabled)
                .SelectMany(parent => parent.subobjects.Count == 0 ? new[] { parent } : parent.subobjects.Values.AsEnumerable())
                .Where(entry => !entry.prop.CO_disabled);
        }

        internal static List<PdoMappingOption> Options(EDSsharp dictionary, bool transmit)
        {
            return Objects(dictionary).Where(entry =>
            {
                AccessPDO access = entry.AccessPDO();
                return (access == AccessPDO.tr || access == (transmit ? AccessPDO.t : AccessPDO.r))
                    && entry.Sizeofdatatype() > 0 && entry.Sizeofdatatype() <= 64;
            }).OrderBy(entry => entry.Index).ThenBy(entry => entry.Subindex)
                .Select(entry => new PdoMappingOption(((uint)entry.Index << 16) | ((uint)entry.Subindex << 8) | (uint)entry.Sizeofdatatype(),
                    ObjectLabel(entry) + " — " + entry.Sizeofdatatype() + " bits")).ToList();
        }

        private static string ObjectLabel(ODentry entry)
        {
            return string.Format("0x{0:X4}:{1:X2} — {2}", entry.Index, entry.Subindex,
                entry.parent == null ? entry.parameter_name : entry.parent.parameter_name + " / " + entry.parameter_name);
        }

        internal static string Decode(EDSsharp dictionary, uint descriptor)
        {
            ushort index = (ushort)(descriptor >> 16);
            byte subindex = (byte)(descriptor >> 8);
            ODentry parent;
            ODentry target = null;
            if (dictionary.ods.TryGetValue(index, out parent))
            {
                target = parent.subobjects.Count == 0 ? (subindex == 0 ? parent : null) : parent.Getsubobject(subindex);
            }
            return (target == null ? string.Format("0x{0:X4}:{1:X2} — unknown in loaded dictionary", index, subindex) : ObjectLabel(target))
                + " — " + (byte)descriptor + " bits";
        }

        internal static string Format(byte subindex, byte[] data)
        {
            uint value = ReadUnsigned(data);
            return subindex == 1 ? "0x" + value.ToString("X8") : value.ToString(CultureInfo.InvariantCulture);
        }

        internal static uint ReadUnsigned(byte[] data)
        {
            if (data.Length != 1 && data.Length != 2 && data.Length != 4)
            {
                throw new ArgumentException("Expected an 8-, 16- or 32-bit parameter.");
            }
            var padded = new byte[4];
            Array.Copy(data, padded, data.Length);
            return BitConverter.ToUInt32(padded, 0);
        }

        internal static byte[] Parse(string text, int width)
        {
            string value = text.Trim();
            bool hex = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            uint number;
            if (!uint.TryParse(hex ? value.Substring(2) : value, hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
                CultureInfo.InvariantCulture, out number) || (width != 1 && width != 2 && width != 4)
                || (width < 4 && number >= (1U << (width * 8))))
            {
                throw new FormatException("Enter a decimal or 0x hexadecimal value fitting " + (width * 8) + " bits.");
            }
            return BitConverter.GetBytes(number).Take(width).ToArray();
        }

        internal static bool CanWrite(PdoSelection selection, byte subindex)
        {
            ODentry entry = selection.Record.Getsubobject(subindex);
            return (subindex == 1 || subindex == 2 || subindex == 3 || subindex == 5 || subindex == 6)
                && entry != null && entry.AccessSDO() == AccessSDO.rw;
        }

        internal static bool CanMap(EDSsharp dictionary, PdoSelection selection)
        {
            ODentry mapping;
            return dictionary.ods.TryGetValue(selection.MappingIndex, out mapping)
                && mapping.Getsubobject(0) != null && mapping.Getsubobject(0).AccessSDO() == AccessSDO.rw;
        }

        internal static SdoParameterWrite[] BuildWrites(PdoSelection selection, PdoConfigurationSnapshot snapshot,
            IDictionary<byte, string> drafts, IList<uint> mappings, IList<PdoMappingOption> options, bool mappingWritable)
        {
            var writes = new List<SdoParameterWrite>();
            foreach (var draft in drafts)
            {
                byte[] original = snapshot.Communication[draft.Key];
                byte[] data;
                try
                {
                    data = Parse(draft.Value, original.Length);
                }
                catch (FormatException error)
                {
                    throw new FormatException(string.Format("{0:X4}:{1:X2}: {2}", selection.Index, draft.Key, error.Message));
                }
                if (!data.SequenceEqual(original))
                {
                    if (!CanWrite(selection, draft.Key))
                    {
                        throw new InvalidOperationException("A read-only parameter was changed.");
                    }
                    writes.Add(new SdoParameterWrite(selection.Index, draft.Key, data));
                }
            }
            if (!snapshot.Mappings.SequenceEqual(mappings))
            {
                if (!mappingWritable || snapshot.MappingError != null)
                {
                    throw new InvalidOperationException("This PDO mapping is read-only or unavailable.");
                }
                int bits = 0;
                for (int position = 0; position < mappings.Count; position++)
                {
                    uint descriptor = mappings[position];
                    int length = (byte)descriptor;
                    if (length == 0 || (bits += length) > 64)
                    {
                        throw new InvalidOperationException("The PDO mapping must fit in 64 bits.");
                    }
                    // Preserve existing unknown/fixed entries, but do not offer
                    // them as new mappings unless the dictionary permits it.
                    if (!snapshot.Mappings.Contains(descriptor) && !options.Any(option => option.Descriptor == descriptor))
                    {
                        throw new InvalidOperationException("A new mapping is not supported by the dictionary for this PDO direction.");
                    }
                    writes.Add(new SdoParameterWrite(selection.MappingIndex, (byte)(position + 1), BitConverter.GetBytes(descriptor)));
                }
                writes.Add(new SdoParameterWrite(selection.MappingIndex, 0, new[] { checked((byte)mappings.Count) }));
            }
            return writes.ToArray();
        }
    }
}
