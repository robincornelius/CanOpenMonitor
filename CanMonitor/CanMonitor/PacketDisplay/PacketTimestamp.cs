using System;
using System.Globalization;

namespace CanMonitor.PacketDisplay
{
    internal static class PacketTimestamp
    {
        // Keep the established saved-log format, but use it consistently across panes and cultures.
        internal const string Format = "MM/dd/yyyy HH:mm:ss.fff";

        internal static string ToDisplayString(DateTime timestamp)
        {
            return timestamp.ToString(Format, CultureInfo.InvariantCulture);
        }
    }
}
