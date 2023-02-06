using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIComponent
{
    public partial class TrafficLight : UserControl
    {
        public TrafficLight()
        {
            InitializeComponent();
        }

        public void SetRed(bool on)
        {
            if(on==true)
                pictureBox_red.Image = GUIComponents.Properties.Resources.trafficlightredon;
            else
                pictureBox_red.Image = GUIComponents.Properties.Resources.trafficlightredoff;
        }
        public void SetOrange(bool on)
        {
            if (on == true)
                pictureBox_orange.Image = GUIComponents.Properties.Resources.trafficlightorangeon;
            else
                pictureBox_orange.Image = GUIComponents.Properties.Resources.trafficlightorangeoff;
        }
        public void SetGreen(bool on)
        {
            if (on == true)
                pictureBox_green.Image = GUIComponents.Properties.Resources.trafficlightgreenon;
            else
                pictureBox_green.Image = GUIComponents.Properties.Resources.trafficlightgreenoff;
        }

    }
}
