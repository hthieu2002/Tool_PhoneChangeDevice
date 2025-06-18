using Newtonsoft.Json;
using System.Collections.Generic;

namespace POCO.Models
{
    public class Settings
    {
        [JsonProperty("timeout_load_url")]
        public decimal TimeOutLoadURL { get; set; }
        [JsonProperty("timeout_install_app")]
        public decimal TimeOutInstallApp { get; set; }
        [JsonProperty("save_rrs_percent")]
        public decimal SaveRRSPercent { get; set; }
        [JsonProperty("check_ip_open_app")]
        public bool CheckIPOpenApp { get; set; }
        [JsonProperty("save_ip")]
        public bool SaveIP { get; set; }
        [JsonProperty("wipe_store")]
        public bool WipeStore { get; set; }
        [JsonProperty("smart_link")]
        public bool SmartLink { get; set; }
        [JsonProperty("install_app_without_proxy")]
        public bool InstallAppWithoutProxy { get; set; }
        [JsonProperty("manufacturer")]
        public List<string> Manufacturer { get; set; }
        [JsonProperty("check_country_ip_rrs")]
        public bool CheckCountryIPRRS { get; set; }
        [JsonProperty("days_old_device")]
        public decimal DaysOldDevice { get; set; }

        public BuildVersion BuildVersionMin { get; set; }
        public BuildVersion BuildVersionMax { get; set; }
    }
}
