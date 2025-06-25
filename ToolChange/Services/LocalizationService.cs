using Amazon.Runtime.Internal.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Services
{
    public static class LocalizationService
    {
        private static string _currentLanguage = Properties.Settings.Default.lang;
        private static string _currentTheme = Properties.Settings.Default.theme;

        // Dictionary<language, Dictionary<key, value>>
        private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        {
            "vi", new Dictionary<string, string>
            {
                { "Device", "Thiết bị" },
                { "Automation", "Tự động hóa" },
                { "Screen", "Xem màn hình" },
                { "langCheckBoxAutomation", "Vô hạn"},
                { "ControlClickXY", "Nhấn X, Y"},
                { "ControlSwipe", "Vuốt"},
                { "ControlRandomClick", "Click ngẫu nhiên"},
                { "ControlWait","Đợi"},
                { "TitleClickControl", "Click tọa độ"},
                { "TitleClick2Control", "Tìm text click"},
                { "ControlSearchAndClick", "Tìm đúng click"},
                { "ControlSearchWaitClick","Tìm đúng đợi" },
                { "ControlSearchAndContinue","Tìm đúng tiếp tục" },
                { "TitleClick3Control","Tìm text trong ảnh" },
                { "ControlFindAndClick", "Tìm đúng click" },
                { "ControlFindWaitClick","Tìm đúng đợi"},
                { "ControlFindAndContinue", "Tìm đúng tiếp tục" },
                { "TitleClick4Control","Xử lý logic" },
                { "TitleTextControl", "Xử lý văn bản" },
                { "ControlSendText" , "Gửi văn bản"},
                { "ControlSendTextFromFileDel", "Gửi văn bản từ file và xóa" },
                { "ControlSendTextRandomFromFile", "Gửi văn bản từ nguồn" },
                { "ControlRandomTextAndSend", "Gửi văn bản ngẫu nhiên" },
                { "ControlDelTextChar", "Xóa 1 ký tự" },
                { "ControlDelAllText", "Xóa toàn bộ" },
                { "ControlSendKey" , "Nhấn phím"},
                { "ControlCtrlA" , "Ctrl + A"},
                { "ControlCheckKeyBoard" , "Kiểm tra bàn phím"},
                { "ControlURLKEYSendText", "Danh sách key phím" },
                { "TitileDataChangeInfo" , "Xử lý hành động trên điện thoại"},
                { "ControlDataChangeInfo1", "Sao lưu" },
                { "ControlDataChangeInfo2", "Khôi phục" },
                { "ControlDataChangeInfo3", "Đăng nhập gmail" },
                { "ControlDataChangeInfo4", "Mở Chplay" },
                { "ControlDataChangeInfo5", "Thay đổi thông tin" },
                { "ControlDataChangeInfo6", "Thay đổi sim" },
                { "ControlDataChangeInfo7", "Xóa tài khoản" },
                { "ControlWaitReboot", "Chờ khởi động lại" },
                { "ControlWaitInternet", "Chờ kết nối internet" },
                { "ControlPushFile", "Gửi tệp vào phone" },
                { "ControlPullFile", "Chuyển tệp về pc" },
                { "TitleGeneral", "Gói ứng dụng" },
                { "ControlWiFiON", "Bật wifi" },
                { "ControlWiFiOFF", "Tắt wifi" },
                { "ControlOpenURL", "Mở Url" },
                { "ControlRunCommandShell", "Lệnh (shell)" },
                { "ControlOpenApp", "Mở ứng dụng" },
                { "ControlCloseApp", "Đóng ứng dụng" },
                { "ControlEnableApp", "Bật ứng dụng" },
                { "ControlDisbledApp", "Tắt ứng dụng" },
                { "ControlInstallApp", "Cài đặt ứng dụng" },
                { "ControlUninstallApp", "Gỡ cài đặt" },
                { "ControlClearDataApp", "Xóa dữ liệu" },
                { "ControlSwipeCloseApp", "Vuốt đóng app" },
                { "ControlLoadApp", "Tải ứng dụng" },
                { "ControlDeviceMenuDropBox1", "Sao chép ID" },
                { "ControlDeviceMenuDropBox2", "Chi tiết device" },
                { "ControlDeviceMenuDropBox3", "Đổi tên" },
                { "ControlDeviceMenuDropBox4", "Fake proxy" },
                { "ControlDeviceMenuDropBox5", "Xóa" },
                { "LableViewDevice1", "Độ phân giải (sắc nét)" },
                { "LableViewDevice2", "Tỉ lệ khung hình(%)" },
                { "ControlCheckScreen", "Tắt màn hình khi xem" },
                { "LabelNumberDevice", "Số thiết bị được chọn" },
                { "Setting", "Cài đặt" },
                { "Document", "Hướng dẫn" },
                { "LogInfomation", "Thông tin" },
                { "LogError", "Lỗi" },
                { "LogWarning", "Cảnh báo" },
                { "logTitleProxy", "Bắt đầu fake proxy .." },
                { "logTitleProxySuccess", "Fake proxy thành công" },
                { "logCheckProxy", "Kiểm tra proxy trên web browserleaks" },
                { "logDeviceRandomEx", "Thiết bị không tồn tại, vui lòng thử lại" },
                { "logSelectDeviceChange", "Không có thiết bị được chọn" },
                { "logChangeDevice", "Bạn có chắc chắn muốn tiếp tục với những thay đổi này và khởi động lại không?" },
                { "logChangeDeviceKeyBox", "Bạn có muốn push file Keybox.xml vào điện thoại không?" },
                { "logChangeDevicePif", "Bạn có muốn push file Pif.json vào điện thoại không?" },
                { "logErrorExChangeDevice", "Thiết bị đã chọn này không thể thay đổi, vui lòng kiểm tra ROM và cài đặt nhà phát triển của bạn và thử tải lại." },
                { "logErrorExTitleChangeDevice", "Thiết bị lỗi" },
                { "TitleDetailDevice", "Thông tin thiết bị " },
                { "TitleProxy", "Nhập proxy cho thiết bị " },
                { "TitleProxyId", " có id " },
                { "TitleLocation", "Fake Location" },
                { "TitleUrl", "Nhập Url" },
                { "DeviceCount", "Thiết bị" },
                { "logPushFile", "Vui lòng chọn ít nhất một thiết bị để push file." },
                { "logPushFileSuccess", "Đã push file đến các thiết bị." },
                { "InfoSuccess", "Thành công" },
                { "logInstallAPK", "Vui lòng chọn ít nhất một thiết bị để cài APK." },
                { "logInstallAPKSuccess", "Đã cài APK cho các thiết bị." },
                { "logViewDevice", "Vui lòng chọn ít nhất 1 thiết bị để hiển thị scrcpy." },
                { "InfoViewDevice", "🔄 Đang hiển thị thiết bị" },
            }
        },
        {
          "en", new Dictionary<string, string>
{
    { "Device", "Device" },
    { "Automation", "Automation" },
    { "Screen", "Screen" },
    { "langCheckBoxAutomation", "Unlimited" },
    { "ControlClickXY", "Click X, Y" },
    { "ControlSwipe", "Swipe" },
    { "ControlRandomClick", "Random Click" },
    { "ControlWait", "Wait" },
    { "TitleClickControl", "Click Coordinates" },
    { "TitleClick2Control", "Find Text Click" },
    { "ControlSearchAndClick", "Search and Click" },
    { "ControlSearchWaitClick", "Search and Wait" },
    { "ControlSearchAndContinue", "Search and Continue" },
    { "TitleClick3Control", "Find Text in Image" },
    { "ControlFindAndClick", "Find and Click" },
    { "ControlFindWaitClick", "Find and Wait" },
    { "ControlFindAndContinue", "Find and Continue" },
    { "TitleClick4Control", "Logic Processing" },
    { "TitleTextControl", "Text Processing" },
    { "ControlSendText", "Send Text" },
    { "ControlSendTextFromFileDel", "Send Text from File and Delete" },
    { "ControlSendTextRandomFromFile", "Send Text from Source" },
    { "ControlRandomTextAndSend", "Send Random Text" },
    { "ControlDelTextChar", "Delete One Character" },
    { "ControlDelAllText", "Delete All" },
    { "ControlSendKey", "Press Key" },
    { "ControlCtrlA", "Ctrl + A" },
    { "ControlCheckKeyBoard", "Check Keyboard" },
    { "ControlURLKEYSendText", "Key List" },
    { "TitileDataChangeInfo", "Phone Action Processing" },
    { "ControlDataChangeInfo1", "Backup" },
    { "ControlDataChangeInfo2", "Restore" },
    { "ControlDataChangeInfo3", "Login to Gmail" },
    { "ControlDataChangeInfo4", "Open Play Store" },
    { "ControlDataChangeInfo5", "Change Information" },
    { "ControlDataChangeInfo6", "Change SIM" },
    { "ControlDataChangeInfo7", "Delete Account" },
    { "ControlWaitReboot", "Wait for Reboot" },
    { "ControlWaitInternet", "Wait for Internet" },
    { "ControlPushFile", "Push File to Phone" },
    { "ControlPullFile", "Pull File to PC" },
    { "TitleGeneral", "Application Package" },
    { "ControlWiFiON", "Turn WiFi On" },
    { "ControlWiFiOFF", "Turn WiFi Off" },
    { "ControlOpenURL", "Open URL" },
    { "ControlRunCommandShell", "Shell Command" },
    { "ControlOpenApp", "Open App" },
    { "ControlCloseApp", "Close App" },
    { "ControlEnableApp", "Enable App" },
    { "ControlDisbledApp", "Disable App" },
    { "ControlInstallApp", "Install App" },
    { "ControlUninstallApp", "Uninstall App" },
    { "ControlClearDataApp", "Clear Data" },
    { "ControlSwipeCloseApp", "Swipe to Close App" },
    { "ControlLoadApp", "Load App" },
    { "ControlDeviceMenuDropBox1", "Copy ID" },
    { "ControlDeviceMenuDropBox2", "Device Details" },
    { "ControlDeviceMenuDropBox3", "Rename" },
    { "ControlDeviceMenuDropBox4", "Fake Proxy" },
    { "ControlDeviceMenuDropBox5", "Delete" },
    { "LableViewDevice1", "Resolution (Sharpness)" },
    { "LableViewDevice2", "Frame Rate (%)" },
    { "ControlCheckScreen", "Turn Off Screen While Viewing" },
    { "LabelNumberDevice", "Number of Selected Devices" },
    { "Setting", "Settings" },
    { "Document", "Documents" },
    { "LogInfomation", "Information" },
    { "LogError", "Error" },
    { "LogWarning", "Warning" },
    { "logTitleProxy", "Starting proxy spoofing..." },
    { "logTitleProxySuccess", "Proxy spoofing successful" },
    { "logCheckProxy", "Checking proxy on browserleaks website" },
    { "logDeviceRandomEx", "Device does not exist, please try again" },
    { "logSelectDeviceChange", "No device selected" },
    { "logChangeDevice", "Are you sure you want to proceed with these changes and restart?" },
    { "logChangeDeviceKeyBox", "Do you want to push the Keybox.xml file to the phone?" },
    { "logChangeDevicePif", "Do you want to push the Pif.json file to the phone?" },
    { "logErrorExChangeDevice", "The selected device cannot be changed. Please check your ROM and developer settings, then try reloading." },
    { "logErrorExTitleChangeDevice", "Device Error" },
    { "TitleDetailDevice", "Device Information" },
    { "TitleProxy", "Enter proxy for device" },
    { "TitleProxyId", " with ID " },
    { "TitleLocation", "Fake Location" },
    { "TitleUrl", "Enter URL" },
    { "DeviceCount", "Devices" },
    { "logPushFile", "Please select at least one device to push the file." },
    { "logPushFileSuccess", "File has been pushed to the devices." },
    { "InfoSuccess", "Success" },
    { "logInstallAPK", "Please select at least one device to install the APK." },
    { "logInstallAPKSuccess", "APK has been installed on the devices." },
    { "logViewDevice", "Please select at least one device to display with scrcpy." },
    { "InfoViewDevice", "🔄 Displaying device" },

    }
        }
    };

        public static void SetLanguage(string language)
        {
            Properties.Settings.Default.lang = language;
            Properties.Settings.Default.Save();

            if (_translations.ContainsKey(language))
                _currentLanguage = language;
        }

        public static string Get(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var langDict)
                && langDict.TryGetValue(key, out var value))
            {
                return value;
            }

            return key;
        }
    }
}
