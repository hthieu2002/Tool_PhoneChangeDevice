
using Newtonsoft.Json;
namespace POCO.Models
{
    public class AppData
    {
        private string currentDir;
        private string absolutePath;
        private string size;
        [JsonProperty("app_data_path")]
        public string FileName { get => currentDir; set => currentDir = value; }
        [JsonProperty("source_path")]
        public string AbsolutePath { get => absolutePath; set => absolutePath = value; }
        [JsonProperty("size")]
        public string Size { get => size; set => size = value; }
    }
}
