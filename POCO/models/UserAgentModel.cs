using Newtonsoft.Json;

namespace POCO.Models
{
    public class UserAgent
    {
        [JsonProperty("user_agents")]
        public string[] Agent { get; set; }
    }
}
