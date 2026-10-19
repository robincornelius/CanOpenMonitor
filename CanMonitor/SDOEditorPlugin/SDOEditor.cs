using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libEDSsharp;
using System.IO;
using Xml2CSharp;
using libCanopenSimple;
using WeifenLuo.WinFormsUI.Docking;
using System.Security.Cryptography;

namespace SDOEditorPlugin
{
    public partial class SDOEditor : DockContent
    {

        EDSsharp eds;
        libCanopenSimple.libCanopenSimple lco;
        string filename = null;
        private string appdatafolder;
        private RecentDeviceFiles recentFiles;

        private List<string> _mru = new List<string>();

        Timer refreshtimer = new Timer();
        private readonly HashSet<SDO> pendingWrites = new HashSet<SDO>();
        private bool editingValue;
        private readonly ToolStripComboBox integerFormat = new ToolStripComboBox();
        private readonly Dictionary<ListViewItem, string> currentIntegerValues = new Dictionary<ListViewItem, string>();
        private readonly Dictionary<ListViewItem, string> dcfIntegerValues = new Dictionary<ListViewItem, string>();
        private readonly Dictionary<ListViewItem, string> wideIntegerValues = new Dictionary<ListViewItem, string>();
        private readonly HashSet<ListViewItem> currentValuesAvailable = new HashSet<ListViewItem>();
        private readonly Dictionary<uint, bool> integerOverrides = new Dictionary<uint, bool>();

        private uint RowAddress(ListViewItem row)
        {
            ODentry entry = ((sdocallbackhelper)row.Tag).od;
            return ((uint)entry.Index << 8) | entry.Subindex;
        }

        private void FormatIntegerRow(ListViewItem row)
        {
            DataType type = OdIntegerDisplay.TypeOf(((sdocallbackhelper)row.Tag).od);
            bool hex;
            if (!integerOverrides.TryGetValue(RowAddress(row), out hex))
            {
                hex = integerFormat.SelectedIndex == 1;
            }
            row.SubItems[4].Text = OdIntegerDisplay.Format(type, ((sdocallbackhelper)row.Tag).od.defaultvalue, hex);
            string value;
            if (currentIntegerValues.TryGetValue(row, out value))
            {
                row.SubItems[5].Text = OdIntegerDisplay.Format(type, value, hex);
            }
            if (dcfIntegerValues.TryGetValue(row, out value))
            {
                row.SubItems[6].Text = OdIntegerDisplay.Format(type, value, hex);
            }
            UpdateValueHighlights(row);
        }

        private void UpdateValueHighlights(ListViewItem row)
        {
            ODentry entry = ((sdocallbackhelper)row.Tag).od;
            if (entry == null)
            {
                return;
            }
            DataType type = OdIntegerDisplay.TypeOf(entry);
            string current = null;
            if (currentValuesAvailable.Contains(row))
            {
                if (!currentIntegerValues.TryGetValue(row, out current) && !wideIntegerValues.TryGetValue(row, out current))
                {
                    current = row.SubItems[5].Text;
                }
            }
            string dcf;
            if (!dcfIntegerValues.TryGetValue(row, out dcf) || dcf.Length == 0)
            {
                dcf = null;
            }
            string baseline = entry.defaultvalue.Length == 0 && type != DataType.VISIBLE_STRING ? null : entry.defaultvalue;
            bool? currentDefault = OdValueComparison.Different(type, current, baseline);
            bool? currentDcf = OdValueComparison.Different(type, current, dcf);
            bool? dcfDefault = OdValueComparison.Different(type, dcf, baseline);
            var differences = new List<string>();
            if (currentDefault == true)
            {
                differences.Add("Current ≠ Default");
            }
            if (currentDcf == true)
            {
                differences.Add("Current ≠ DCF");
            }
            if (dcfDefault == true)
            {
                differences.Add("DCF ≠ Default");
            }
            while (row.SubItems.Count < 8)
            {
                row.SubItems.Add("");
            }
            bool comparable = currentDefault.HasValue || currentDcf.HasValue || dcfDefault.HasValue;
            row.SubItems[7].Text = differences.Count != 0 ? string.Join("; ", differences)
                : comparable ? "Comparable values match" : "No comparable values";
            row.UseItemStyleForSubItems = false;
            row.BackColor = listView1.BackColor;
            foreach (ListViewItem.ListViewSubItem cell in row.SubItems)
            {
                cell.BackColor = listView1.BackColor;
                cell.ForeColor = listView1.ForeColor;
            }
            row.SubItems[5].BackColor = currentDefault == true || currentDcf == true ? Color.LightGoldenrodYellow : listView1.BackColor;
            row.SubItems[6].BackColor = dcfDefault == true ? Color.Lavender : listView1.BackColor;
            row.SubItems[7].BackColor = differences.Count != 0 ? Color.LightGoldenrodYellow : listView1.BackColor;
            Func<bool?, string> verdict = value => value.HasValue ? value.Value ? "different" : "equal" : "unavailable";
            row.ToolTipText = "Current / Default: " + verdict(currentDefault) + "\nCurrent / DCF: " + verdict(currentDcf)
                + "\nDCF / Default: " + verdict(dcfDefault);
        }

