using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JLRParser
{
    public partial class JLRStatus : Form
    {
        public JLRStatus()
        {
            InitializeComponent();
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void JLRStatus_Load(object sender, EventArgs e)
        {

        }

        public void updateGM(byte chan,float val)
        {
            if (chan > 15)
                return;

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
            }));


        }

        public void updateflux(float val)
        {
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
