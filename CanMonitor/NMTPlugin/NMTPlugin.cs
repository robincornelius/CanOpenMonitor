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
using libCanopenSimple;

namespace NMTPlugin
{
    public class NMTPlugin : InterfaceService, IPDOParser
    {
        public NMTPlugin()
        {
            addverb("NMT", "_root_", null);
            addverb("Start Bus", "NMT", startbus);
            addverb("Pre-op Bus", "NMT", preopbus);
            addverb("Stop Bus", "NMT", stopbus);
            addverb("Reset Bus", "NMT", resetbus);
            addverb("Reset Communication", "NMT", resetcomms);
            addverb("---", "NMT", null);
            addverb("Advanced", "NMT", showdlg);

        }

        public void registerPDOS()
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

    
        void startbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_start();
        }

        void preopbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_preop();
        }

        void stopbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_stop();
        }

        void resetbus(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_ResetNode();
        }

        void resetcomms(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            _lco.NMT_ResetComms();
        }

        void showdlg(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            NMTFrm FRM = new NMTFrm(_lco);
            FRM.Show();
        }
    }


}
