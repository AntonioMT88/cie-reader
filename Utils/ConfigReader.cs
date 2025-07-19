using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieReader.Utils
{
    internal class ConfigReader
    {
        public Configuration Configuration { get; set; }
        public ConfigReader() 
        { 
            LoadJson();           
        }
        private void LoadJson()
        {
            if (File.Exists("config.json"))
            {
                using (StreamReader r = new StreamReader("config.json"))
                {
                    string json = r.ReadToEnd();
                    Configuration = JsonConvert.DeserializeObject<Configuration>(json);
                }
            }
            else 
            {
                Configuration = new Configuration();
            }
        }
    }
}
