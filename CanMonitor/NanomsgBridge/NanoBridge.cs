/*
    This file is part of CanOpenMonitor.

    CanOpenMonitor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CanOpenMonitor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CanOpenMonitor.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

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
