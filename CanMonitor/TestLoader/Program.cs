using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libCanopenSimple;

namespace TestLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            DriverLoader loader = new libCanopenSimple.DriverLoader();
            DriverInstance driver = loader.loaddriver("can_usb_win32.dll");
            DriverInstance.struct_s_BOARD brd = new DriverInstance.struct_s_BOARD();
            brd.busname = "COM4";
            brd.baudrate = "500K";
            driver.open(brd);
          
            DriverInstance.Message msg = new DriverInstance.Message();

            driver.rxmessage += Driver_rxmessage;
            
            while(true)
            {
                System.Threading.Thread.Sleep(10);
            }

        }

        private static void Driver_rxmessage(DriverInstance.Message msg)
        {
            Console.WriteLine(string.Format("RX MESSAGE ID {0:x2} LEN {1:x2} DATA {2:x2}", msg.cob_id, msg.len, msg.data));
        }
    }
}