        private void InitializeIntegerDisplay()
        {
            listView1.Columns.Add("Differences", 410);
            listView1.ShowItemToolTips = true;
            integerFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            integerFormat.Items.AddRange(new object[] { "Decimal", "Hexadecimal" });
            integerFormat.SelectedIndex = 0;
            menuStrip1.Items.Add(new ToolStripLabel("Integer display:"));
            menuStrip1.Items.Add(integerFormat);
            integerFormat.SelectedIndexChanged += (sender, args) =>
            {
                // An explicit page-wide switch resets row overrides so every
                // integer row follows the newly selected format.
                integerOverrides.Clear();
                listView1.BeginUpdate();
                foreach (ListViewItem row in listView1.Items)
                {
                    FormatIntegerRow(row);
                }
                listView1.EndUpdate();
            };
            var context = new ContextMenuStrip();
            foreach (string label in new[] { "Use page default", "Show decimal", "Show hexadecimal" })
            {
                var item = new ToolStripMenuItem(label);
                item.Click += (sender, args) =>
                {
                    foreach (ListViewItem row in listView1.SelectedItems)
                    {
                        if (label == "Use page default")
                        {
                            integerOverrides.Remove(RowAddress(row));
                        }
                        else
                        {
                            integerOverrides[RowAddress(row)] = label == "Show hexadecimal";
                        }
                        FormatIntegerRow(row);
                    }
                };
                context.Items.Add(item);
            }
            context.Opening += (sender, args) => args.Cancel = listView1.SelectedItems.Count == 0
                || listView1.SelectedItems.Cast<ListViewItem>().Any(row => OdIntegerDisplay.Width(OdIntegerDisplay.TypeOf(((sdocallbackhelper)row.Tag).od)) == 0);
            listView1.MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButtons.Right)
                {
                    ListViewItem row = listView1.GetItemAt(args.X, args.Y);
                    if (row == null)
                    {
                        listView1.SelectedItems.Clear();
                    }
                    else if (!row.Selected)
                    {
                        listView1.SelectedItems.Clear();
                        row.Selected = true;
                    }
                }
            };
            listView1.ContextMenuStrip = context;
            Disposed += (sender, args) => context.Dispose();
        }


        public SDOEditor(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
            var editPdos = new ToolStripMenuItem("Edit PDOs...");
            editPdos.Click += EditPdos;
            menuStrip1.Items.Add(editPdos);
            InitializeIntegerDisplay();
            mnuRecentlyUsed.DropDownOpening += (sender, args) => LoadRecentFiles();
            refreshtimer.Tick += Refreshtimer_Tick;
            Disposed += (sender, args) => refreshtimer.Dispose();
        }

        // Completion callbacks run independently of the protocol listener.
        // Never synchronously marshal back to a UI that may enqueue another SDO.
        private void PostToUi(Action action)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }
            try
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    if (!IsDisposed)
                    {
                        action();
                    }
                }));
            }
            catch (InvalidOperationException) when (IsDisposed || !IsHandleCreated)
            {
                // The editor may close between the checks and BeginInvoke.
            }
        }

        bool oknextread = false;

        private void EditPdos(object sender, EventArgs args)
        {
            if (eds == null)
            {
                MessageBox.Show(this, "Load an EDS/XDD device dictionary first.");
                return;
            }
            if (!lco.isopen() || numericUpDown_node.Value == 0)
            {
                MessageBox.Show(this, "Open CAN and select a node before editing PDOs.");
                return;
            }
            if (applyingParameters || pendingWrites.Count != 0)
            {
                MessageBox.Show(this, "Wait for outstanding parameter writes to finish.");
                return;
            }
            editingValue = true;
            try
            {
                byte node = (byte)numericUpDown_node.Value;
                using (var editor = new PdoEditorForm(eds, node, new CanPdoEditorDevice(lco, node)))
                {
                    editor.ShowDialog(this);
                }
            }
            finally
            {
                editingValue = false;
            }
        }

        private void Refreshtimer_Tick(object sender, EventArgs e)
        {
            if (editingValue || applyingParameters || pendingWrites.Count != 0 || !lco.isopen())
            {
                return;
            }
            if (oknextread == false)
                return;


            if (button_read.Enabled == false)
                return;

            button_read.Enabled = false;
            foreach (ListViewItem lvi in listView1.Items)
            {

                UpdateValueHighlights(lvi);
                sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                if (help.od.objecttype != ObjectType.DOMAIN)
                {
                    SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.Index, (byte)help.od.Subindex, gotit);
                    help.sdo = sdo;
                    lvi.Tag = help;
                }

            }


        }

        private void loadeds(string filename)
        {
            if (filename == null || filename == "")
                return;

            string extension = Path.GetExtension(filename).ToLowerInvariant();
            if (!System.IO.File.Exists(filename) || (extension != ".eds" && extension != ".dcf" && extension != ".xdd" && extension != ".xml"))
            {
                MessageBox.Show(this, "The selected device file is missing or unsupported.", "Open device file");
                return;
            }
            bool isdcf = false;
            bool isemptydcf = false;

            button_writeDCF.Enabled = false;

            try
            {

                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".xdd":
                    {

                            CanOpenXDD_1_1 xdd = new CanOpenXDD_1_1();
                            eds = xdd.ReadXML(filename);
                          

                                if(eds==null)
                                {
                                    CanOpenXDD xdd2 = new CanOpenXDD();
                                    xdd2.readXML(filename);

                                    Bridge b = new Bridge();
                                    eds = xdd2.convert(xdd2.dev);
                                }
  
                            /*   
                            eds.xmlfilename = filename;
                            */
                        }

                    break;

                    case ".xml":
                    {
                        CanOpenXML coxml = new CanOpenXML();
                        coxml.readXML(filename);

                        Bridge b = new Bridge();

                        eds = b.convert(coxml.dev);
                        eds.xmlfilename = filename;
                    }

                    break;

                    case ".dcf":
                    {
                        isdcf = true;
                        if (listView1.Items.Count == 0)
                            isemptydcf = true;

                        eds = new EDSsharp();
                        eds.Loadfile(filename);
                        button_writeDCF.Enabled = true;

                    }
                    break;

                    case ".eds":
                    {
                        eds = new EDSsharp();
                        eds.Loadfile(filename);

                    }
                    break;


                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return;
            }

            if(eds!=null)
               textBox_edsfilename.Text = eds.di.ProductName;


            //if (eds.di.concreteNodeId >= numericUpDown_node.Minimum && eds.di.concreteNodeId <= numericUpDown_node.Maximum)
            //    numericUpDown_node.Value = eds.di.concreteNodeId;

            updatetable(isdcf,isemptydcf);



            this.ToolTipText = eds.di.ProductName;
            this.DockHandler.TabText = "OD: "+eds.di.ProductName;

            this.filename = filename;
            addtoMRU(filename);
        }

        private void updatetable(bool isdcf=false,bool isemptydcf=false)
        {

            listView1.BeginUpdate();
            if (!isdcf)
            {
                listView1.Items.Clear();
                currentIntegerValues.Clear();
                currentValuesAvailable.Clear();
                wideIntegerValues.Clear();
                dcfIntegerValues.Clear();
            }

            //           StorageLocation loc = StorageLocation


            foreach (ODentry tod in eds.ods.Values)
            {


                if (comboBoxtype.SelectedItem.ToString() != "ALL")
                {
                    if (comboBoxtype.SelectedItem.ToString() == "EEPROM" && (tod.prop.CO_storageGroup.ToUpper() != "EEPROM"))
                        continue;
                    if (comboBoxtype.SelectedItem.ToString() == "ROM" && (tod.prop.CO_storageGroup.ToUpper() != "ROM"))
                        continue;
                    if (comboBoxtype.SelectedItem.ToString() == "RAM" && (tod.prop.CO_storageGroup.ToUpper() != "RAM"))
                        continue;

                }


                if (tod.prop.CO_disabled == true)
                    continue;

                if (tod.Index < 0x2000 && checkBox_useronly.Checked == true)
                    continue;

                if (tod.objecttype == ObjectType.ARRAY || tod.objecttype == ObjectType.RECORD)
                {
                    foreach (ODentry subod in tod.subobjects.Values)
                    {
                        if (subod.Subindex == 0 && !((subod.Index >= 0x1600 && subod.Index <= 0x17FF)
                            || (subod.Index >= 0x1A00 && subod.Index <= 0x1BFF)))
                            continue;

                        addtolist(subod, isdcf, isemptydcf);
                    }

                    continue;

                }

                addtolist(tod, isdcf, isemptydcf);


            }

            listView1.EndUpdate();



        }

        private void button1_Click(object sender, EventArgs e)
        {


        }

        public struct sdocallbackhelper
        {
            public SDO sdo;
            public ODentry od;
        }


        void adddcfvalue(ODentry od)
        {

            foreach (ListViewItem lvi in listView1.Items)
            {
                sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                if ((help.od.Index == od.Index) && (help.od.Subindex == od.Subindex))
                {
                    dcfIntegerValues[lvi] = od.actualvalue;
                    lvi.SubItems[6].Text = od.actualvalue;
                    FormatIntegerRow(lvi);
                }
            }
        }

        void addtolist(ODentry od, bool dcf,bool isemptydcf)
        {


            if (!dcf || isemptydcf)
            {
                string[] items = new string[7];
                items[0] = string.Format("0x{0:x4}", od.Index);
                items[1] = string.Format("0x{0:x2}", od.Subindex);

                if (od.parent == null)
                    items[2] = od.parameter_name;
                else
                    items[2] = od.parent.parameter_name + " -- " + od.parameter_name;

                if (od.datatype == DataType.UNKNOWN && od.parent != null)
                {
                    items[3] = od.parent.datatype.ToString();
                }
                else
                {
                    items[3] = od.datatype.ToString();
                }


                items[4] = od.defaultvalue;



                items[5] = "";

                //  items[6] = od.actualvalue;


                ListViewItem lvi = new ListViewItem(items);



                // SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)od.index, (byte)od.subindex, gotit);

                sdocallbackhelper help = new sdocallbackhelper();
                help.sdo = null;
                help.od = od;
                lvi.Tag = help;

                listView1.Items.Add(lvi);
                FormatIntegerRow(lvi);
            }

            if (dcf)
            {
                adddcfvalue(od);
            }

        }

        void upsucc(SDO completed)
        {
            PostToUi(delegate
            {
                pendingWrites.Remove(completed);
                button_read.Enabled = !applyingParameters && pendingWrites.Count == 0 && lco.getSDOQueueSize() == 0;
                foreach (ListViewItem item in listView1.Items)
                {
                    sdocallbackhelper help = (sdocallbackhelper)item.Tag;
                    if (help.sdo != completed)
                    {
                        continue;
                    }
                    if (completed.state == SDO.SDO_STATE.SDO_ERROR)
                    {
                        currentIntegerValues.Remove(item);
                        currentValuesAvailable.Remove(item);
                        item.SubItems[5].Text = "Write failed: 0x" + completed.expitideddata.ToString("X8");
                        UpdateValueHighlights(item);
                        return;
                    }
                    if (help.od.objecttype != ObjectType.DOMAIN)
                    {
                        oknextread = true;
                        // Preserve the node that was written even if the user
                        // changes the editor's node selection before completion.
                        help.sdo = lco.SDOread(completed.node, help.od.Index, (byte)help.od.Subindex, gotit);
                        item.Tag = help;
                    }
                    break;
                }
            });
        }

        void gotit(SDO sdo)
        {
            try
            {

                PostToUi(delegate
                {

                    if (!applyingParameters && pendingWrites.Count == 0 && lco.getSDOQueueSize() == 0)
                        button_read.Enabled = true;

                    label_sdo_queue_size.Text = string.Format("SDO Queue Size: {0}", lco.getSDOQueueSize());

                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        sdocallbackhelper h = (sdocallbackhelper)lvi.Tag;
                        if (h.sdo == sdo)
                        {
                            if (sdo.state == SDO.SDO_STATE.SDO_ERROR)
                            {
                                currentIntegerValues.Remove(lvi);
                                currentValuesAvailable.Remove(lvi);
                                lvi.SubItems[5].Text = " **ERROR **";
                                UpdateValueHighlights(lvi);
                                return;
                            }

                            //if (sdo.exp == true)
                            {

                                DataType meh = h.od.datatype;
                                if (meh == DataType.UNKNOWN && h.od.parent != null)
                                    meh = h.od.parent.datatype;


                                //item 5 is the read value item 4 is the actual value

                                switch (meh)
                                {
                                    case DataType.REAL32:

                                        float myFloat = System.BitConverter.ToSingle(BitConverter.GetBytes(h.sdo.expitideddata), 0);
                                        lvi.SubItems[5].Text = myFloat.ToString();

                                        break;

                                    case DataType.REAL64:

                                        double myDouble = System.BitConverter.ToDouble(h.sdo.databuffer, 0);
                                        lvi.SubItems[5].Text = myDouble.ToString();

                                        break;

                                    case DataType.INTEGER8:
                                    case DataType.INTEGER16:
                                    case DataType.INTEGER32:
                                    case DataType.UNSIGNED8:
                                    case DataType.UNSIGNED16:
                                    case DataType.UNSIGNED32:
                                    {
                                        int width = OdIntegerDisplay.Width(meh);
                                        uint mask = width == 32 ? uint.MaxValue : (1U << width) - 1;
                                        string raw = "0x" + (h.sdo.expitideddata & mask).ToString("X" + width / 4);
                                        string canonical = OdIntegerDisplay.Format(meh, raw, false);
                                        currentIntegerValues[lvi] = canonical;
                                        FormatIntegerRow(lvi);
                                        break;
                                    }

                                    case DataType.VISIBLE_STRING:

                                        lvi.SubItems[5].Text = System.Text.Encoding.UTF8.GetString(h.sdo.databuffer);


                                        break;

                                    case DataType.OCTET_STRING:

                                        StringBuilder sb = new StringBuilder();

                                        foreach (byte b in h.sdo.databuffer)
                                        {
                                            sb.Append(string.Format("{0:x} ", b));
                                        }

                                        lvi.SubItems[5].Text = sb.ToString();


                                        break;


                                    case DataType.UNSIGNED64:
                                    {

                                        UInt64 data = (UInt64)System.BitConverter.ToUInt64(h.sdo.databuffer, 0);
                                        lvi.SubItems[5].Text = String.Format("{0:x}", data);
                                        wideIntegerValues[lvi] = data.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    break;

                                    case DataType.INTEGER64:
                                    {

                                        Int64 data = (Int64)System.BitConverter.ToInt64(h.sdo.databuffer, 0);
                                        lvi.SubItems[5].Text = String.Format("{0:x}", data);
                                        wideIntegerValues[lvi] = data.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    break;

                                    case DataType.BOOLEAN:
                                    {
                                        lvi.SubItems[5].Text = String.Format("{0}", h.sdo.expitideddata);
                                    }
                                    break;


                                    case DataType.DOMAIN:


                                        // Console.WriteLine(sdo.ToString());

                                        lvi.SubItems[5].Text = String.Format("Domain Len = {0}", h.sdo.databuffer.Length);

                                        

                                 //       StreamWriter file = new StreamWriter(string.Format("DOMAIN_{0}-{1}.txt",h.sdo.index,h.sdo.subindex));

                                 //       for(int p=0;p<h.sdo.databuffer.Length;p++)
                                 //       {
                                 //           file.WriteLine(h.sdo.databuffer[p].ToString());
                                 //       }
                                        

                                 //       file.Close();


                                        break;


                                    default:
                                        lvi.SubItems[5].Text = " **UNSUPPORTED **";
                                        break;


                                }

                            }
                            currentValuesAvailable.Add(lvi);
                            UpdateValueHighlights(lvi);
                            if (OdIntegerDisplay.Width(OdIntegerDisplay.TypeOf(h.od)) == 0)
                            {
                                h.od.actualvalue = lvi.SubItems[5].Text;
                            }
                            oknextread = true;
                            break;
                        }

                        oknextread = true;

                    }



                });
            }
            catch (Exception)
            {


            }

            return;


        }

        private static SdoParameterWrite CreateParameterWrite(sdocallbackhelper helper, string value)
        {
            DataType type = helper.od.datatype;
            if (type == DataType.UNKNOWN && helper.od.parent != null)
            {
                type = helper.od.parent.datatype;
            }
            byte[] data;
            switch (type)
            {
                case DataType.REAL32: data = BitConverter.GetBytes((float)new SingleConverter().ConvertFromString(value)); break;
                case DataType.REAL64: data = BitConverter.GetBytes((double)new DoubleConverter().ConvertFromString(value)); break;
                case DataType.INTEGER8: data = new[] { unchecked((byte)(sbyte)new SByteConverter().ConvertFromString(value)) }; break;
                case DataType.INTEGER16: data = BitConverter.GetBytes((short)new Int16Converter().ConvertFromString(value)); break;
                case DataType.INTEGER32: data = BitConverter.GetBytes((int)new Int32Converter().ConvertFromString(value)); break;
                case DataType.INTEGER64: data = BitConverter.GetBytes((long)new Int64Converter().ConvertFromString(value)); break;
                case DataType.BOOLEAN: case DataType.UNSIGNED8: data = new[] { (byte)new ByteConverter().ConvertFromString(value) }; break;
                case DataType.UNSIGNED16: data = BitConverter.GetBytes((ushort)new UInt16Converter().ConvertFromString(value)); break;
                case DataType.UNSIGNED32: data = BitConverter.GetBytes((uint)new UInt32Converter().ConvertFromString(value)); break;
                case DataType.UNSIGNED64: data = BitConverter.GetBytes((ulong)new UInt64Converter().ConvertFromString(value)); break;
                case DataType.VISIBLE_STRING: case DataType.DOMAIN: data = Encoding.ASCII.GetBytes(value); break;
                default: throw new NotSupportedException("Writing this OD data type is not supported.");
            }
            return new SdoParameterWrite(helper.od.Index, (byte)helper.od.Subindex, data);
        }

        private SDO dovalueupdate(sdocallbackhelper helper, string value)
        {
            SdoParameterWrite write = CreateParameterWrite(helper, value);
            if (PdoConfiguration.IsPdoParameter(write.Index))
            {
                // The form remains responsive while all handshake steps are
                // acknowledged. Never treat an intermediate write as success.
                ApplyParameterBatch(new[] { write });
                return null;
            }
            SDO transfer = lco.SDOwrite((byte)numericUpDown_node.Value, write.Index, write.Subindex, write.Data, upsucc);
            pendingWrites.Add(transfer);
            button_read.Enabled = false;
            return transfer;
        }

        private bool applyingParameters;

        private async void ApplyParameterBatch(SdoParameterWrite[] writes)
        {
            if (applyingParameters || pendingWrites.Count != 0)
            {
                MessageBox.Show(this, "Wait for the outstanding parameter writes to finish.");
                return;
            }
            applyingParameters = true;
            byte node = (byte)numericUpDown_node.Value;
            button_read.Enabled = false;
            button_writeDCF.Enabled = false;
            button_flush_queue.Enabled = false;
            numericUpDown_node.Enabled = false;
            label_sdo_queue_size.Text = "Applying parameters; waiting for SDO/NMT acknowledgements...";
            try
            {
                PdoConfigurationResult result = await PdoConfiguration.ApplyAsync(lco, node, writes);
                if (IsDisposed)
                {
                    return;
                }
                label_sdo_queue_size.Text = result.NmtStateChanged && !result.NmtStateConfirmed
                    ? "Parameters verified; Operational NMT sent. Heartbeat disabled: NMT state unconfirmed."
                    : "Parameters verified; configuration complete.";
                foreach (ListViewItem row in listView1.Items)
                {
                    sdocallbackhelper helper = (sdocallbackhelper)row.Tag;
                    if (writes.Any(write => write.Index == helper.od.Index && write.Subindex == helper.od.Subindex))
                    {
                        helper.sdo = lco.SDOread(node, helper.od.Index, (byte)helper.od.Subindex, gotit);
                        row.Tag = helper;
                    }
                }
            }
            catch (Exception error)
            {
                if (!IsDisposed)
                {
                    label_sdo_queue_size.Text = "Parameter configuration failed: " + error.Message;
                    MessageBox.Show(this, error.Message, "Configuration stopped");
                }
            }
            finally
            {
                applyingParameters = false;
                if (!IsDisposed)
                {
                    button_read.Enabled = pendingWrites.Count == 0;
                    button_writeDCF.Enabled = filename != null && Path.GetExtension(filename).Equals(".dcf", StringComparison.OrdinalIgnoreCase);
                    button_flush_queue.Enabled = true;
                    numericUpDown_node.Enabled = true;
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (applyingParameters)
            {
                return;
            }

            if (listView1.SelectedItems.Count == 0)
                return;

            if (!lco.isopen())
            {
                MessageBox.Show("CAN not open");
                return;
            }

            sdocallbackhelper h = (sdocallbackhelper)listView1.SelectedItems[0].Tag;
            if (h.od.AccessSDO() != AccessSDO.rw && h.od.AccessSDO() != AccessSDO.wo)
            {
                MessageBox.Show(this, "This OD entry is read-only.");
                return;
            }
            ValueEditor ve = new ValueEditor(h.od, listView1.SelectedItems[0].SubItems[5].Text);

            if (h.od.prop.CO_storageGroup == "ROM")
            {
               // MessageBox.Show("Should not edit ROM objects");

            }

            ve.UpdateValue += delegate (string s)
            {
                h.sdo = dovalueupdate(h, s);
                listView1.SelectedItems[0].Tag = h;
            };

            ve.SaveValue += delegate (string file, ODentry od)
            {
                if (od.objecttype != ObjectType.DOMAIN)
                {
                    lco.SDOread((byte)this.numericUpDown_node.Value, od.Index, (byte)od.Subindex, delegate (SDO s)
                    {
                        try
                        {
                            if (s.state == SDO.SDO_STATE.SDO_FINISHED)
                            {
                                System.IO.File.WriteAllBytes(file, s.databuffer);
                            }
                            else
                            {
                                throw new Exception("SDO transfer failed");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.ToString());
                        }
                    });
                }
            };


            // A modal editor still pumps WinForms timers. Pause refresh until
            // it closes, then keep it paused until outstanding writes complete.
            editingValue = true;
            try
            {
                ve.ShowDialog();
            }
            finally
            {
                editingValue = false;
                ve.Dispose();
            }
        }

        private void Ve_UpdateValue(string value)
        {

        }

        private void button_read_Click(object sender, EventArgs e)
        {
            if (applyingParameters || pendingWrites.Count != 0)
            {
                return;
            }

            if (!lco.isopen())
            {
                MessageBox.Show("CAN not open");
                return;
            }

            if(numericUpDown_node.Value == 0)
            {
                MessageBox.Show("You cannot read from Node 0, please select a node");
                return;
            }

            button_read.Enabled = false;
            foreach (ListViewItem lvi in listView1.Items)
            {

                UpdateValueHighlights(lvi);
                sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                if (help.od.objecttype != ObjectType.DOMAIN)
                {
                    oknextread = false;
                    SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.Index, (byte)help.od.Subindex, gotit);
                    help.sdo = sdo;
                    lvi.Tag = help;
                }

            }





        }

        private void loadEDSXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog odf = new OpenFileDialog();
            odf.Filter = "All supported files (*.eds;*.xml;*.xdd;*.dcf)|*.eds;*.xml;*.xdd;*.dcf|XML Electronic Data Sheet (*.xdd)|*.xdd|Legacy CanOpenNode project (*.xml)|*.xml|Electronic Datasheet (*.eds)|*.eds|Device Configuration File (*.dcf)|*.dcf";
            if (odf.ShowDialog() == DialogResult.OK)
            {

                button_writeDCF.Enabled = false;
                loadeds(odf.FileName);
            }

        }

        void OpenRecentFile(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var filepath = (string)menuItem.Tag;
            loadeds(filepath);

        }

        private void comboBoxtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadeds(filename);
        }

        private void EnsureRecentFiles()
        {
            if (recentFiles == null)
            {
                if (appdatafolder == null)
                {
                    appdatafolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanMonitor");
                }
                recentFiles = new RecentDeviceFiles(Path.Combine(appdatafolder, "SDOMRU.txt"));
            }
        }

        private void LoadRecentFiles()
        {
            EnsureRecentFiles();
            _mru = recentFiles.Load().ToList();
            populateMRU();
            ReportRecentFileError();
        }

        private void ReportRecentFileError()
        {
            if (recentFiles.Error != null)
            {
                label_sdo_queue_size.Text = "Recent-file history unavailable: " + recentFiles.Error;
            }
        }

        private void addtoMRU(string path)
        {
            EnsureRecentFiles();
            // Merge with disk immediately: other OD editor windows may have
            // opened files since this window last displayed its history.
            _mru = recentFiles.Add(path).ToList();
            populateMRU();
            ReportRecentFileError();
        }

        private void populateMRU()
        {

            foreach (ToolStripItem oldItem in mnuRecentlyUsed.DropDownItems.Cast<ToolStripItem>().ToArray())
            {
                oldItem.Dispose();
            }
            mnuRecentlyUsed.DropDownItems.Clear();
            if (_mru.Count == 0)
            {
                mnuRecentlyUsed.DropDownItems.Add(new ToolStripMenuItem("No recent device files") { Enabled = false });
            }

            foreach (var path in _mru)
            {
                var item = new ToolStripMenuItem(path) { AutoToolTip = false };
                item.Enabled = System.IO.File.Exists(path);
                item.ToolTipText = item.Enabled ? path : "File unavailable: " + path;
                item.Text = path.Replace("&", "&&");
                item.Tag = path;
                item.Click += OpenRecentFile;
                switch (Path.GetExtension(path).ToLowerInvariant())
                {
                    case ".xml":
                    case ".xdd":
                        item.Image = Properties.Resource1.GenericVSEditor_9905;
                        break;

                    case ".eds":
                    case ".dcf":
                        item.Image = Properties.Resource1.EventLog_5735;
                        break;



                }

                mnuRecentlyUsed.DropDownItems.Add(item);
            }
        }

        private void SDOEditor_Load(object sender, EventArgs e)
        {
            LoadRecentFiles();
        }

        private void saveDifferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog odf = new SaveFileDialog();
            odf.Filter = "(*.dcf)|*.dcf";
            if (odf.ShowDialog() == DialogResult.OK)
            {

                foreach (ListViewItem lvi in listView1.Items)
                {

                    string index = lvi.SubItems[0].Text;
                    string sub = lvi.SubItems[1].Text;
                    string name = lvi.SubItems[2].Text;


                    sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                    string defaultstring = help.od.defaultvalue;
                    string currentstring;
                    if (!currentIntegerValues.TryGetValue(lvi, out currentstring))
                    {
                        currentstring = help.od.actualvalue;
                    }

                    UInt16 key = Convert.ToUInt16(index, 16);
                    UInt16 subi = Convert.ToUInt16(sub, 16);

                    if (subi == 0)
                    {
                        eds.ods[key].actualvalue = currentstring;
                    }
                    else
                    {
                        ODentry subod = eds.ods[key].Getsubobject(subi);
                        if (subod != null)
                        {
                            subod.actualvalue = currentstring;
                        }
                    }

                    // file.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}",index,sub,name,defaultstring,currentstring));
                }

                eds.Savefile(odf.FileName, InfoSection.Filetype.File_DCF);

                //file.Close();
            }


        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)0x1010, (byte)0x01, (UInt32)0x65766173, null);
        }

        private void button_flush_queue_Click(object sender, EventArgs e)
        {
            lco.flushSDOqueue();
            button_read.Enabled = pendingWrites.Count == 0;

        }

        private static List<SdoParameterWrite> BuildDcfWrites(EDSsharp dcf)
        {
            if (dcf == null)
            {
                throw new InvalidOperationException("Load a DCF before applying it.");
            }
            var writes = new List<SdoParameterWrite>();
            // Use the complete DCF, not the currently filtered rows. Otherwise
            // a hidden mapping-count entry can leave a remap incomplete.
            foreach (ODentry parent in dcf.ods.Values)
            {
                IEnumerable<ODentry> entries = parent.subobjects.Count == 0
                    ? new[] { parent } : parent.subobjects.Values.AsEnumerable();
                foreach (ODentry entry in entries)
                {
                    if (entry.prop.CO_disabled || entry.actualvalue.Length == 0 || entry.AccessSDO() != AccessSDO.rw)
                    {
                        continue;
                    }
                    if (entry.Subindex == 0 && PdoConfiguration.IsPdoParameter(entry.Index)
                        && !((entry.Index >= 0x1600 && entry.Index <= 0x17FF) || (entry.Index >= 0x1A00 && entry.Index <= 0x1BFF)))
                    {
                        continue;
                    }
                    writes.Add(CreateParameterWrite(new sdocallbackhelper { od = entry }, entry.actualvalue));
                }
            }
            return writes;
        }

        private void button_writeDCF_Click(object sender, EventArgs e)
        {
            if (numericUpDown_node.Value == 0 || !lco.isopen())
            {
                MessageBox.Show(this, "Open CAN and select a valid node before applying a DCF.");
                return;
            }
            try
            {
                var writes = BuildDcfWrites(eds);
                if (writes.Count != 0)
                {
                    ApplyParameterBatch(writes.ToArray());
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(this, "DCF validation failed before download: " + error.Message);
            }
        }

        private void checkBox_autorefresh_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox_autorefresh.Checked==true)
            {
                refreshtimer.Interval = (int)numericUpDown_refreshtime.Value*1000;
                refreshtimer.Enabled = true;
            }
            else
            {
                refreshtimer.Enabled = false;
            }

        }

        private void button_addcustom_Click(object sender, EventArgs e)
        {
            List<ListViewItem> lvi2 = new List<ListViewItem>();

            foreach(ListViewItem lvi in listView1.SelectedItems)
            {
                lvi2.Add(lvi);
            }

            listView1.Items.Clear();

            foreach(ListViewItem lvi in lvi2)
            {
                listView1.Items.Add(lvi);
                FormatIntegerRow(lvi);
            }

        }

        ushort scanindex = 0x1000;
        byte scansub = 0x00;
        byte maxsub = 0;
        ODentry parentval = null;

        private void button_scan_Click(object sender, EventArgs e)
        {

            scanindex = 0x1018;
            scansub = 0x00;
            maxsub = 0;
            parentval = null;

            eds = new EDSsharp();


            lco.SDOread((byte)numericUpDown_node.Value, scanindex, scansub, scansomplete);

     
        }

        private void scansomplete(SDO obj)
        {
            PostToUi(() => ProcessScanComplete(obj));
        }

        private void ProcessScanComplete(SDO obj)
        {

            //Console.WriteLine(obj.ToString());

            if(obj.state== SDO.SDO_STATE.SDO_FINISHED)
            {

              

                    Console.WriteLine($"FOUND OBJECT {scanindex:x4}/{scansub:x2} length {obj.returnlen}");
                    ODentry val = new ODentry($"Object {scanindex:x4}", scanindex, 0);

                    switch (obj.returnlen)
                    {
                        case 32:
                            val.datatype = DataType.UNSIGNED32;
                            break;
                        case 24:
                            val.datatype = DataType.UNSIGNED24;
                            break;
                        case 16:
                            val.datatype = DataType.UNSIGNED16;
                            break;
                        case 8:
                            val.datatype = DataType.UNSIGNED8;


                            if (scansub==0 && obj.expitideddata != 0 )
                            {
                                maxsub = (byte) obj.expitideddata;
                                //it might have sub objects
                                parentval = val;
                               
                            }


                            break;
                        default:
                            break;
                    }

                    val.defaultvalue = obj.expitideddata.ToString();
                    
                    
                    if(scansub==0)
                        eds.ods.Add(scanindex, val);
                    else
                    {
                        parentval.objecttype = ObjectType.RECORD; //we don't know!!
                        parentval.addsubobject(scansub, val); 

                    }



            }

            if(obj.state==SDO.SDO_STATE.SDO_ERROR)
            {
                if (obj.expitideddata != 0x06020000)
                {
                    Console.WriteLine($"ERROR READING OBJECT {scanindex:x4}/{scansub:x2} error {obj.expitideddata:x8}");
                }
            
                  
            }



            if (scansub < maxsub)
            {
                scansub++;
            }
            else
            {
                maxsub = 0;
                scansub = 0;
                scanindex++;
            }

            if (scanindex < 0x9999)
                lco.SDOread((byte)numericUpDown_node.Value, scanindex, scansub, scansomplete);
            else
            {
                PostToUi(delegate
                {
                    updatetable();
                });

            }



        }
    }
}
