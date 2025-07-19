using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieReader
{
    public class Configuration
    {
        [JsonProperty("websocket")]
        public WebSocketConfig WebSocketConfig { get; set;}

        public Configuration()
        {
            WebSocketConfig = new WebSocketConfig();
            WebSocketConfig.Host = "localhost";
            WebSocketConfig.Port = 8080;
        }
       
    }

    public class WebSocketConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }        
    }

}
