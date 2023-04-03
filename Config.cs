using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerUDP
{
   
    public class Config
    {
        private const  string configFileName = "Config.xml";
        
        public string BroadcastIp { get; set; }
        public int BroadcastPort { get; set; }
        public int MinRndValue { get; set; }
        public int MaxRndValue { get; set; }
        public double Point { get; set; }
        public int ClientDelayms { get; set; }

        protected internal static void CreateConfigFileIfNotExist()
        {
            if (File.Exists(configFileName)) return;

            XmlSerializer formatter = new XmlSerializer(typeof(Config));

            using (FileStream fs = new FileStream(configFileName, FileMode.OpenOrCreate))
            {
                Config cfg = new Config() {BroadcastIp = "224.10.10.1", BroadcastPort = 5001, MinRndValue = 0, MaxRndValue = 100, Point = 0.01, ClientDelayms = 1000 };
                formatter.Serialize(fs, cfg);
            }

        }
        protected internal static Config DeserializeFromConfigFile()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Config));

            using (FileStream fs = new FileStream(configFileName, FileMode.OpenOrCreate))
            {
                Config cfg = (Config)formatter.Deserialize(fs);
                return cfg;
            }

        }
    }
}
