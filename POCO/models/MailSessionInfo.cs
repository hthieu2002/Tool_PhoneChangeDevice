namespace POCO.Models
{
    public class MailSessionInfo
    {
        public string DeviceId { get; set; }
        public string VerifyStatus { get; set; } = "UNDEFINED";
        public string ExceptionMessage { get; set; }
        public int IndexOfRetrying { get; set; }
        public string DeviceSourceInfo { get; set; }
        public string DeviceFingerPrint { get; set; }
        public string Proxy { get; set; }
        public string ProxyType { get; set; }
        public string MailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DeviceRomName { get; set; }
        public string DeviceName { get; set; }
    }
}
