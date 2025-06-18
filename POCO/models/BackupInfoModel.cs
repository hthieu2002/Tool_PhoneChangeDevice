using Newtonsoft.Json;
using System;

namespace POCO.Models
{
    public class BackupInfoModel
    {
        private string email;
        private string note;
        private DateTime timeCreate;
        private string telecom;
        private string fileName;
        private string size;
        private string fullPathDesktop;
        private string account;
        private string vendingVersion;
        private string serialNo;
        private string toolVersion;
        private string deviceCodeName;
        [JsonProperty("email")]
        public string Email { get => email; set => email = value; }
        [JsonProperty("note")]
        public string Note { get => note; set => note = value; }
        [JsonProperty("date_create")]
        public DateTime TimeCreate { get => timeCreate; set => timeCreate = value; }
        [JsonProperty("telecom")]
        public string Telecom { get => telecom; set => telecom = value; }
        [JsonProperty("file_name")]
        public string FileName { get => fileName; set => fileName = value; }
        [JsonProperty("size")]
        public string Size { get => size; set => size = value; }
        [JsonProperty("dir_path_desktop")]
        public string DirPathDesktop { get => fullPathDesktop; set => fullPathDesktop = value; }
        [JsonProperty("account")]
        public string Account { get => account; set => account = value; }
        [JsonProperty("vending_version")]
        public string VendingVersion { get => vendingVersion; set => vendingVersion = value; }
        [JsonProperty("serial_no")]
        public string SerialNo { get => serialNo; set => serialNo = value; }
        [JsonProperty("tool_version")]
        public string ToolVersion { get => toolVersion; set => toolVersion = value; }
        [JsonProperty("device_code_name")]
        public string DeviceCodeName { get => deviceCodeName; set => deviceCodeName = value; }
        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", email, note, timeCreate, telecom, fileName, size);
        }
    }
}
