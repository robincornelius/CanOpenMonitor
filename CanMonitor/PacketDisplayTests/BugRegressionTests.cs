using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using libCanopenSimple;

internal static class BugRegressionTests
{
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    internal static void Run(string directory, string nativeFixtureDirectory)
    {
        string applicationDirectory = Path.GetFullPath(directory);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string path = Path.Combine(applicationDirectory, new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        Assembly application = Assembly.LoadFrom(Path.Combine(applicationDirectory, "CanMonitor.exe"));
        application.GetType("CanMonitor.Program", true).GetField("lco", BindingFlags.Static | BindingFlags.Public)
            .SetValue(null, new libCanopenSimple.libCanopenSimple());
        int failures = 0;
        foreach (Action test in new Action[] { () => TimestampCultures(application), () => DriverDiscovery(application, applicationDirectory, nativeFixtureDirectory), () => StartupSmoke(applicationDirectory) })
        {
            try
            {
                test();
            }
            catch (Exception error)
            {
                failures++;
                Console.Error.WriteLine("FAIL: " + error);
            }
        }
        Require(failures == 0, failures + " issue regression(s) failed.");
    }

    private static void StartupSmoke(string applicationDirectory)
    {
        string temporary = Path.Combine(Path.GetTempPath(), "canmonitor-launch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        string disconnected = Path.Combine(temporary, "without-drivers");
        Directory.CreateDirectory(disconnected);
        foreach (string file in Directory.GetFiles(applicationDirectory))
        {
            File.Copy(file, Path.Combine(disconnected, Path.GetFileName(file)));
        }
        string plugins = Path.Combine(applicationDirectory, "plugins");
        Directory.CreateDirectory(Path.Combine(disconnected, "plugins"));
        foreach (string file in Directory.GetFiles(plugins))
        {
            File.Copy(file, Path.Combine(disconnected, "plugins", Path.GetFileName(file)));
        }
        foreach (string deployment in new[] { applicationDirectory, disconnected })
        {
            using (Process process = Process.Start(new ProcessStartInfo(Path.Combine(deployment, "CanMonitor.exe"))
            {
                WorkingDirectory = temporary,
                UseShellExecute = false
            }))
            {
                try
                {
                    bool ready = SpinWait.SpinUntil(() =>
                    {
                        process.Refresh();
                        return process.HasExited || process.MainWindowTitle.StartsWith("CanOpen Monitor", StringComparison.Ordinal);
                    }, 20000);
                    Require(ready && !process.HasExited && process.MainWindowHandle != IntPtr.Zero
                        && process.MainWindowTitle.StartsWith("CanOpen Monitor", StringComparison.Ordinal),
                        "Actual application did not show its main window from " + deployment);
                    Thread.Sleep(500);
                    Require(!process.HasExited, "Actual application quit immediately after showing its window");
                    Require(process.CloseMainWindow() && process.WaitForExit(10000), "Actual application failed to close cleanly");
                }
                finally
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
            }
        }
        Console.WriteLine("PASS #2: actual CanMonitor.exe starts and closes from unrelated working directory, with and without native drivers");
    }

    private static void TimestampCultures(Assembly application)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            foreach (string culture in new[] { "en-GB", "en-US", "de-DE", "fr-FR" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                var can = new libCanopenSimple.libCanopenSimple();
                Type type = application.GetType("CanMonitor.NMTDocument", true);
                using (var form = (Form)Activator.CreateInstance(type, Instance, null, new object[] { can }, null))
                {
                    DateTime timestamp = new DateTime(2026, 10, 19, 13, 14, 15, 678);
                    var packet = new canpacket { cob = 0x719, len = 1, data = new byte[] { 5 } };
                    type.GetMethod("Lco_nmtecevent", Instance).Invoke(form, new object[] { packet, timestamp });
                    type.GetMethod("timer1_Tick", Instance).Invoke(form, new object[] { null, EventArgs.Empty });
                    var list = (ListView)type.GetField("listView_nmt", Instance).GetValue(form);
                    string actual = list.Items[0].SubItems[0].Text;
                    string expected = "10/19/2026 13:14:15.678";
                    Require(actual == expected, culture + ": actual NMT pane timestamp was " + actual + ", expected " + expected);
                    Type formatterType = application.GetType("CanMonitor.PacketDisplay.PacketLogFormatter", true);
                    object formatter = Activator.CreateInstance(formatterType, Instance, null, new object[] { null, null }, null);
                    Type recordType = application.GetType("CanMonitor.PacketDisplay.PacketLogRecord", true);
                    Type kindType = application.GetType("CanMonitor.PacketDisplay.PacketLogKind", true);
                    object record = Activator.CreateInstance(recordType, Instance, null,
                        new object[] { Enum.Parse(kindType, "Heartbeat"), (ushort)0x719, timestamp, packet.data }, null);
                    object row = formatterType.GetMethod("Format", Instance).Invoke(formatter, new[] { record });
                    var cells = (string[])row.GetType().GetField("Cells", Instance).GetValue(row);
                    Require(actual == cells[0], culture + ": NMT pane and packet log disagree");
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
        Console.WriteLine("PASS #3: actual NMT pane and packet log agree, including milliseconds, in four cultures");
    }

    private static void DriverDiscovery(Assembly application, string applicationDirectory, string nativeFixtureDirectory)
    {
        string temporary = Path.Combine(Path.GetTempPath(), "canmonitor-startup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        string previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = temporary;
            Type type = application.GetType("CanMonitor.DriverLoader", true);
            // Exercise the real production loader from an unrelated working directory.
            object real = Activator.CreateInstance(type);
            type.GetMethod("finddrivers").Invoke(real, null);
            var drivers = (List<string>)type.GetField("drivers", Instance).GetValue(real);
            Require(drivers.Count > 0 && drivers.All(Path.IsPathRooted), "Driver paths must be rooted at the application directory");
            type.GetMethod("finddrivers").Invoke(real, null);
            Require(drivers.Count == drivers.Distinct().Count(), "Repeated discovery duplicated drivers");

            Type managerType = application.GetType("PFMMeasurementService.Models.Devices.Buses.IComPortManagerInterface", true);
            Type factoryType = typeof(Func<>).MakeGenericType(managerType);
            Delegate unavailable = Expression.Lambda(factoryType,
                Expression.Throw(Expression.New(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
                    Expression.Constant("Simulated unavailable WMI service")), managerType)).Compile();
            ConstructorInfo constructor = type.GetConstructor(Instance, null, new[] { typeof(string), factoryType }, null);
            Require(constructor != null, "No injectable startup path to test unavailable optional WMI");
            object missing = constructor.Invoke(new object[] { temporary, unavailable });
            type.GetMethod("finddrivers").Invoke(missing, null);
            type.GetMethod("enumerateports").Invoke(missing, null);
            var ports = (System.Collections.IList)type.GetField("_driverport").GetValue(missing);
            Require(ports.Count == 0, "Missing drivers should leave a usable disconnected application");
            var errors = (IEnumerable<string>)type.GetProperty("DiscoveryErrors").GetValue(missing);
            Require(errors.Any(e => e.Contains("WMI")) && errors.Any(e => e.Contains("drivers")), "Missing drivers/WMI were not diagnosed");

            string native = Path.Combine(temporary, "drivers");
            Directory.CreateDirectory(native);
            File.Copy(Path.Combine(applicationDirectory, "drivers", "can_null_win32.dll"), Path.Combine(native, "can_null_win32.dll"));
            File.WriteAllText(Path.Combine(native, "unavailable_driver.dll"), "Not a loadable native DLL");
            Require(nativeFixtureDirectory != null, "Supply the native test build directory to test an absent imported runtime");
            File.Copy(Path.Combine(nativeFixtureDirectory, "startup_test_driver.dll"), Path.Combine(native, "startup_test_driver.dll"));
            // Do not copy startup_test_dependency.dll: this forces ERROR_MOD_NOT_FOUND
            // without altering any installed FTDI files or the physical adapter.
            type.GetMethod("finddrivers").Invoke(missing, null);
            type.GetMethod("enumerateports").Invoke(missing, null);
            ports = (System.Collections.IList)type.GetField("_driverport").GetValue(missing);
            Require(ports.Cast<object>().Any(p => (string)p.GetType().GetField("port").GetValue(p) == "null://null1"), "A bad driver prevented a healthy driver from enumerating");
            errors = (IEnumerable<string>)type.GetProperty("DiscoveryErrors").GetValue(missing);
            Require(errors.Any(e => e.Contains("unavailable_driver")), "Driver load failure was silently swallowed");
            Require(errors.Any(e => e.Contains("startup_test_driver") && e.Contains("126")),
                "Absent imported runtime was not isolated and diagnosed as loader error 126");
        }
        finally
        {
            Environment.CurrentDirectory = previous;
            // This directory can contain a loaded DLL until the test process exits.
        }
        Console.WriteLine("PASS #2: unrelated working directory, repeated discovery, missing drivers/WMI and failed DLL isolation");
    }
}
