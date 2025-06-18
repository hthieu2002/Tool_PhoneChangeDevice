using Newtonsoft.Json;
using System;

namespace POCO.Models
{
    public class DeviceConfigModel
    {
        [JsonProperty("isAlwaysGetProxyFromServer")]
        public bool IsAlwaysGetProxyFromServer { get; set; } = false;
        [JsonProperty("serial")]
        public string Serial { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("tinsoft_key")]
        public string TinsoftKey { get; set; } = string.Empty;
        [JsonProperty("private_proxy")]
        public AutoProxyInfo FixedProxy { get; set; }
        [JsonProperty("is_check_reset_proxy")]
        public bool IsCheckResetProxy { get; set; }
        [JsonIgnore]
        public AutoProxyInfo ProxyUsing {
            get 
            {
                return this.FixedProxy ?? this.DynamicProxy;
            }
        }
        [JsonProperty("is_fake_sim")]
        public bool IsFakeSim { get; set; } = false;
        [JsonProperty("country_name")]
        public string CountryName { get; set; } = string.Empty;
        [JsonProperty("Carrier_name")]
        public string CarrierName { get; set; } = string.Empty;
        [JsonProperty("recovery_mail")]
        public string RecoveryMail { get; set; } = "All";
        [JsonProperty("device_source")]
        public string DeviceSource { get; set; } = "All";
        [JsonProperty("device_samsungPercentage")]
        public int DeviceSamsungPercentage { get; set; } = 20;
        [JsonProperty("open_from")]
        public string OpenFrom { get; set; } = "Vending";
        [JsonProperty("is_postScript_Running")]
        public bool IsPostScriptRunning { get; set; }
        [JsonIgnore]
        public bool IsPostScriptRunningSucceeded { get; set; } = false;
        [JsonIgnore]
        public string GroupName { get; set; } = string.Empty;
        [JsonIgnore]
        public int DeviceIndex { get; set; }
        [JsonIgnore]
        public bool IsConnected;
        [JsonIgnore]
        public bool IsActivated { get; set; }
        [JsonIgnore]
        public bool IsAuto { get; set; }
        [JsonIgnore]
        public bool IsStopping { get; set; }
        [JsonIgnore]
        public LicensesModel Licenses { get; set; }
        [JsonIgnore]
        public DateTime LastDateTimeProxyChanged { get; set; }
        [JsonIgnore]
        public bool IsInformedProxyChangedAlready { get; set; }
        [JsonIgnore]
        public AutoProxyInfo DynamicProxy { get; set; }
        [JsonIgnore]
        public int TotalToday { get; set; }
        [JsonIgnore]
        public int TotalTodayLastHour { get; set; }
        [JsonIgnore]
        public bool IsProxyProcessing { get; set; }
        [JsonProperty("isViewCount")]
        public int ViewCount { get; set; } = 0;
        [JsonProperty("isFakeDevice")]
        public bool IsFakeDevice { get; set; } = false;
        [JsonProperty("isFakeTun2Socks")]
        public bool IsFakeTun2Socks { get; set; } = false;
        [JsonProperty("isFakeRedSocks")]
        public bool IsFakeRedSocks { get; set; } = false;
        [JsonProperty("isAndroid13")]
        public bool IsAndroid13 { get; set; } = false;
        [JsonProperty("isWipeDevice")]
        public bool IsWipeDevice { get; set; } = false;
        [JsonProperty("isListWifi")]
        public bool IsListWifi { get; set; } = false;
        [JsonProperty("isLoginMail")]
        public bool IsLoginMail { get; set; } = false;
        [JsonIgnore]
        public AutoProxyInfo ProxyForDevice { get; set; } = new AutoProxyInfo();
        [JsonProperty("numberSDK")]
        public int SDK { get; set; } = 30;
    }
}
