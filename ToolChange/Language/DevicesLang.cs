using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Language
{
    public static class DevicesLang
    {
        // log 
        public static string logTitleProxy { get; set; } = "Start fake proxy ..";
        public static string logTitleProxySuccess { get; set; } = "Fake proxy success..";
        public static string logCheckProxy { get; set; } = "Check fake proxy in browserleaks ... ";
        public static string logDeviceRandomEx { get; set; } = "Devices not existed, please try again";
        public static string logSelectDeviceChange { get; set; } = "No devices selected.";
        public static string logChangeDevice { get; set; } = "Are you sure to proceed with these changes and reboot ?";
        public static string logChangeDeviceKeyBox { get; set; } = "Are you push file keybox.xml to phone ?";
        public static string logChangeDevicePif { get; set; } = "Are you push file pif.json to phone ?";
        public static string logErrorExChangeDevice { get; set; } = "This selected device cannot be changed, please check your rom and developer setting and try loading again.";
        public static string logErrorExTitleChangeDevice { get; set; } = "Device Error";
        // biến 
        public static string TitleDetailDevice { get; set; } = "Thông tin thiết bị";
        public static string TitleProxy { get; set; } = "Nhập proxy cho thiết bị ";
        public static string TitleProxyId { get; set; } = "có id";
        public static string TitleLocation { get; set; } = "Fake location";
        public static string TitleUrl { get; set; } = "Nhập url";

        //function
        public static string GetTitieProxy(string deviceName, string deviceId)
        {
            return $"{TitleProxy} {deviceName} {TitleProxyId} {deviceId}";
        }
        
    }
}
