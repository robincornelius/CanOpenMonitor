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

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Collections.Generic;

namespace N_SettingsMgr
{
    public static class SettingsMgr
    {

        public static Settings settings = new Settings();

        public static void readXML(string file)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                StreamReader reader = new StreamReader(file);
                settings = (Settings)serializer.Deserialize(reader);
                reader.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("Unable to load settings file :" + file);
            }
        }

        public static void writeXML(string file)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            StreamWriter writer = new StreamWriter(file);
            serializer.Serialize(writer, settings);
            writer.Close();
        }

        [XmlRoot(ElementName = "Settings")]
        public class Settings
        {
            public Settings()
            {
                options = new Options();
               

            }

            [XmlElement(ElementName = "Options")]
            public Options options { get; set; }
 
        }

        [XmlRoot(ElementName = "Options")]
        public class Options
        {
            public Options()
            {
                selectedport = "COM4";
                selectedrate = 6; //500k
            }

            [XmlElement(ElementName = "selectedport")]
            public String selectedport { get; set; }

            [XmlElement(ElementName = "selectedrate")]
            public int selectedrate { get; set; }
           
        }

    }
}
