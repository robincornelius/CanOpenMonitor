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
using System.Windows.Forms;
using libCanopenSimple;


namespace PluginExample
{
    public class PluginExample : InterfaceService, IPDOParser
    {

     
        public PluginExample()
        {
            addverb("hello", "Tools", sayhello);
            addverb("New menu", "_root_", null);
            addverb("HELLO 2", "New menu", sayhello2);
            addverb("---", "New menu", null);
            addverb("More", "New menu", sayhello2);
            addverb("More", "New menu", sayhello3);
            addverb("More", "File", sayhello3);

        }

        void sayhello(object sender, System.EventArgs e)
        {
            if (_lco == null || !_lco.isopen())
                return;

            MessageBox.Show("HELLO WORLD");
            
            _lco.NMT_start();

        }


        void sayhello2(object sender, System.EventArgs e)
        {

            MessageBox.Show("HELLO WORLD 2");

        }

        void sayhello3(object sender, System.EventArgs e)
        {

            MessageBox.Show("HELLO WORLD 3");

        }

        public void registerPDOS()
        {

        }

        public string decodesdo(int node, int index, int sub, byte[] payload)
        {
            return "";
        }

    }
}
