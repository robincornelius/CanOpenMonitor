using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libCanopenSimple;

namespace CanMonitor
{
    public partial class Flasher : Form
    {
        libCanopen lco;
        IntelHex ih;
        public Flasher(libCanopen lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                ih = new IntelHex();
                ih.loadhex(ofd.FileName);

            }



        }

        private void button_flash_Click(object sender, EventArgs e)
        {
            //The data transfer is based on the CANopen standard. More precisely the object 
            //dictionary entry at index 0x1F50, subindex 0x01 is used to program memory pages. 
            //Data is transferred one memory page at a time using SDO(addres 0x600 + node id) 
            //and block transfers are not supported. The first 3 bytes contain the page address, 
            //followed by the 24 bit instructions of the current page. The main program can be 
            //started by writing 0x01 to object 0x1F51, subindex 0x01 or sending an NMT message 
            //to put the device in operational mode. The bootloader is not fully CANopen 
            //compliant(deemed unnecessary), but should be compatible with most CANopen devices.

            if (ih == null)
                return;

            currentpage = 0;

            nextsdoplease(null);

        }

        UInt32 currentpage;

        void nextsdoplease(SDO sdo)
        {

            if (ih.pages.ContainsKey(currentpage))
            {

                byte[] sdodata = new byte[1024 + 3]; //pagesize + header;

                byte[] pagebits = BitConverter.GetBytes(currentpage);

                //24 bit page address
                sdodata[0] = pagebits[0];
                sdodata[1] = pagebits[1];
                sdodata[2] = pagebits[2];

                for (int x = 0; x < 1024; x++)
                {
                    sdodata[x + 3] = ih.pages[currentpage][x];
                }

                textBox1.Invoke(new MethodInvoker(delegate ()
                {
                    textBox1.AppendText(String.Format("writing page {0}/{1}\r\n", currentpage, ih.maxpageno));
                }));

                currentpage++;
                lco.SDOwrite((byte)numericUpDown_node.Value, 0x1f50, 0x01, sdodata, nextsdoplease);
            }
            else
            {
                textBox1.Invoke(new MethodInvoker(delegate ()
                {
                    textBox1.AppendText(String.Format("Skipping page {0}\r\n", currentpage));
                }));

                currentpage++;
                if(currentpage<=ih.maxpageno)
                {
                    nextsdoplease(null);
                }
                else
                {
                    textBox1.Invoke(new MethodInvoker(delegate ()
                    {
                        textBox1.AppendText("DONE ALL PAGES\r\n");
                    }));
                }
             
            }


          

        }
    }
}
