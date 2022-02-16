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

namespace SDOEditorPlugin
{
    public partial class SDOEditor : Form
    {

        EDSsharp eds;
        libCanopenSimple.libCanopenSimple lco;
        string filename = null;
        private string appdatafolder;

        private List<string> _mru = new List<string>();

        Timer refreshtimer = new Timer();


        public SDOEditor(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
            refreshtimer.Tick += Refreshtimer_Tick;
        }

        private void Refreshtimer_Tick(object sender, EventArgs e)
        {

            if (button_read.Enabled == false)
                return;

            listView1.Invoke(new MethodInvoker(delegate
            {
                button_read.Enabled = false;
                foreach (ListViewItem lvi in listView1.Items)
                {

                    lvi.BackColor = Color.White;
                    sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;
                    SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.Index, (byte)help.od.Subindex, gotit);
                    help.sdo = sdo;
                    lvi.Tag = help;

                }

            }));
        }

        private void loadeds(string filename)
        {
            if (filename == null || filename == "")
                return;

            bool isdcf = false;
            bool isemptydcf = false;

            button_writeDCF.Enabled = false;

            try
            {

                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".xdd":
                    {
                        CanOpenXDD xdd = new CanOpenXDD();

                        xdd.readXML(filename);

                        Bridge b = new Bridge();
                        eds = xdd.convert(xdd.dev);
                        
                        eds.xmlfilename = filename;
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


            this.filename = filename;
            addtoMRU(filename);
        }

        private void updatetable(bool isdcf=false,bool isemptydcf=false)
        {

            listView1.BeginUpdate();
            if (!isdcf)
                listView1.Items.Clear();

            //           StorageLocation loc = StorageLocation


            foreach (ODentry tod in eds.ods.Values)
            {


                if (comboBoxtype.SelectedItem.ToString() != "ALL")
                {
                    if (comboBoxtype.SelectedItem.ToString() == "EEPROM" && (tod.StorageLocation.ToUpper() != "EEPROM"))
                        continue;
                    if (comboBoxtype.SelectedItem.ToString() == "ROM" && (tod.StorageLocation.ToUpper() != "ROM"))
                        continue;
                    if (comboBoxtype.SelectedItem.ToString() == "RAM" && (tod.StorageLocation.ToUpper() != "RAM"))
                        continue;

                }


                if (tod.Disabled == true)
                    continue;

                if (tod.Index < 0x2000 && checkBox_useronly.Checked == true)
                    continue;

                if (tod.objecttype == ObjectType.ARRAY || tod.objecttype == ObjectType.REC)
                {
                    foreach (ODentry subod in tod.subobjects.Values)
                    {
                        if (subod.Subindex == 0)
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
                    lvi.SubItems[6].Text = od.actualvalue;
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
            }

            if (dcf)
            {
                adddcfvalue(od);
            }

        }

        void upsucc(SDO sdo)
        {

            //button_read_Click(null, null);

           
                listView1.BeginInvoke(new MethodInvoker(delegate
                {
                    foreach (ListViewItem lvi in listView1.Items)
                    {


                        sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                        if (help.sdo != sdo)
                            continue;

                        sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.Index, (byte)help.od.Subindex, gotit);
                        help.sdo = sdo;
                        lvi.Tag = help;

                        break;


                    }
                }));
            

        }

        void testnumber(ListViewItem lvi)
        {
            Int64 i1, i2;

            if (Int64.TryParse(lvi.SubItems[5].Text, out i1) && Int64.TryParse(lvi.SubItems[4].Text, out i2))
            {
                if (i1 != i2)
                {
                    lvi.BackColor = Color.Red;
                }
            }
        }

        void gotit(SDO sdo)
        {
            try
            {

                listView1.Invoke(new MethodInvoker(delegate
                {

                    if (lco.getSDOQueueSize() == 0)
                        button_read.Enabled = true;

                    label_sdo_queue_size.Text = string.Format("SDO Queue Size: {0}", lco.getSDOQueueSize());

                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        sdocallbackhelper h = (sdocallbackhelper)lvi.Tag;
                        if (h.sdo == sdo)
                        {
                            if (sdo.state == SDO.SDO_STATE.SDO_ERROR)
                            {
                                lvi.SubItems[5].Text = " **ERROR **";
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

                                        float fout;
                                        if (float.TryParse(lvi.SubItems[4].Text, out fout))
                                        {
                                            if (fout != myFloat)
                                            {
                                                lvi.BackColor = Color.Red;
                                            }
                                        }

                                        break;

                                    case DataType.REAL64:

                                        double myDouble = System.BitConverter.ToDouble(h.sdo.databuffer, 0);
                                        lvi.SubItems[5].Text = myDouble.ToString();

                                        //fixme bad test
                                        if(lvi.SubItems[5].Text!=lvi.SubItems[4].Text)
                                        {
                                            lvi.BackColor = Color.Red;
                                        }

                                        break;

                                    case DataType.INTEGER8:
                                    {
                                        testnumber(lvi);

                                        byte[] data = BitConverter.GetBytes(h.sdo.expitideddata);
                                        byte num = data[0];
                                        lvi.SubItems[5].Text = String.Format("{0}", num);
                                        break;
                                    }

                                    case DataType.INTEGER16:
                                    {
                                        testnumber(lvi);

                                        byte[] data = BitConverter.GetBytes(h.sdo.expitideddata);
                                        Int16 num = BitConverter.ToInt16(data, 0);
                                        lvi.SubItems[5].Text = String.Format("{0}", num);
                                        break;
                                    }

                                    case DataType.INTEGER32:
                                    {

                                        testnumber(lvi);

                                        byte[] data = BitConverter.GetBytes(h.sdo.expitideddata);
                                        Int32 num = BitConverter.ToInt32(data, 0);
                                        lvi.SubItems[5].Text = String.Format("{0}", num);
                                        break;

                                    }

                                    case DataType.UNSIGNED8:
                                    case DataType.UNSIGNED16:
                                    case DataType.UNSIGNED32:

                                        lvi.SubItems[5].Text = String.Format("{0}", h.sdo.expitideddata);

                                        testnumber(lvi);

                                        break;

                                    case DataType.VISIBLE_STRING:

                                        lvi.SubItems[5].Text = System.Text.Encoding.UTF8.GetString(h.sdo.databuffer);
                                        if (lvi.SubItems[5].Text != lvi.SubItems[4].Text)
                                        {
                                            lvi.BackColor = Color.Red;
                                        }


                                        break;

                                    case DataType.OCTET_STRING:

                                        StringBuilder sb = new StringBuilder();

                                        foreach (byte b in h.sdo.databuffer)
                                        {
                                            sb.Append(string.Format("{0:x} ", b));
                                        }

                                        lvi.SubItems[5].Text = sb.ToString();

                                        //fixme bad test
                                        if (lvi.SubItems[5].Text != lvi.SubItems[4].Text)
                                        {
                                            lvi.BackColor = Color.Red;
                                        }


                                        break;


                                    case DataType.UNSIGNED64:
                                    {
                                        testnumber(lvi);

                                        UInt64 data = (UInt64)System.BitConverter.ToUInt64(h.sdo.databuffer, 0);
                                        lvi.SubItems[5].Text = String.Format("{0:x}", data);
                                    }
                                    break;

                                    case DataType.INTEGER64:
                                    {
                                        testnumber(lvi);

                                        Int64 data = (Int64)System.BitConverter.ToInt64(h.sdo.databuffer, 0);
                                        lvi.SubItems[5].Text = String.Format("{0:x}", data);
                                    }
                                    break;

                                    case DataType.BOOLEAN:
                                    {
                                            lvi.SubItems[5].Text = String.Format("{0}", h.sdo.expitideddata);
                                            testnumber(lvi);
                                    }
                                    break;


                                    default:
                                        lvi.SubItems[5].Text = " **UNSUPPORTED **";
                                        break;


                                }

                                if(lvi.BackColor==Color.Red)
                                {
                                    if(h.od.accesstype == EDSsharp.AccessType.ro || h.od.accesstype==EDSsharp.AccessType.@const)
                                    {
                                        lvi.BackColor = Color.Yellow;
                                    }
                                }

                            }
                            break;
                        }

                        h.od.actualvalue = lvi.SubItems[5].Text;

                    }



                }));
            }
            catch (Exception e)
            {


            }

            return;


        }

        private SDO dovalueupdate(sdocallbackhelper h,string sval)
        {
            DataType dt = h.od.datatype;

            if (dt == DataType.UNKNOWN && h.od.parent != null)
                dt = h.od.parent.datatype;

            SDO sdo = null;

            switch (dt)
            {
                case DataType.REAL32:
                {

                    float val = (float)new SingleConverter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }

                case DataType.REAL64:
                {

                    double val = (double)new DoubleConverter().ConvertFromString(sval);
                    byte[] payload = BitConverter.GetBytes(val);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, payload, upsucc);
                    break;
                }

                case DataType.INTEGER8:
                {
                    sbyte val = (sbyte)new SByteConverter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }

                case DataType.INTEGER16:
                {
                    Int16 val = (Int16)new Int16Converter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }

    
                case DataType.INTEGER32:
                {
                    Int32 val = (Int32)new Int32Converter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }
                case DataType.UNSIGNED8:
                {
                    byte val = (byte)new ByteConverter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }
                case DataType.UNSIGNED16:
                {
                    UInt16 val = (UInt16)new UInt16Converter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }

                case DataType.UNSIGNED32:
                {
                    UInt32 val = (UInt32)new UInt32Converter().ConvertFromString(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, val, upsucc);
                    break;
                }

                case DataType.INTEGER64:
                {

                    Int64 val = (Int64)new Int64Converter().ConvertFromString(sval);
                    byte[] payload = BitConverter.GetBytes(val);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, payload, upsucc);
                    break;
                }

                case DataType.UNSIGNED64:
                {

                    UInt64 val = (UInt64)new UInt64Converter().ConvertFromString(sval);
                    byte[] payload = BitConverter.GetBytes(val);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, payload, upsucc);
                    break;
                }

