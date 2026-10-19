using System;
using System.Globalization;
using libEDSsharp;

namespace SDOEditorPlugin
{
    internal static class OdIntegerDisplay
    {
        internal static DataType TypeOf(ODentry entry)
        {
            return entry.datatype == DataType.UNKNOWN && entry.parent != null ? entry.parent.datatype : entry.datatype;
        }

        internal static int Width(DataType type)
        {
            switch (type)
            {
                case DataType.INTEGER8: case DataType.UNSIGNED8: return 8;
                case DataType.INTEGER16: case DataType.UNSIGNED16: return 16;
                case DataType.INTEGER32: case DataType.UNSIGNED32: return 32;
                default: return 0;
            }
        }

        internal static string Format(DataType type, string source, bool hex)
        {
            int width = Width(type);
            if (width == 0 || string.IsNullOrWhiteSpace(source))
            {
                return source;
            }
            string text = source.Trim();
            long number;
            bool signed = type == DataType.INTEGER8 || type == DataType.INTEGER16 || type == DataType.INTEGER32;
            long mask = (1L << width) - 1;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!long.TryParse(text.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out number)
                    || number < 0 || number > mask)
                {
                    return source;
                }
                // Hex represents the declared width's two's-complement bits.
                if (signed && number >= (1L << (width - 1)))
                {
                    number -= 1L << width;
                }
            }
            else if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
                || number < (signed ? -(1L << (width - 1)) : 0)
                || number > (signed ? (1L << (width - 1)) - 1 : mask))
            {
                // Preserve expressions such as $NODEID and error messages.
                return source;
            }
            return hex ? "0x" + (number & mask).ToString("X" + width / 4, CultureInfo.InvariantCulture)
                : number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
