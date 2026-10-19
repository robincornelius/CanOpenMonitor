using System;
using System.Globalization;
using System.Linq;
using libEDSsharp;

namespace SDOEditorPlugin
{
    internal static class OdValueComparison
    {
        // Null means the pair cannot be compared, not that the values match.
        internal static bool? Different(DataType type, string left, string right)
        {
            if (left == null || right == null)
            {
                return null;
            }
            if (type == DataType.VISIBLE_STRING)
            {
                return !string.Equals(left, right, StringComparison.Ordinal);
            }
            if (type == DataType.OCTET_STRING)
            {
                byte[] a, b;
                return TryOctets(left, out a) && TryOctets(right, out b) ? (bool?)!a.SequenceEqual(b) : null;
            }
            if (OdIntegerDisplay.Width(type) != 0)
            {
                decimal a, b;
                if (decimal.TryParse(OdIntegerDisplay.Format(type, left, false), NumberStyles.Integer, CultureInfo.InvariantCulture, out a)
                    && decimal.TryParse(OdIntegerDisplay.Format(type, right, false), NumberStyles.Integer, CultureInfo.InvariantCulture, out b))
                {
                    int width = OdIntegerDisplay.Width(type);
                    bool signed = type == DataType.INTEGER8 || type == DataType.INTEGER16 || type == DataType.INTEGER32;
                    decimal min = signed ? -(1L << (width - 1)) : 0;
                    decimal max = signed ? (1L << (width - 1)) - 1 : (1L << width) - 1;
                    return a >= min && a <= max && b >= min && b <= max ? (bool?)(a != b) : null;
                }
                return null;
            }
            if (type == DataType.INTEGER64 || type == DataType.UNSIGNED64 || type == DataType.BOOLEAN)
            {
                decimal a, b;
                return TryWideInteger(type, left, out a) && TryWideInteger(type, right, out b) ? (bool?)(a != b) : null;
            }
            if (type == DataType.REAL32 || type == DataType.REAL64)
            {
                double a, b;
                if (TryReal(left, out a) && TryReal(right, out b))
                {
                    return type == DataType.REAL32 ? !((float)a).Equals((float)b) : !a.Equals(b);
                }
            }
            return null;
        }

        private static bool TryReal(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryOctets(string text, out byte[] value)
        {
            string[] tokens = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            value = new byte[tokens.Length];
            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? tokens[index].Substring(2) : tokens[index];
                if (!byte.TryParse(token, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value[index]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryWideInteger(DataType type, string text, out decimal value)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                ulong bits;
                if (ulong.TryParse(text.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out bits))
                {
                    value = type == DataType.INTEGER64 ? (decimal)unchecked((long)bits) : bits;
                    return type != DataType.BOOLEAN || bits <= 1;
                }
                value = 0;
                return false;
            }
            if (!decimal.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }
            return type == DataType.INTEGER64 ? value >= long.MinValue && value <= long.MaxValue
                : type == DataType.UNSIGNED64 ? value >= 0 && value <= ulong.MaxValue : value == 0 || value == 1;
        }
    }
}