                case DataType.VISIBLE_STRING:
                {

                    byte[] payload = Encoding.ASCII.GetBytes(sval);
                    sdo = lco.SDOwrite((byte)numericUpDown_node.Value, (UInt16)h.od.Index, (byte)h.od.Subindex, payload, upsucc);
                    break;
                }


                default:

                    break;
            }

            return sdo;

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {

            if (listView1.SelectedItems.Count == 0)
                return;

            if (!lco.isopen())
            {
                MessageBox.Show("CAN not open");
                return;
            }



            sdocallbackhelper h = (sdocallbackhelper)listView1.SelectedItems[0].Tag;
            ValueEditor ve = new ValueEditor(h.od, listView1.SelectedItems[0].SubItems[5].Text);

            if (h.od.StorageLocation == "ROM")
            {
               // MessageBox.Show("Should not edit ROM objects");

            }

            ve.UpdateValue += delegate (string s)
            {
                h.sdo = dovalueupdate(h, s);
                listView1.SelectedItems[0].Tag = h;
            };


            //SDO sdo = null;
            if (ve.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void Ve_UpdateValue(string value)
        {

        }

        private void button_read_Click(object sender, EventArgs e)
        {

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

            listView1.Invoke(new MethodInvoker(delegate
            {
                button_read.Enabled = false;
                foreach (ListViewItem lvi in listView1.Items)
                {

                    lvi.BackColor = Color.White;
                    sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;
                    SDO sdo = lco.SDOread((byte)numericUpDown_node.Value, (UInt16)help.od.Index, (byte)help.od.Subindex, gotit);
                    help.sdo = sdo;
                    lvi.Tag = help;

                }

            }));



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
                    case ".xdd":
                        item.Image = Properties.Resource1.GenericVSEditor_9905;
                        break;

                    case ".eds":
                        item.Image = Properties.Resource1.EventLog_5735;
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
                    string currentstring = help.od.actualvalue;

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
            button_read.Enabled = true;

        }

        private void button_writeDCF_Click(object sender, EventArgs e)
        {

            if(numericUpDown_node.Value==0)
            {
                MessageBox.Show("Cannot write to node 0, please select a valid node");
                return;
            }

            foreach (ListViewItem lvi in listView1.Items)
            {

                string index = lvi.SubItems[0].Text;
                string sub = lvi.SubItems[1].Text;
                string name = lvi.SubItems[2].Text;

                UInt16 key = Convert.ToUInt16(index, 16);
                byte subi = Convert.ToByte(sub, 16);

                sdocallbackhelper help = (sdocallbackhelper)lvi.Tag;

                string edsstring = help.od.defaultvalue; //the eds value
                string actualstring = lvi.SubItems[5].Text;  // the dcf value
                string dcfstring = lvi.SubItems[6].Text;

                if(actualstring!=dcfstring )
                {
                    if (dcfstring != "")
                    {
                        sdocallbackhelper h = (sdocallbackhelper)lvi.Tag;
                        try
                        {
                            h.sdo = dovalueupdate(h, dcfstring);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(string.Format("Error writing to 0x{0:x4}/{1:x2} details :-\n{2}", h.od.Index, h.od.Subindex, ex.ToString()));
                        }

                        lvi.Tag = h;
                    }
                }
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
                        parentval.objecttype = ObjectType.REC; //we don't know!!
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
                listView1.Invoke(new MethodInvoker(delegate
                {
                    updatetable();
                }));

            }



        }
    }
}
