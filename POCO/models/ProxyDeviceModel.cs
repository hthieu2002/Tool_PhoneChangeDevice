using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POCO.Models
{
    public class ProxyDeviceModel
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Protocol { get; set; }
        public string Type { get; set; }
        public string RotateLink { get; set; }
        [JsonProperty("isUse")]
        public bool IsUse { get; set; } = false;
        [JsonProperty("isUsed")]
        public bool IsUsed { get; set; } = false;
    }
}
