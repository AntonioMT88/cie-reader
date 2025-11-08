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

        public Configuration() {}
       
    }    

    public class WebSocketConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }    
        public string WsApiKey { get; set; }
    }

}
