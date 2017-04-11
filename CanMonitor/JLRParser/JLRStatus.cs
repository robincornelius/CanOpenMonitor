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

using System.IO;

namespace JLRParser
{
    public partial class JLRStatus : Form
    {
        libCanopen lco;
        public JLRStatus()
        {
          
            InitializeComponent();

            chart1.Series.Clear();
            chart1.Series.Add("Chan1");
            chart1.Series.Add("Chan2");
            chart1.Series.Add("Chan3");
            chart1.Series.Add("Chan4");

            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart1.Series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart1.Series[3].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;



            
            
        }

        public void setlco(libCanopen lco)
        {
            this.lco = lco;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void JLRStatus_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            timer1.Enabled = false;;
        }

        void sdocallback(SDO sdo)
        {
            this.Invoke(new MethodInvoker(delegate()
            {

                if (sdo.databuffer != null)
                {
                    float val = (float)BitConverter.ToSingle(sdo.databuffer, 0);
                    chart1.Series[sdo.subindex - 1].Points.AddY(val);
                }
            }));

        }

        void timer1_Tick(object sender, EventArgs e)
        {
            if (lco == null)
                return;

            lco.SDOread(0x07, 0x6403, 0x01, sdocallback);
            lco.SDOread(0x07, 0x6403, 0x02, sdocallback);
            lco.SDOread(0x07, 0x6403, 0x03, sdocallback);
            lco.SDOread(0x07, 0x6403, 0x04, sdocallback);


        }

        public void updateGM(byte chan,float val)
        {
            if (chan > 15)
                return;

            System.IO.StreamWriter file = new System.IO.StreamWriter(@"gausslog.txt",true);
            file.Write(val.ToString() + "\t");

            //if (chan == 15)
              //  file.Write("\r\n");

            file.Close();

            string name = string.Format("label{0}",chan);

            this.Invoke(new MethodInvoker(delegate()
            {
                foreach (Control c in this.Controls)
                {
                    if (c.Name == name)
                    {
                        Label l = (Label)c;
                        c.Text = string.Format("{0:0.000}", val*1000.0);

                    }
                }

                if (chan == 15)
                {
                }


            }));


        }

        public void updateflux(float val)
        {

            System.IO.StreamWriter file = new System.IO.StreamWriter(@"gausslog.txt", true);
            file.Write(val.ToString() + "\t");

            file.Write("\r\n");

            file.Close();


            this.Invoke(new MethodInvoker(delegate()
            {

                label_flux.Text = string.Format("{0:0.000} mVs", val * 100000.0);

            }));

        }

        public void updatemaxvoltage(int vol,int fx)
        {

            this.Invoke(new MethodInvoker(delegate()
            {
                progressBar1.Maximum = vol;
                label_fx.Text = fx.ToString();
            }));
        }

        public void updatevoltage(int vol,string charger,string fsm)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                label_fsm.Text = fsm;
                label_charger.Text = charger;

                if (vol > progressBar1.Maximum)
                    vol = progressBar1.Maximum;

                if (vol < progressBar1.Minimum)
                    vol = progressBar1.Minimum;

                    progressBar1.Value = vol;
            }));

        }

        public void updatetemp(byte chan,float val)
        {
            if (chan > 3)
                return;

            string name = string.Format("temp{0}", chan);

            this.Invoke(new MethodInvoker(delegate()
            {
                foreach (Control c in this.Controls)
                {
                    if (c.Name == name)
                    {
                        Label l = (Label)c;
                        c.Text = string.Format("{0:0.0}oC", val);
                    }
                }
            }));
        }
        

    }
}
