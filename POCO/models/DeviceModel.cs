using Newtonsoft.Json;

namespace POCO.Models
{
    public class ComboBoxItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class DeviceModel
    {
        public string Name { get; set; }
        public string Baseband { get; set; }
        public string Bootloader { get; set; }
        public string SerialNo { get; set; }
        public string Model { get; set; }
        public string Board { get; set; }
        public string Hardware { get; set; }
        public string Manufacturer { get; set; }
        public string Brand { get; set; }
        public string Product { get; set; } = "lineage_tissot";
        public string Platform { get; set; } = "msm8953";
        public string Imei { get; set; }
        public string Imei1 { get; set; }
        public string Code { get; set; }
        public string WifiMacAddress { get; set; }
        public string BlueToothMacAddress { get; set; }
        public string RadioVersion { get; set; }
        public string AndroidId { get; set; }
        public string Fingerprint { get; set; }
        public string Release { get; set; }
        public string SDK { get; set; } = "28";
        public string SecurityPath { get; set; }
        public string BuildHost { get; set; }
        public string BuildId { get; set; }
        public string BuildDisplayId { get; set; } //
        public string BuildIncremental { get; set; }
        public string BuildDescription { get; set; }//[crownltexx-user 10 QP1A.190711.020 N960FXXU6ETG3 release-keys]
        public string BuildDate { get; set; }
        public string BuildDateUtc { get; set; }
        public string BuildFlavor { get; set; } //crownltexx-user
        public string Lon { get; set; } = "77";
        public string Lat { get; set; } = "77";
        public string ICCID { get; set; }
        public string IMSI { get; set; }
        public string SimPhoneNumber { get; set; }
        public string SimOperatorNumeric { get; set; }
        public string SimOperatorCountry { get; set; }
        public string SimOperatorName { get; set; }
        public string Gaid { get; set; } = string.Empty;
        public string Gsf { get; set; } = string.Empty;
        //public string Source { get; set; }
        [JsonIgnore]
        public string Tags { get; set; } = "release-keys";
        public int Index { get; set; }

    }

}
