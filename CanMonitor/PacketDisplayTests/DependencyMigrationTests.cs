using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

internal static class DependencyMigrationTests
{
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    public static void Run(Type editorType, libCanopenSimple.libCanopenSimple can)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        string folder = Path.Combine(Path.GetTempPath(), "canmonitor-dependency-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            using (var editor = (Form)Activator.CreateInstance(editorType, new object[] { can }))
            {
                editorType.GetField("appdatafolder", flags).SetValue(editor, folder);
                ((CheckBox)editorType.GetField("checkBox_useronly", flags).GetValue(editor)).Checked = false;
                ((ComboBox)editorType.GetField("comboBoxtype", flags).GetValue(editor)).SelectedItem = "ALL";
                string edsFile = Path.Combine(folder, "device.eds");
                File.WriteAllText(edsFile, "[FileInfo]\nFileName=device.eds\n[DeviceInfo]\nProductName=Migration test\n"
                    + "[MandatoryObjects]\nSupportedObjects=1\n1=0x1000\n[1000]\nParameterName=Device type\nObjectType=0x7\n"
                    + "DataType=0x7\nAccessType=ro\nDefaultValue=42\nPDOMapping=0\n");
                Action<string> load = path =>
                {
                    editorType.GetMethod("loadeds", flags).Invoke(editor, new object[] { path });
                    Require((string)editorType.GetField("filename", flags).GetValue(editor) == path,
                        "Migrated editor did not load " + Path.GetFileName(path));
                    var rows = (ListView)editorType.GetField("listView1", flags).GetValue(editor);
                    Require(rows.Items.Count == 1 && rows.Items[0].SubItems[2].Text == "Device type",
                        "Migrated editor lost OD rows from " + Path.GetFileName(path));
                    object loaded = editorType.GetField("eds", flags).GetValue(editor);
                    var objects = (IDictionary)loaded.GetType().GetField("ods").GetValue(loaded);
                    object entry = objects[(ushort)0x1000];
                    Require(Convert.ToUInt32((string)entry.GetType().GetField("defaultvalue").GetValue(entry)) == 42,
                        "Migrated import changed the device value");
                };
                load(edsFile);
                object model = editorType.GetField("eds", flags).GetValue(editor);
                Type modelType = model.GetType();
                Type filetype = modelType.Assembly.GetType("libEDSsharp.InfoSection+Filetype", true);
                string dcf = Path.Combine(folder, "device.dcf");
                modelType.GetMethod("Savefile").Invoke(model, new object[] { dcf, Enum.Parse(filetype, "File_DCF") });
                load(dcf);
                foreach (bool legacy in new[] { false, true })
                {
                    Type converter = modelType.Assembly.GetType(legacy ? "libEDSsharp.CanOpenXDD" : "libEDSsharp.CanOpenXDD_1_1", true);
                    string xdd = Path.Combine(folder, legacy ? "v1.xdd" : "v11.xdd");
                    converter.GetMethod(legacy ? "writeXML" : "WriteXML").Invoke(Activator.CreateInstance(converter),
                        legacy ? new object[] { xdd, model } : new object[] { xdd, model, false, false });
                    load(xdd);
                }
                string xml = Path.Combine(folder, "original.xml");
                File.WriteAllText(xml, "<device><CANopenObjectList><CANopenObject index='1000' name='Device type' "
                    + "objectType='VAR' dataType='07' accessType='ro' PDOmapping='no' defaultValue='42'/></CANopenObjectList>"
                    + "<other><file fileName='original.xml' fileVersion='1' fileRevision='1'/><DeviceIdentity>"
                    + "<productName>Migration test</productName><productText><description>Test</description></productText>"
                    + "</DeviceIdentity><baudRate/><dummyUsage><dummy entry='Dummy0001=0'/><dummy entry='Dummy0002=0'/>"
                    + "<dummy entry='Dummy0003=0'/><dummy entry='Dummy0004=0'/><dummy entry='Dummy0005=0'/>"
                    + "<dummy entry='Dummy0006=0'/><dummy entry='Dummy0007=0'/></dummyUsage></other></device>");
                load(xml);
            }
            Console.WriteLine("PASS: migrated CanMonitor imports EDS, DCF, XDD 1.0/1.1 and original device XML with intact values");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
