using System;
using PDOInterface;
using libCanopenSimple;

namespace NanomsgBridge
{
    public class NanoBridge : InterfaceService, IPDOParser
    {

        private libCanopenSimple.libCanopenSimple lcobridge = new libCanopenSimple.libCanopenSimple();

        public NanoBridge()
        {
            lcobridge.open("ipc://can_id1", BUSSPEED.BUS_500Kbit, "can_nanomsg_win32");
            lcobridge.packetevent += Lcobridge_packetevent;
          
        }

        private void Lco_packetevent(canpacket p, DateTime dt)
        {
            lcobridge.SendPacket(p, true);
        }

        public void endsdo(int node, int index, int sub, byte[] payload)
        {

        }

        private void Lcobridge_packetevent(canpacket p, DateTime dt)
        {
            _lco.SendPacket(p, true);
        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

        public void registerPDOS()
        {
        }

        public override void DriverStateChange(ConnectionChangedEventArgs e)
        {
            if(e.connecting==true)
            {
                _lco.packetevent += Lco_packetevent;
            }
        }
    }
}
