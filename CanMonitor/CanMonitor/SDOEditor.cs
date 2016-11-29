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

namespace CanMonitor
{
    public partial class SDOEditor : Form
    {

        EDSsharp eds;
        libCanopen lco;
        string filename = null;
        private string appdatafolder;

        private List<string> _mru = new List<string>();

        public SDOEditor(libCanopen lco)
        {
            this.lco = lco;
            InitializeComponent();
        }


        private void loadeds(string filename)
        {
            if (filename == null || filename == "")
                return;

            switch (Path.GetExtension(filename).ToLower())
            {
                case ".xml":
                    {
                        CanOpenXML coxml = new CanOpenXML();
                        coxml.readXML(filename);

                        Bridge b = new Bridge();

                        eds = b.convert(coxml.dev);
                        eds.filename = filename;

                    }

                    break;

                case ".eds":
                    {
                        eds = new EDSsharp();
                        eds.loadfile(filename);
                    }
                    break;


            }

            textBox_edsfilename.Text = eds.di.ProductName;

            numericUpDown_node.Value = eds.di.concreteNodeId;

            listView1.BeginUpdate();
            listView1.Items.Clear();

            StorageLocation loc = StorageLocation.EEPROM;


            foreach (ODentry tod in eds.ods.Values)
            {

                if (comboBoxtype.SelectedItem.ToString() != "ALL")
                { 
                    if (comboBoxtype.SelectedItem.ToString()=="EEPROM" && (tod.location != StorageLocation.EEPROM))
                        continue;
                    if (comboBoxtype.SelectedItem.ToString() == "ROM" && (tod.location != StorageLocation.ROM))
                        continue;
                    if (comboBoxtype.SelectedItem.ToString() == "RAM" && (tod.location != StorageLocation.RAM))
                        continue;
                             
                }
              
                if (tod.objecttype == ObjectType.ARRAY || tod.objecttype == ObjectType.REC)
                {
                    foreach (ODentry subod in tod.subobjects.Values)
                    {
                        if (subod.subindex == 0)
                            continue;

                        addtolist(subod);
                    }

                    continue;

                }

                addtolist(tod);


            }

            listView1.EndUpdate();

            this.filename = filename;
            addtoMRU(filename);
        }

        private void button1_Click(object sender, EventArgs e)
        {

          
        }

        public struct sdocallbackhelper
        {
            public SDO sdo;
            public ODentry od;
        }

        void addtolist(ODentry od)
        {

            string[] items = new string[6];
            items[0] = string.Format("0x{0:x4}", od.index);
            items[1] = string.Format("0x{0:x2}", od.subindex);

            if(od.parent==null)
                items[2] = od.parameter_name;
            else
                items[2] = od.parent.parameter_name + " -- " + od.parameter_name;

            if (od.datatype == DataType.UNKNOWN && od.parent!=null)
            {
                items[3] = od.parent.datatype.ToString();
            }
            else
            {
                items[3] = od.datatype.ToString();
            }

            
            items[4] = od.defaultvalue;


            items[5] = "";

            ListViewItem lvi = new ListViewItem(items);

           // SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)od.index, (byte)od.subindex, gotit);

            sdocallbackhelper help = new sdocallbackhelper();
            help.sdo = null;
            help.od = od;
            lvi.Tag = help;

            listView1.Items.Add(lvi);

          
        }

        void upsucc(SDO sdo)
        {

            //button_read_Click(null, null);

            listView1.Invoke(new MethodInvoker(delegate
            {
                foreach (ListViewItem lvi in listView1.Items)
                {
                   

                    sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                    if (help.sdo != sdo)
                        continue;

                    sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.index, (byte)help.od.subindex, gotit);
                    help.sdo = sdo;
                    lvi.Tag = help;

                    break;


                }
            }));

        }

