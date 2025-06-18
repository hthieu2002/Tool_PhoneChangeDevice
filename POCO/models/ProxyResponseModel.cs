using Newtonsoft.Json;

namespace POCO.Models
{
    public class ProxyStatusResponseModel
    {
        [JsonProperty("status")]
        public bool Status { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public class ProxyResponseModel
    {
        [JsonProperty("ip")]
        public string IP { get; set; } = "";
        [JsonProperty("query")]
        public string PublicIp { get; set; } = "";
        [JsonProperty("regionName")]
        public string RegionName { get; set; }
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
        [JsonProperty("org")]
        public string Org { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; } = "";
        [JsonProperty("proto")]
        public string Proto { get; set; } = "ipv4";
        public string rawResponse { get; set; }
    }
}
