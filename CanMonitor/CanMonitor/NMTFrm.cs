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
    public partial class NMTFrm : Form
    {
        libCanopen lco;

        public NMTFrm(libCanopen lco)
        {
            this.lco = lco;
            InitializeComponent();
        }

        private void button_startbus_Click(object sender, EventArgs e)
        {
            lco.NMT_start(0);
        }

        private void button_stopbus_Click(object sender, EventArgs e)
        {
            lco.NMT_stop(0);
        }

        private void button_preop_Click(object sender, EventArgs e)
        {
            lco.NMT_preop(0);
        }

        private void button_resetcomms_Click(object sender, EventArgs e)
        {
            lco.NMT_ResetComms(0);
        }

        private void button_resetnodes_Click(object sender, EventArgs e)
        {
            lco.NMT_ResetNode(0);
        }
    }
}
