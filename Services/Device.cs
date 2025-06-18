using POCO.Models;
using System.Collections.Generic;
using System.Reflection;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public enum FileTransferAction
    {
        PUSH,
        PULL
    }

    public enum DeviceStatus
    {
        Online,
        Offline,
        Undefined,
        ReadyToChange
    }
    [ObfuscationAttribute(Exclude = false)]
    public class SimProperty
    {
        private string iCCID = "";
        private string subScriberId = "";

        public string SubScriberId { get => subScriberId; set => subScriberId = value; }
        public string ICCID { get => iCCID; set => iCCID = value; }

        public string SimOperatorCode { get; set; }
    }
    [ObfuscationAttribute(Exclude = false)]
    public class Device
    {
        private string deviceId;
        private DeviceStatus status = DeviceStatus.Online;
        public bool HasSimCardInserted { get; set; } = false;
        private SimProperty simProperty = new SimProperty();

        public DeviceStatus Status
        {
            get { return status; }
            set { status = value; }
        }


        public string DeviceId
        {
            get { return deviceId; }
            set { deviceId = value; }
        }

        private Dictionary<string, string> systemProperties = new Dictionary<string, string>();

        public Dictionary<string, string> SystemProperties
        {
            get { return systemProperties; }
            set { systemProperties = value; }
        }

        public SimProperty SimProperty { get => simProperty; set => simProperty = value; }
        private string googleEmail = string.Empty;
        public string GoogleEmail { get => googleEmail; set => googleEmail = value; }
        public string Packages { get; set; }
        public string SerialNo { get; set; }
        public string Imei { get; set; }
        public LicensesModel Licenses { get; set; }

        public DeviceCodeName CodeName { get; set; }
    }
}
