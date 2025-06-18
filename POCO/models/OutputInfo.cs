using System;

namespace POCO.Models
{
    public class OutputInfo
    {
        public long StartSessionDateTimeUTC { get; set; }
        public DateTime StartSingleTime { get; set; } = DateTime.Now;
        public DateTime EndSingleTime { get; set; } = DateTime.Now;
        public string Email { get; set; }
        public string RecoverEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int TotalSingleTimeInSeconds { get => (int)(EndSingleTime - StartSingleTime).TotalSeconds; }
        public string ProxyPublicIP { get; set; }
        public string CountryCode { get; set; }
        public string ProxyHostFull { get; set; }
        public string DeviceFingerPrint { get; set; }
        public string DeviceSourceInfo { get; set; }
        public string DeviceMacAddress { get; set; }
        public string Rom { get; set; } = "";
        public string WebViewVersion { get; set; } = "";
        public string GmsVersion { get; set; } = "";
        public string DeviceId { get; set; }
        public string ExceptionMessage { get; set; }
        public string VerifyStatus { get; set; } = "UNDEFINED";
        public int IndexOfRetrying { get; set; }
        public string VideoLink { get; set; }
        public int VideoDuration { get; set; }
        public bool Liked { get; set; }
        public bool Subscribed { get; set; }
        public int TotalSkipAds { get; set; } = 0;
    }
}
