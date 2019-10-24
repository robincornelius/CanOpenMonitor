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

namespace PDOInjector
{
    public partial class PDOForm : Form
    {
        libCanopenSimple.libCanopenSimple lco;
        public PDOForm(libCanopenSimple.libCanopenSimple lco)
        {
            this.lco = lco;

            InitializeComponent();
            pdoValue1.sendpdo += PdoValue_sendpdo;
            pdoValue2.sendpdo += PdoValue_sendpdo;
            pdoValue3.sendpdo += PdoValue_sendpdo;
            pdoValue4.sendpdo += PdoValue_sendpdo;

        }

        private void PdoValue_sendpdo(object sender, EventArgs e)
        {
            PDOValue p = (PDOValue)sender;
            lco.writePDO(p.cob, p.data);
        }
    }
}
