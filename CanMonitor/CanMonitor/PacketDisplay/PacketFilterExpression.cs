using System;
using System.Globalization;

namespace CanMonitor.PacketDisplay
{
    // Compile once when Apply is pressed. Packet matching allocates no parser
    // objects and never executes user code or reflection.
    internal static class PacketFilterExpression
    {
        internal static Func<PacketLogRow, bool> Compile(string expression)
        {
            return new Parser(expression ?? "").Parse();
        }

        internal static string PacketCondition(PacketLogRecord record)
        {
            return "type == " + record.TypeName + " and cobid == 0x" + record.CobId.ToString("X3");
        }

        internal static string Combine(string current, string condition, bool exclude)
        {
            string next = exclude ? "not (" + condition + ")" : "(" + condition + ")";
            return string.IsNullOrWhiteSpace(current) ? next : "(" + current + ") and " + next;
        }

        private sealed class Parser
        {
            private readonly string text;
            private int position;
            private int tokenPosition;
            private string token;
            private bool quoted;
            private int depth;

            internal Parser(string text)
            {
                if (text.Length > 4096)
                {
                    throw new FormatException("Filter is limited to 4096 characters.");
                }
                this.text = text;
                Next();
            }

            internal Func<PacketLogRow, bool> Parse()
            {
                if (token == "")
                {
                    return row => true;
                }
                Func<PacketLogRow, bool> result = Or();
                if (token != "")
                {
                    throw Error("Unexpected '" + token + "'.");
                }
                return result;
            }

            private Func<PacketLogRow, bool> Or()
            {
                var left = And();
                while (Take("or") || Take("||"))
                {
                    var previous = left;
                    var right = And();
                    left = row => previous(row) || right(row);
                }
                return left;
            }

            private Func<PacketLogRow, bool> And()
            {
                var left = Unary();
                while (Take("and") || Take("&&"))
                {
                    var previous = left;
                    var right = Unary();
                    left = row => previous(row) && right(row);
                }
                return left;
            }

            private Func<PacketLogRow, bool> Unary()
            {
                if (++depth > 64)
                {
                    throw Error("Filter nesting is limited to 64 levels.");
                }
                try
                {
                    if (Take("not") || Take("!"))
                    {
                        var operand = Unary();
                        return row => !operand(row);
                    }
                    if (Take("("))
                    {
                        var inner = Or();
                        Expect(")");
                        return inner;
                    }
                    return Comparison();
                }
                finally
                {
                    depth--;
                }
            }

            private Func<PacketLogRow, bool> Comparison()
            {
                if (quoted)
                {
                    throw Error("Expected a field name without quotes.");
                }
                string field = token.ToLowerInvariant();
                Next();
                if (field == "type" || field == "info" || field == "payload")
                {
                    string operation = Operator(field != "type");
                    string value = token;
                    if (value == "" && !quoted)
                    {
                        throw Error("Expected a value.");
                    }
                    if (field != "type" && !quoted)
                    {
                        throw Error("Quote text values, for example info contains \"abort\".");
                    }
                    if (field == "type")
                    {
                        value = value.ToUpperInvariant();
                        if (value == "HEARTBEAT")
                        {
                            value = "NMTEC";
                        }
                        if (value != "PDO" && value != "SDO" && value != "NMT" && value != "NMTEC" && value != "EMCY")
                        {
                            throw Error("Type must be PDO, SDO, NMT, NMTEC or EMCY.");
                        }
                    }
                    Next();
                    return row => CompareText(field == "type" ? row.Record.TypeName :
                        field == "info" ? row.Cells[5] : row.Cells[4], value, operation);
                }

                Func<PacketLogRecord, long?> get;
                long maximum;
                switch (field)
                {
                    case "cobid": case "cob": get = record => record.CobId; maximum = 0x7FF; break;
                    // NMT targets a node in byte 1. Other supported CANopen
                    // protocols carry the node in the low seven COB-ID bits.
                    case "node": case "nodeid":
                        get = record => record.Kind == PacketLogKind.Nmt
                            ? (record.Length >= 2 && ((record.Payload >> 8) & 0xFF) <= 127
                                ? (long?)((record.Payload >> 8) & 0xFF) : null)
                            : record.CobId & 0x7F;
                        maximum = 127; break;
                    case "length": case "len": get = record => record.Length; maximum = 8; break;
                    case "sdo.index":
                        get = record => HasSdoIndex(record)
                            ? (long?)((record.Payload >> 8) & 0xFFFF) : null;
                        maximum = 0xFFFF; break;
                    case "sdo.subindex":
                        get = record => HasSdoIndex(record)
                            ? (long?)((record.Payload >> 24) & 0xFF) : null;
                        maximum = 255; break;
                    case "emcy.code":
                        get = record => record.Kind == PacketLogKind.Emcy && record.Length >= 2
                            ? (long?)(record.Payload & 0xFFFF) : null;
                        maximum = 0xFFFF; break;
                    default:
                        if (field != "data")
                        {
                            throw Error("Unknown field '" + field + "'.");
                        }
                        Expect("[");
                        int index = (int)Number(7);
                        Expect("]");
                        get = record => record.Length > index ? (long?)((record.Payload >> (8 * index)) & 0xFF) : null;
                        maximum = 255; break;
                }
                string op = Operator(false, true);
                long number = Number(maximum);
                // Missing protocol fields/bytes do not match, including !=.
                // Use 'not (...)' to include packets where a field is absent.
                return row =>
                {
                    long? actual = get(row.Record);
                    return actual.HasValue && CompareNumber(actual.Value, number, op);
                };
            }

