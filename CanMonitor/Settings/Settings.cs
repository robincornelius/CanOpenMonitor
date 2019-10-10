using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Collections.Generic;

namespace N_SettingsMgr
{
    public static class SettingsMgr
    {

        public enum manualbuttontypes
        {
            BUTTON_CALIBRATE=1,
            BUTTON_MAG=2,
            BUTTON_MEASURE=3,

        }

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
