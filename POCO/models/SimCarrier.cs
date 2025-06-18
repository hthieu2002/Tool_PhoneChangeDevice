using Newtonsoft.Json;
namespace POCO.Models
{
    public class CarrierAttribute
    {
        string mcc;
        string mnc;
        [JsonProperty("mcc")]
        public string Mcc { get => mcc; set => mcc = value; }
        [JsonProperty("mnc")]
        public string Mnc { get => mnc; set => mnc = value; }
    }
    public class SimCarrier
    {
        string name;
        [JsonProperty("carrier_name")]
        public string Name { get => name; set => name = value; }
        [JsonProperty("country_code")]
        public string CountryCode { get => countryCode; set => countryCode = value; }
        [JsonProperty("country_name")]
        public string CountryName { get => countryName; set => countryName = value; }
        [JsonProperty("country_iso")]
        public string CountryIso { get => countryIso; set => countryIso = value; }

        string countryCode;
        string countryName;
        string countryIso;
        [JsonProperty("carrier_attribute")]
        public CarrierAttribute Attribute { get; set; }
    }

}