            private string Operator(bool allowContains, bool numeric = false)
            {
                string result = token.ToLowerInvariant();
                if (quoted || !(result == "=" || result == "==" || result == "!=" ||
                    (numeric && (result == "<" || result == "<=" || result == ">" || result == ">=")) ||
                    (allowContains && result == "contains")))
                {
                    throw Error("Expected a comparison operator.");
                }
                Next();
                return result;
            }

            private long Number(long maximum)
            {
                long result;
                bool hexadecimal = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                if (quoted || !long.TryParse(hexadecimal ? token.Substring(2) : token,
                    hexadecimal ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
                    CultureInfo.InvariantCulture, out result) || result < 0 || result > maximum)
                {
                    throw Error("Expected a number from 0 to " + maximum + " (decimal or 0x hexadecimal).");
                }
                Next();
                return result;
            }

            private bool Take(string expected)
            {
                if (!quoted && string.Equals(token, expected, StringComparison.OrdinalIgnoreCase))
                {
                    Next();
                    return true;
                }
                return false;
            }

            private void Expect(string expected)
            {
                if (!Take(expected))
                {
                    throw Error("Expected '" + expected + "'.");
                }
            }

            private FormatException Error(string message)
            {
                return new FormatException(message + " At character " + (tokenPosition + 1) + ".");
            }

            private void Next()
            {
                while (position < text.Length && char.IsWhiteSpace(text[position]))
                {
                    position++;
                }
                tokenPosition = position;
                quoted = false;
                if (position == text.Length)
                {
                    token = "";
                    return;
                }
                char first = text[position++];
                if (first == '"')
                {
                    quoted = true;
                    var value = new System.Text.StringBuilder();
                    while (position < text.Length)
                    {
                        char next = text[position++];
                        if (next == '"')
                        {
                            token = value.ToString();
                            return;
                        }
                        if (next == '\\')
                        {
                            if (position == text.Length || (text[position] != '"' && text[position] != '\\'))
                            {
                                throw Error("Only escaped quotes and backslashes are supported.");
                            }
                            next = text[position++];
                        }
                        value.Append(next);
                    }
                    throw Error("Unterminated quoted value.");
                }
                if (char.IsLetterOrDigit(first) || first == '_')
                {
                    while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '.' || text[position] == '_'))
                    {
                        position++;
                    }
                    token = text.Substring(tokenPosition, position - tokenPosition);
                    return;
                }
                token = first.ToString();
                if (position < text.Length && (("=!<>".IndexOf(first) >= 0 && text[position] == '=') ||
                    (first == '&' && text[position] == '&') || (first == '|' && text[position] == '|')))
                {
                    token += text[position++];
                }
            }
        }

        private static bool CompareText(string actual, string expected, string operation)
        {
            bool equal = string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
            return operation == "contains" ? (actual ?? "").IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0
                : operation == "!=" ? !equal : equal;
        }

        private static bool HasSdoIndex(PacketLogRecord record)
        {
            if (record.Kind != PacketLogKind.Sdo || record.Length < 4)
            {
                return false;
            }
            int command = (int)((record.Payload >> 5) & 7);
            // Standard CANopen request/response command specifiers differ:
            // segment payload bytes are never object index/subindex fields.
            return command == 4 || (record.CobId >= 0x600 && record.CobId <= 0x67F
                ? command == 1 || command == 2
                : record.CobId >= 0x580 && record.CobId <= 0x5FF && (command == 2 || command == 3));
        }

        private static bool CompareNumber(long actual, long expected, string operation)
        {
            switch (operation)
            {
                case "!=": return actual != expected;
                case "<": return actual < expected;
                case "<=": return actual <= expected;
                case ">": return actual > expected;
                case ">=": return actual >= expected;
                default: return actual == expected;
            }
        }
    }
}
