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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDOInterface;

namespace FlashLoader
{
    public class FlashLoader : InterfaceService, IPDOParser
    {
        libCanopenSimple.libCanopenSimple _lco;

        public FlashLoader()
        {
            addverb("---", "File", null);
            addverb("Flash node", "File", showdlg);
            addverb("---", "File", null);
        }

        public void registerPDOS()
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

        public void setlco(libCanopenSimple.libCanopenSimple lco)
        {
            _lco = lco;
        }

        public void showdlg(object sender, System.EventArgs e)
        {

            if (_lco == null || !_lco.isopen())
                return;

            Flasher f = new Flasher(_lco);
            f.ShowDialog();
        }
    }

}

