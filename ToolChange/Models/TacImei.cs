using System.Text.Json.Serialization;

namespace DeepDroid.Models
{
    internal class TacImei
    {
        [JsonPropertyName("tac")]
        public List<long> Tac { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
