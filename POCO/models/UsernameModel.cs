using Newtonsoft.Json;

namespace POCO.Models
{
    public class Username
    {
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("firstnames")]
        public string[] FirstNames { get; set; }
        [JsonProperty("lastnames")]
        public string[] LastNames { get; set; }
        [JsonProperty("midnames")]
        public string[] MidNames { get; set; }
    }

    public class CommonName
    {
        [JsonProperty("names")]
        public string[] Names { get; set; }
    }
}
