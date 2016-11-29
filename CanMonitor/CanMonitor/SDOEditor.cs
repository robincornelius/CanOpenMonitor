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

        public SDOEditor(libCanopen lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog odf = new OpenFileDialog();
            odf.Filter = "XML (*.xml)|*.xml|EDS (*.eds)|*.eds";
            if (odf.ShowDialog() == DialogResult.OK)
            {
                switch(Path.GetExtension(odf.FileName).ToLower())
                {
                    case ".xml":
                        {
                            CanOpenXML coxml = new CanOpenXML();
                            coxml.readXML(odf.FileName);

                            Bridge b = new Bridge();

                            eds = b.convert(coxml.dev);
                            eds.filename = odf.FileName;
                          
                        }

                        break;

                    case ".eds":
                        {
                            eds = new EDSsharp();
                            eds.loadfile(odf.FileName);
                        }
                        break;


                }

                numericUpDown_node.Value = eds.di.concreteNodeId;

                listView1.BeginUpdate();
                listView1.Items.Clear();
    
                foreach(ODentry tod in eds.ods.Values)
                {
                    if (tod.location != StorageLocation.EEPROM)
                        continue;

                    if(tod.objecttype==ObjectType.ARRAY || tod.objecttype == ObjectType.REC)
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

   
            }
           
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

            items[2] = od.parameter_name;
            
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

            sdocallbackhelper h = (sdocallbackhelper)listView1.SelectedItems[0].Tag;
            ValueEditor ve = new ValueEditor(h.od, listView1.SelectedItems[0].SubItems[5].Text);

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
    }

}
