using Newtonsoft.Json;
using System.Reflection;

namespace POCO.Models
{
    public class AndroidSimModel
    {
        [JsonProperty("iccid")]
        public string ICCID { get; set; }
        [JsonProperty("imsi")]
        public string IMSI { get; set; }
        [JsonProperty("sim_operator_numeric")]
        public string SimOperatorNumeric { get; set; }
        [JsonProperty("sim_operator_country")]
        public string SimOperatorCountry { get; set; }
        [JsonProperty("sim_operator_name")]
        public string SimOperatorName { get; set; }
    }

    public class GPSModel
    {
        public string lon { get; set; }
        public string lat { get; set; }

    }
    [ObfuscationAttribute(Exclude = false)]
    public class AndroidPhoneModel
    {
        [JsonProperty("name")]
        public string Name { get; set; } // model
        [JsonProperty("board")]
        public string Board { get; set; }
        [JsonProperty("hardware")]
        public string Hardware { get; set; }
        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }
        [JsonProperty("imei0")]
        public string Imei0 { get; set; }
        [JsonProperty("imei1")]
        public string Imei1 { get; set; }
        [JsonProperty("brand")]
        public string Brand { get; set; }
        [JsonProperty("code_name")]
        public string CodeName { get; set; }
        [JsonProperty("wifi_mac_address")]
        public string WifiMacAddress { get; set; }
        [JsonProperty("bluetooth_mac_address")]
        public string BluetoothMacAddress { get; set; }
        [JsonProperty("fingerprint")]
        public string Fingerprint { get; set; }
        [JsonProperty("serialno")]
        public string Serialno { get; set; }
        [JsonProperty("build_host")]
        public string BuildHost { get; set; }
        [JsonProperty("android_id")]
        public string AndroidId { get; set; }
        [JsonProperty("build_id")]
        public string BuildId { get; set; }
        [JsonProperty("gaid")]
        public string Gaid { get; set; }
        public GPSModel GPS { get; set; }
        public AndroidSimModel SimInfo { get; set; }
        public BuildVersion BuildVersion { get; set; }
    }
    public class BuildVersion
    {
        [JsonProperty("release")]
        public string Release { get; set; }
        [JsonProperty("sdk")]
        public string SDK { get; set; }
    }
    public class DeviceOfferModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("board")]
        public string Board { get; set; }
        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }
        [JsonProperty("hardware")]
        public string HardWare { get; set; }
        [JsonProperty("carrier")]
        public string CarrierName { get; set; }
        [JsonProperty("model")]
        public string Model { get; set; }
        [JsonProperty("sdk")]
        public string[] SDK { get; set; }

    }
}
