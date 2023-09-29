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
using WeifenLuo.WinFormsUI.Docking;

namespace FlashLoader
{
    public partial class Flasher : DockContent
    {
        libCanopenSimple.libCanopenSimple lco;
        IntelHex ih;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        bool busy = false;
        string filename = "";
        byte node;
        byte node2;

        public Flasher(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
            this.FormClosing += Flasher_FormClosing;
        }

        private void Flasher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(busy)
            {
                e.Cancel = true;   
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if(ofd.ShowDialog() == DialogResult.OK)
            {

                button1.Enabled = false;
                button_flash.Enabled = false;
                busy = true;

                ih = new IntelHex();
                //ih.loadhex(ofd.FileName);

                textBox1.AppendText("** Loading hex file **\r\n");

                filename = ofd.FileName;
                node2 = (byte)numericUpDown_node.Value;
                node = 0x7f;

                backgroundWorker1 = new BackgroundWorker();

                backgroundWorker1.DoWork +=
                   new DoWorkEventHandler(backgroundWorker1_DoWork);

                backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;

                backgroundWorker1.RunWorkerAsync();
            }



        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            this.Invoke(new MethodInvoker(delegate ()
            {
                button1.Enabled = true;
                button_flash.Enabled = true;
                busy = false;

                textBox1.AppendText("** Hex file loaded **\r\n");


                progressBar1.Minimum = 0;
                progressBar1.Maximum = ih.maxpageno;
                progressBar1.Value = 0;

            }));

        }


        // This event handler is where the time-consuming work is done.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ih.loadhex(filename);


        }


        // This event handler is where the time-consuming work is done.
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
           
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

            node2 = (byte)numericUpDown_node.Value;
            node = 0x7f;

            if (ih == null)
                return;


            busy = true;

            currentpage = 0;

            lco.nmtecevent += Lco_nmtecevent;

            textBox1.AppendText("** Enabling boot loader restart **\r\n");

            UInt32 boot = 0x66797984;

            textBox1.AppendText("** Sending enter boot loader command **\r\n");

            lco.SDOwrite(node2, 0x5fff, 0x00, boot, a => {
               
            });

            //backgroundWorker2.RunWorkerAsync();

        }

        private void Lco_nmtecevent(canpacket p,DateTime dt)
        {
            if (!busy)
                return;

           
            if ((p.cob&0x0ff) == 0x7f)
            {

                if (p.len ==1)
                {

                    if (p.data[0] == 0x00)
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            textBox1.AppendText("** RESET EVENT **\r\n");
                        }));


                        System.Threading.Thread.Sleep(1000);

                        nextsdoplease(null);

                        lco.nmtecevent -= Lco_nmtecevent; ;
                    }
                }
            }
        }

     
        

        UInt32 currentpage;

        void nextsdoplease(SDO sdo)
        {

            System.Threading.Thread.Sleep(1000);

            if(sdo!=null && sdo.state == SDO.SDO_STATE.SDO_ERROR)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    textBox1.AppendText($"SDO ERROR\r\n");
                }));
            }

            if (ih.pages.ContainsKey(currentpage))
            {

                byte[] sdodata = new byte[3*1024 + 3]; //pagesize + header;


                UInt32 pageaddr = currentpage * 2*1024; //awesome

                byte[] pagebits = BitConverter.GetBytes(pageaddr);

                //24 bit page address
                sdodata[0] = pagebits[0]; //LSB
                sdodata[1] = pagebits[1];
                sdodata[2] = pagebits[2]; //c# MSB

                for (int x = 0; x < (3*1024); x++)
                {
                    sdodata[x + 3] = ih.pages[currentpage][x];
                }

                textBox1.Invoke(new MethodInvoker(delegate ()
                {
                    progressBar1.Value = (int)currentpage;
                    label2.Text = (String.Format("writing page {0}/{1}\r\n", currentpage, ih.maxpageno));
                }));

                currentpage++;
                //nextsdoplease(null);
                lco.SDOwrite((byte)node, 0x1f50, 0x01, sdodata, nextsdoplease);
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
                        label2.Text = "DONE";
                        textBox1.AppendText("DONE ALL PAGES\r\n");
                        busy = false;
                        button1.Enabled = true;
                        button_flash.Enabled = true;

                        textBox1.AppendText("** booting main program **\r\n");

                        lco.NMT_start(node);

                    }));
                }
             
            }


          

        }

        private void writepage(int page)
        {

        }


        private void button_run_Click(object sender, EventArgs e)
        {

            lco.NMT_start(0);

        }

        private void button_flashzero_Click(object sender, EventArgs e)
        {

            currentpage = 0;

            byte[] sdodata = new byte[3 * 1024 + 3]; //pagesize + header;


            UInt32 pageaddr = currentpage * 2 * 1024; //awesome

            byte[] pagebits = BitConverter.GetBytes(pageaddr);

            //24 bit page address
            sdodata[0] = pagebits[0]; //LSB
            sdodata[1] = pagebits[1];
            sdodata[2] = pagebits[2]; //c# MSB

            for (int x = 0; x < (3 * 1024); x++)
            {
                sdodata[x + 3] = ih.pages[currentpage][x];
            }

            textBox1.Invoke(new MethodInvoker(delegate ()
            {
                progressBar1.Value = (int)currentpage;
                label2.Text = (String.Format("writing page {0}/{1}\r\n", currentpage, ih.maxpageno));
            }));

            currentpage++;
            //nextsdoplease(null);
            lco.SDOwrite((byte)node, 0x1f50, 0x01, sdodata, null);

        }

        private void button_enterbootloader_Click(object sender, EventArgs e)
        {


            UInt32 boot = 0x66797984;

            node2 = (byte)numericUpDown_node.Value;

            textBox1.AppendText("** Sending enter boot loader command **\r\n");

            lco.SDOwrite(node2, 0x5fff, 0x00, boot, a => {

            });

        }
    }
}