        void gotit(SDO sdo)
        {
            listView1.Invoke(new MethodInvoker(delegate
            {


                foreach (ListViewItem lvi in listView1.Items)
                {
                    sdocallbackhelper h = (sdocallbackhelper)lvi.Tag;
                    if (h.sdo == sdo)
                    {
                        if (sdo.exp == true)
                        {
                            switch(h.od.datatype)
                            {
                                case DataType.REAL32:

                                    float myFloat = System.BitConverter.ToSingle(BitConverter.GetBytes(h.sdo.expitideddata), 0);
                                    lvi.SubItems[5].Text = myFloat.ToString();
                                    break;

                                case DataType.INTEGER8:
                                case DataType.INTEGER16:
                                case DataType.INTEGER32:
                                case DataType.UNSIGNED8:
                                case DataType.UNSIGNED16:
                                case DataType.UNSIGNED32:

                                    lvi.SubItems[5].Text = h.sdo.expitideddata.ToString();
                                    break;

                                default:
                                    lvi.SubItems[5].Text = " **UNSUPPORTED **";
                                    break;


                            }
                          
                        }
                        break;
                    }

                }
            }));

            
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {

            if(listView1.SelectedItems.Count==0)
                return;

            if (!lco.isopen())
            {
                MessageBox.Show("CAN not open");
                return;
            }

         

            sdocallbackhelper h = (sdocallbackhelper)listView1.SelectedItems[0].Tag;
            ValueEditor ve = new ValueEditor(h.od, listView1.SelectedItems[0].SubItems[5].Text);

            if(h.od.location == StorageLocation.ROM)
            {
                MessageBox.Show("Cannot editor ROM objects");
                return;
            }

            SDO sdo = null;
            if(ve.ShowDialog()==DialogResult.OK)
            {
                switch (h.od.datatype)
                {
                    case DataType.REAL32:
                        {
                            float val = Convert.ToSingle(ve.newvalue);
                            sdo=lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }

                    case DataType.INTEGER8:
                        {
                            sbyte val = Convert.ToSByte(ve.newvalue);
                            sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }

                    case DataType.INTEGER16:
                        {
                            Int16 val = Convert.ToInt16(ve.newvalue);
                            sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }
                 

                    case DataType.INTEGER32:
                        {
                            Int32 val = Convert.ToInt32(ve.newvalue);
                            sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }
                    case DataType.UNSIGNED8:
                        {
                            byte val = Convert.ToByte(ve.newvalue);
                            sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }
                    case DataType.UNSIGNED16:
                        {
                            UInt16 val = Convert.ToUInt16(ve.newvalue);
                            sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }
                    
                    case DataType.UNSIGNED32:
                        {
                            UInt32 val = Convert.ToUInt32(ve.newvalue);
                            sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.index, (byte)h.od.subindex, val, upsucc);
                            break;
                        }


                        break;

                    default:

                        break;
                }

                h.sdo = sdo;
                listView1.SelectedItems[0].Tag = h;

                

            }
        }

        private void button_read_Click(object sender, EventArgs e)
        {

            if (!lco.isopen())
            {
                MessageBox.Show("CAN not open");
                return;
            }
                
            listView1.Invoke(new MethodInvoker(delegate
            {
                foreach (ListViewItem lvi in listView1.Items)
                {
                    sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;
                    SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.index, (byte)help.od.subindex, gotit);
                    help.sdo = sdo;
                    lvi.Tag = help;

                }

            }));



        }

        private void loadEDSXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog odf = new OpenFileDialog();
            odf.Filter = "XML (*.xml)|*.xml|EDS (*.eds)|*.eds";
            if (odf.ShowDialog() == DialogResult.OK)
            {
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

        private void addtoMRU(string path)
        {
            // if it already exists remove it then let it readd itsself
            // so it will be promoted to the top of the list
            if (_mru.Contains(path))
                _mru.Remove(path);

            _mru.Insert(0, path);

            if (_mru.Count > 10)
                _mru.RemoveAt(10);

            populateMRU();

        }

        private void populateMRU()
        {

            mnuRecentlyUsed.DropDownItems.Clear();

            foreach (var path in _mru)
            {
                var item = new ToolStripMenuItem(path);
                item.Tag = path;
                item.Click += OpenRecentFile;
                switch (Path.GetExtension(path))
                {
                    case ".xml":
                        item.Image = Properties.Resources.GenericVSEditor_9905;
                        break;
                    case ".eds":
                        item.Image = Properties.Resources.EventLog_5735;
                        break;
                  
                }

                mnuRecentlyUsed.DropDownItems.Add(item);
            }
        }

        private void SDOEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            var mruFilePath = Path.Combine(appdatafolder, "SDOMRU.txt");
            System.IO.File.WriteAllLines(mruFilePath, _mru);
        }

        private void SDOEditor_Load(object sender, EventArgs e)
        {
            //First lets create an appdata folder

            // The folder for the roaming current user 
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Combine the base folder with your specific folder....
            appdatafolder = Path.Combine(folder, "CanMonitor");

            // Check if folder exists and if not, create it
            if (!Directory.Exists(appdatafolder))
                Directory.CreateDirectory(appdatafolder);

            var mruFilePath = Path.Combine(appdatafolder, "SDOMRU.txt");
            if (System.IO.File.Exists(mruFilePath))
                _mru.AddRange(System.IO.File.ReadAllLines(mruFilePath));

            populateMRU();
        }

    }

}
