using System.Text.Json.Serialization;

namespace CieReader
{
    public class Configuration
    {
        [JsonPropertyName("websocket")]
        public WebSocketConfig WebSocketConfig { get; set;}
        [JsonPropertyName("autoStartUp")]
        public bool AutoStartUp { get; set; }

        public Configuration() {}
       
    }    

    public class WebSocketConfig
    {
        [JsonPropertyName("host")]
        public string Host { get; set; }
        [JsonPropertyName("port")]
        public int Port { get; set; }
        [JsonPropertyName("wsApiKey")]
        public string WsApiKey { get; set; }
    }

}
