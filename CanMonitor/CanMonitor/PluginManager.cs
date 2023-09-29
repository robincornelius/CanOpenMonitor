using libCanopenSimple;
using Microsoft.CSharp;
using PDOInterface;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CanMonitor
{
    public class PluginManager
    {

        public Dictionary<string, object> plugins = new Dictionary<string, object>();

        List<string> loadedplugins = new List<string>();

        public IPDOParser ipdo;
        public Dictionary<UInt16, Func<byte[], string>> pdoprocessors = new Dictionary<ushort, Func<byte[], string>>();
        public MenuStrip menuStrip1;

        DockPanel dp;

        public PluginManager()
        {

            if (Program.lco == null)
                return;

            Program.lco.connectionevent += Lco_connectionevent;

        }

        public void SetDockPanel(DockPanel p)
        {
            dp = p;

        }

        private void Lco_connectionevent(object sender, EventArgs e)
        {
            //invoked when the underlying libcanopensimple opens or closes a driver conenction
            //send this message to all plugins that care

            foreach (KeyValuePair<string, object> kvp in plugins)
            {
                // if (o is IInterfaceService)
                {
                    IInterfaceService iis = (IInterfaceService)kvp.Value;
                    iis.DriverStateChange((ConnectionChangedEventArgs)e);
                }
            }

        }


        public void autoloadplugins()
        {

            var autoloadPath = Path.Combine(Program.assemblyfolder, "autoload.txt");
            if (File.Exists(autoloadPath))
            {
                string[] autoload = System.IO.File.ReadAllLines(autoloadPath);

                foreach (string plugin in autoload)
                {
                    loadplugin(plugin, false);
                }
            }

            if (Program.appdatafolder != Program.assemblyfolder)
            {
                autoloadPath = Path.Combine(Program.appdatafolder, "autoload.txt");
                if (File.Exists(autoloadPath))
                {
                    string[] autoload = System.IO.File.ReadAllLines(autoloadPath);

                    foreach (string plugin in autoload)
                    {
                        loadplugin(plugin, false);
                    }
                }
            }
        }

        public void loadplugin(String pfilename, bool addmru)
        {

            if (menuStrip1 == null)
                throw new Exception("Plugin manager setup incorrectly, need to pass the menustrip");

            if (plugins.ContainsKey(pfilename))
            {
                try
                {
                    IInterfaceService iis = (IInterfaceService)plugins[pfilename];
                    iis.deregisterplugin();

                }
                catch (Exception e)
                {
                    MessageBox.Show("Error deregistering plugin --- \n" + e.ToString());
                }

                plugins.Remove(pfilename);
                loadedplugins.Remove(pfilename);
            }

          

            if (loadedplugins.Contains(pfilename))
                return;

            try
            {

                string filename = pfilename;

                if (!File.Exists(filename))
                {

                    filename = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + pfilename;
                    if (!File.Exists(filename))
                        filename = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "privateplugins" + Path.DirectorySeparatorChar + pfilename;
                    if (!File.Exists(filename))
                        filename = Program.appdatafolder + Path.DirectorySeparatorChar + "plugins" + pfilename;
                    if (!File.Exists(filename))
                        filename = Program.appdatafolder + Path.DirectorySeparatorChar + "privateplugins" + pfilename;

                    if (!File.Exists(filename))
                    {
                        MessageBox.Show(string.Format("Could not find plugin {0}", pfilename));
                        return;
                    }
                }


                string ext = Path.GetExtension(filename);

                Assembly assembly;


                if (ext == ".cs")
                {
                    CSharpCodeProvider provider = new CSharpCodeProvider();
                    CompilerParameters parameters = new CompilerParameters();

                    // Reference to System.Drawing library
                    parameters.ReferencedAssemblies.Add("System.dll");
                    parameters.ReferencedAssemblies.Add("System.Core.dll");
                    parameters.ReferencedAssemblies.Add("System.Data.dll");
                    parameters.ReferencedAssemblies.Add("PDOInterface.dll");
                    parameters.ReferencedAssemblies.Add("libCanopenSimple.dll");
                    parameters.ReferencedAssemblies.Add("System.Windows.Forms");
                    parameters.ReferencedAssemblies.Add("System.Drawing");




                    // True - memory generation, false - external file generation
                    parameters.GenerateInMemory = true;
                    // True - exe file generation, false - dll file generation
                    parameters.GenerateExecutable = false;

                    string code = File.ReadAllText(filename);

                    CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                    if (results.Errors.HasErrors)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (CompilerError error in results.Errors)
                        {
                            sb.AppendLine(String.Format("{0}: Error ({1}): {2}", error.Line, error.ErrorNumber, error.ErrorText));
                        }

                        MessageBox.Show(sb.ToString());
                        return;

                    }

                    assembly = results.CompiledAssembly;

                }
                else
                {
                    assembly = Assembly.LoadFrom(filename);
                }

                Type[] types = assembly.GetExportedTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    object obj = null;

                    Type type = assembly.GetType(types[i].FullName);
                    if (type.GetInterface("PDOInterface.IInterfaceService") != null)
                    {
                        obj = Activator.CreateInstance(type);
                        if (obj != null)
                        {
                            plugins.Add(filename, obj);
                            IInterfaceService iis = (IInterfaceService)obj;
                            ipdo = (IPDOParser)obj;

                            Dictionary<UInt16, Func<byte[], string>> dictemp = new Dictionary<ushort, Func<byte[], string>>();

                            iis.setlco(Program.lco);

                            iis.preregisterPDOS(pdoprocessors);
                            ipdo.registerPDOS();


                            if(obj is IInterfaceService2)
                            {
                                IInterfaceService2 iis2 = (IInterfaceService2)obj;
                                iis2.setdockmanager(dp);
                            }
                           
                            //fixme
                            //textBox_info.AppendText(string.Format("SUCCESS loading plugin {0}\r\n", filename));
                        }

                    }






                    


            
                    if (type.GetInterface("PDOInterface.IInterfaceService") != null)
                    {
                        if (obj != null && obj is PDOInterface.IInterfaceService)
                        {
                            // do nothing use the object to save recreate
                        }
                        else
                        {
                            obj = Activator.CreateInstance(type);
                        }

                        if (obj != null)
                        {
                            IInterfaceService iss = (IInterfaceService)obj;

                            IVerb[] verbsroot = iss.GetVerbs("_root_");

                            if (verbsroot != null)
                            {
                                foreach (IVerb v in verbsroot)
                                {
                                    bool found = false;
                                    foreach (ToolStripItem ii in menuStrip1.Items)
                                    {
                                        if (ii.Text == v.Name)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (found == false)
                                    {

                                        menuStrip1.Items.Add(v.Name);
                                    }

                                }
                            }

                            foreach (ToolStripMenuItem ii in menuStrip1.Items)
                            {
                                IVerb[] verbs = iss.GetVerbs(ii.Text);

                                if (verbs != null)
                                {
                                    foreach (IVerb v in verbs)
                                    {
                                        if (v.Name == "---")
                                        {
                                            ToolStripSeparator item = new ToolStripSeparator();
                                            ii.DropDownItems.Add(item);
                                        }
                                        else
                                        {
                                            ToolStripMenuItem item = new ToolStripMenuItem(v.Name, null, v.Action);
                                            ii.DropDownItems.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                loadedplugins.Add(filename);

                if (addmru)
                {
                    //fixme
                    //addtoMRU(filename);
                }
            }
            catch (Exception ex)
            {
                //fixme
                //textBox_info.AppendText("Failed loading plugi \r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show("Error loading plugin :"+pfilename);
            }


        }


    }
}
