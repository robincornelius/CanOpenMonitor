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

namespace NMTPlugin
{
    public partial class NMTFrm : DockContent
    {
        libCanopenSimple.libCanopenSimple lco;

        public NMTFrm(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;
            InitializeComponent();
        }


        private void button_startbus_Click(object sender, EventArgs e)
        {
            lco.NMT_start((byte)numericUpDown1.Value);
        }

        private void button_stopbus_Click(object sender, EventArgs e)
        {
            lco.NMT_stop((byte)numericUpDown1.Value);
        }

        private void button_preop_Click(object sender, EventArgs e)
        {
            lco.NMT_preop((byte)numericUpDown1.Value);
        }

        private void button_resetcomms_Click(object sender, EventArgs e)
        {
            lco.NMT_ResetComms((byte)numericUpDown1.Value);
        }

        private void button_resetnodes_Click(object sender, EventArgs e)
        {
            lco.NMT_ResetNode((byte)numericUpDown1.Value);
        }
    }
}
