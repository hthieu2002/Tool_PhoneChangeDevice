using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Language
{
    public static class StaticLang
    {
        private static string _deviceCount = "Thiết bị";
        public static string DeviceCount
        {
            get => _deviceCount;
            set
            {
                if (_deviceCount != value)
                {
                    _deviceCount = value;
                    PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DeviceCount)));
                }
            }
        }

        public static event PropertyChangedEventHandler PropertyChanged;
    }

    public static class ViewDeviceLang
    {
        // log
        public static string logPushFile { get; set; } = "Vui lòng chọn ít nhất một thiết bị để push file.";
        public static string logPushFileSuccess { get; set; } = "Đã push file đến các thiết bị.";
        public static string InfoSuccess { get; set; } = "Thành công";
        public static string logInstallAPK { get; set; } = "Vui lòng chọn ít nhất một thiết bị để cài APK.";
        public static string logInstallAPKSuccess { get; set; } = "Đã cài APK cho các thiết bị.";
        public static string logViewDevice { get; set; } = "Vui lòng chọn ít nhất 1 thiết bị để hiển thị scrcpy.";
        public static string InfoViewDevice { get; set; } = "🔄 Đang hiển thị thiết bị";

        // biến

        // function
    }
}
