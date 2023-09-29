using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CanMonitor
{
    public static class ControlExtensions
    {
        public static void DoubleBuffering(this Control control, bool enable)
        {
            var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
        }
    }

    public class driverport : Object
    {
        public string driver;
        public string port;
        public string VID;
        public string PID;

        public override string ToString()
        {
            return port;
        }

        
        public bool issamedriver(driverport obj)
        {
            if (obj == null)
                return false;

            if (this.driver == obj.driver && this.port == obj.port)
                return true;

            return false;

        }
        

    }
}
