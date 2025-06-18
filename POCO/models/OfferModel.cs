using Newtonsoft.Json;
namespace POCO.Models
{
    public class Offer
    {
        private string name;
        private string url;
        private string package;
        private ScriptOffer script;
        private string serialno;
        private string note;
        private string status = SessionStatus.Ready.ToString();
        private OfferSetting settings;
        [JsonProperty("script")]
        public ScriptOffer Script { get => script; set => script = value; }
        [JsonProperty("package")]
        public string Package { get => package; set => package = value; }
        [JsonProperty("url")]
        public string Url { get => url; set => url = value; }
        [JsonProperty("name")]
        public string Name { get => name; set => name = value; }
        [JsonProperty("settings")]
        public OfferSetting Settings { get => settings; set => settings = value; }
        [JsonProperty("serialno")]
        public string Serialno { get => serialno; set => serialno = value; }
        [JsonProperty("note")]
        public string Note { get => note; set => note = value; }
        public string Status { get => status; set => status = value; }
    }

    public class OfferSetting
    {
        private int delayBeforeOpenAppInSecondFrom;
        private int delayBeforeOpenAppInSecondTo;
        private bool testoffer;
        public int OldDeviceRatio { get; set; }
        public int DelayBeforeOpenAppInSecondFrom { get => delayBeforeOpenAppInSecondFrom; set => delayBeforeOpenAppInSecondFrom = value; }
        public int DelayBeforeOpenAppInSecondTo { get => delayBeforeOpenAppInSecondTo; set => delayBeforeOpenAppInSecondTo = value; }
        public bool TestOffer { get => testoffer; set => testoffer = value; }
    }
    public class ScriptOffer
    {
        private string namePackage;
        private string fullPath;
        public string NamePackage { get => namePackage; set => namePackage = value; }
        public string FullPath { get => fullPath; set => fullPath = value; }
    }
}
