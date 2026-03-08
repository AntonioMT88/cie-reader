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
                string configContentJson = File.ReadAllText("config.json");
                Configuration = System.Text.Json.JsonSerializer.Deserialize<Configuration>(configContentJson) ?? new Configuration();
            }
            else 
            {
                Configuration = new Configuration();
            }
        }
    }
}
