using POCO.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public static partial class ADBService
    {
        public static void rootAndRemount(string deviceIP)
        {
            runCMDRoot("root", deviceIP);
            runCMDRoot("remount", deviceIP);
        }
        public static void rootAndRemountSystemBin(string deviceId)
        {
            runCMD("root", deviceId);
            runCMD("shell \"mount -o rw,remount /system/bin\"", deviceId);
        }
        public static bool connectDevice(string deviceIP, int port = 5555, bool isRoot = true)
        {
            string adbConnectResponse = runCMD(string.Format("adb connect {0}:{1}", deviceIP, port));
            if (isRoot)
            {
                string adbRootResponse = runCMD("root", deviceIP + ":" + port);
                return adbConnectResponse.Contains(string.Format("connected to {0}:{1}", deviceIP, port)) && !adbRootResponse.Contains("error") && !string.IsNullOrEmpty(adbRootResponse);
            }
            else
            {
                return adbConnectResponse.Contains(string.Format("connected to {0}:{1}", deviceIP, port));
            }
        }
        public static void disconnectDevice(string deviceIP, int port = 5555)
        {
            if (!string.IsNullOrEmpty(deviceIP))
                runCMD(String.Format("adb disconnect {0}:{1}", deviceIP, port));
        }
        public static void pullOrPushFile(FileTransferAction action, string sourcePath, string destinationPath, string deviceIP)
        {
            //runCMD("root", deviceIP);
            //runCMD("remount", deviceIP);
            string fileTransferAction = Enum.GetName(typeof(FileTransferAction), action).ToLower();
            runCMD(string.Format("{0} \"{1}\" \"{2}\"", fileTransferAction, sourcePath, destinationPath), deviceIP);
        }
        public static string getRoot(string prop, string deviceIP)
        {
            return runCMD(string.Format("root", prop), deviceIP).Trim();
        }
        public static void shellRemoveIfContainSpecificText(string filePath, string text, string deviceIP)
        {
            runCMDRoot(string.Format("shell \"sed -i '/{0}/d' {1}\"", text, filePath), deviceIP);
        }
        public static void FakeLocalIP(string deviceIP, string NewIp)
        {

            if (NewIp == "")
            {
                Random random = new Random();
                int randomNewIP = random.Next(2, 254);

                NewIp = $"192.168.1.{randomNewIP}";
            }
            Thread.Sleep(2000);
            string oldIP = GetDeviceIP(deviceIP);
            while (string.IsNullOrEmpty(oldIP))
            {
                oldIP = GetDeviceIP(deviceIP);
            }
            Console.WriteLine(oldIP);
            runCMDRoot("root", deviceIP);
            runCMDRoot("remount", deviceIP);
            Thread.Sleep(2000);
            runCMDRoot(string.Format("shell \"ip addr add {0} dev wlan0\"", NewIp), deviceIP);
            Thread.Sleep(2000);
            runCMDRoot($"shell ip addr del {oldIP} dev wlan0", deviceIP);
            Thread.Sleep(2000);
        }
        public static string GetDeviceIP(string deviceIP)
        {
            // Tạo một đối tượng ProcessStartInfo để thiết lập các thuộc tính cho quá trình
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "adb", // Lệnh cần thực thi (ở đây là adb)
                Arguments = $"-s {deviceIP} shell ip addr show dev wlan0 | grep 'inet ' | awk '{{print $2}}' | cut -d/ -f1", // Tham số lệnh
                RedirectStandardOutput = true, // Chuyển hướng đầu ra tiêu chuẩn để lấy kết quả
                UseShellExecute = false, // Không sử dụng shell thực thi mặc định
                CreateNoWindow = true // Không tạo cửa sổ mới cho quá trình
            };

            // Tạo và bắt đầu quá trình
            using (Process process = Process.Start(psi))
            {
                process.WaitForExit(); // Chờ quá trình hoàn thành
                string output = process.StandardOutput.ReadToEnd(); // Đọc toàn bộ đầu ra của quá trình

                // Loại bỏ ký tự thừa (nếu có) và trả về IP
                return output.Trim();
            }
        }
        public static ProxyResponseModel getDevicePublicIpV4V6(string proxy, string deviceId, string urlToCheck = URL_Data.URL_CHECK_IPv4v6)
        {
            try
            {
                var proxyParams = "";
                if (!string.IsNullOrEmpty(proxy))
                {
                    proxyParams = string.Concat("--proxy http://", proxy);
                }
                var rawResponse = runCMD(string.Format("shell curl -s -S {0} {1}", proxyParams, urlToCheck), deviceId, 10000);
                IPAddress ipAddress;
                if (IPAddress.TryParse(rawResponse, out ipAddress))
                {
                    return new ProxyResponseModel
                    {
                        Address = rawResponse
                    };
                }
                else
                {
                    return new ProxyResponseModel
                    {
                        Address = "",
                        PublicIp = ""
                    };
                }
            }
            catch
            {
                return new ProxyResponseModel
                {
                    Address = "",
                    PublicIp = ""
                };
            }

        }

        public static ProxyResponseModel getDevicePublicIP(string deviceIP, string proxy = "")
        {
            try
            {
                string[] urlToCheck = new string[] { URL_Data.URL_CHECK_IPv4v6_API64, URL_Data.URL_CHECK_IPv4v6, URL_Data.URL_CHECK_IPv4v6_IFCONFIG };
                var proxyParams = "";
                var ipAddress = "";
                var loop = 0;
                if (!string.IsNullOrEmpty(proxy))
                {
                    proxyParams = string.Concat("--proxy http://", proxy);
                }
                while (loop < urlToCheck.Length && string.IsNullOrEmpty(ipAddress))
                {
                    try
                    {
                        var rawResponse = runCMD(string.Format("shell curl -s -S {0} {1}", proxyParams, urlToCheck[loop]), deviceIP, 10000);
                        IPAddress ip;
                        if (IPAddress.TryParse(rawResponse, out ip))
                        {
                            ipAddress = rawResponse;
                            break;
                        }
                    }
                    catch
                    {
                        //ignored
                    }
                    loop++;
                }
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    using (WebConnection webClient = new WebConnection())
                    {
                        webClient.Timeout = 15;
                        var rawResponse2 = webClient.DownloadString(string.Format(URL_Data.URL_CHECK_BY_IP, ipAddress));
                        return JsonService<ProxyResponseModel>.textToJsonObject(rawResponse2);
                    }
                }
                else
                {
                    return new ProxyResponseModel();
                }
            }
            catch
            {
                return new ProxyResponseModel();
            }
        }

        public static void openLink(string url, string deviceIP, string package = "")
        {
            //var tempURL = url.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            //var trueURL = string.Join("\\&", tempURL);
            //runCMD(String.Format("shell am start -a android.intent.action.VIEW -d \"{0}\"", trueURL), deviceIP);
            var acceptedURL = url.Replace("&", "\\&");
            runCMD(String.Format("shell \"am start -a android.intent.action.VIEW -d {0} {1}\"", acceptedURL, package), deviceIP);
        }

        public static string GetCurrentActivity(string deviceIP)
        {
            return runCMD("shell \"dumpsys activity | grep mResumedActivity | tail -1\"", deviceIP);
        }
        public static bool hasComponentInVendingActivity(string component, string deviceIP)
        {
            var response = runCMD(string.Format("shell \"dumpsys activity top | grep {0}\"", component), deviceIP);
            return !string.IsNullOrEmpty(response);
        }
        public static int getSurfaceOrientation(string deviceIP)
        {
            var result = runCMD("shell \"dumpsys input | grep SurfaceOrientation | awk '{print $2}' | head -n 1\"", deviceIP);
            var orientationId = 0;
            int.TryParse(result, out orientationId);
            return orientationId;
        }

        public static void UIDump(string deviceIP)
        {
            runCMD("shell uiautomator dump", deviceIP);
        }
        public static string getUIDump(string deviceIP)
        {
            runCMD("shell uiautomator dump", deviceIP);
            return readFromFile("/sdcard/window_dump.xml", deviceIP);
        }
        public static void shellSleep(int timeSleep, string deviceIP)
        {
            runCMD(string.Format("shell sleep {0}", timeSleep), deviceIP);
        }
        public static void inputTapEvent(int x, int y, string deviceIP)
        {
            runCMD(String.Format("shell input tap {0} {1}", x, y), deviceIP);
        }
        public static void inputTapEvent(Point p, string deviceIP)
        {
            inputTapEvent(p.X, p.Y, deviceIP);
        }

        public static void inputKeyEvent(string code, string deviceIP)
        {
            runCMD(String.Format("shell input keyevent {0}", code.ToUpper()), deviceIP);
        }
        public static void inputKeyEvent(KeyEventCode key, string deviceIP)
        {
            runCMD(String.Format("shell input keyevent {0}", key.ToString().ToUpper()), deviceIP);
        }
        public static void inputTextEvent(string inputText, string deviceIP)
        {
            runCMD(string.Format("shell \"input text '{0}'\"", inputText), deviceIP);
            //foreach (var ch in inputText)
            //{
            //    runCMD(string.Format("shell \"input text '{0}'\"", ch.ToString()), deviceIP);
            //    //Thread.Sleep(100);
            //}
        }
        public static void inputUnicodeTextEvent(string inputText, string deviceIP)
        {
            runCMD("shell ime set com.android.adbkeyboard/.AdbIME", deviceIP); // Switch to adbkeyboard
            Thread.Sleep(200);
            runCMD(string.Format("shell \"am broadcast -a ADB_INPUT_B64 --es msg `echo -n '{0}' | base64`\"", inputText), deviceIP);
            runCMD("shell ime set com.android.inputmethod.latin/.LatinIME", deviceIP); //Switch back to default keyboard
            //runCMD(string.Format("shell \"input text '{0}'\"", inputText), deviceIP);
        }
        public static void inputCharEvent(char text, string deviceIP)
        {
            runCMD(string.Format("shell \"input text '{0}'\"", text), deviceIP);
        }
        public static void inputSwipeEvent(int x1, int y1, int x2, int y2, int speed, string deviceIP)
        {
            runCMD(String.Format("shell input swipe {0} {1} {2} {3} {4}", x1, y1, x2, y2, speed), deviceIP);
        }
        public static void inputLongPressEvent(int x, int y, int speed, string deviceIP)
        {
            runCMD(String.Format("shell input swipe {0} {1} {2} {3} {4}", x, y, x, y, speed), deviceIP);
        }
        public static void openPackage(string package, string deviceIP)
        {
            runCMD(String.Format("shell \"monkey -p {0} -c android.intent.category.LAUNCHER 1\"", package), deviceIP);
        }
        public static string getPackageNameOnScreen(string deviceId)
        {
            string result = runCMD("shell dumpsys activity recents | find \"Recent #0\"", deviceId);
            return result;
        }
        public static void forceStopPackage(string package, string deviceIP)
        {
            runCMD(String.Format("shell \"am force-stop {0}\"", package), deviceIP);
        }
        public static void copyFile(string source, string destination, string deviceIP)
        {
            runCMD(String.Format("shell \"cp -r {0} {1}\"", source, destination), deviceIP);
        }
        public static void moveFile(string source, string destination, string deviceIP)
        {
            runCMD(String.Format("shell \"mv -f {0} {1}\"", source, destination), deviceIP);
        }
        public static void removeFile(string source, string deviceIP)
        {
            runCMD(String.Format("shell \"rm -rf {0}\"", source), deviceIP);
        }
        public static void makeDirIfNotExist(string path, string deviceIP)
        {
            runCMD(String.Format("shell \"mkdir -p '{0}'\"", path), deviceIP);
        }
        public static bool uninstallPackage(string package, string deviceIP)
        {
            return runCMD(String.Format("shell pm uninstall {0}", package), deviceIP).Contains("Success");
        }
        public static bool installAPK(string apkPath, string deviceIP)
        {
            return runCMD(String.Format("install \"{0}\"", apkPath), deviceIP).Contains("Success");
        }

        public static void pushFiles(Dictionary<string, string> files, string deviceId)
        {
            //runCMD("root", deviceId);
            //runCMD("remount", deviceId);
            foreach (var file in files)
            {
                runCMDRoot(String.Format("push \"{0}\" {1} ", file.Key, file.Value), deviceId);

            }
            //runCMD("unroot", deviceId);

        }
        public static string getProp(string prop, string deviceIP)
        {
            return runCMD(string.Format("shell getprop {0}", prop), deviceIP).Trim();
        }
        public static void setPermission(string permission, string fileSystemPath, string deviceId)
        {
            runCMDRoot(String.Format("shell \"chmod {0} {1}\"", permission, fileSystemPath), deviceId);
        }
        public static void stopRedSocks(string deviceId)
        {
            runCMDRoot("shell \"iptables -t nat -F\"", deviceId);
            runCMDRoot("shell \"iptables -t mangle -F\"", deviceId);
            runCMDRoot("shell \"iptables -F\"", deviceId);
            runCMDRoot("shell \"iptables -X\"", deviceId);
            runCMDRoot("shell \"iptables -t nat -X REDSOCKS\"", deviceId);
            runCMDRoot("shell \"iptables -t mangle -X REDSOCKS\"", deviceId);
            runCMDRoot("shell \"killall redsocks\"", deviceId);
            runCMDRoot("shell \"killall redsocks2\"", deviceId);
        }

        public static void enableInternet(string deviceId, bool enabled = true)
        {
            var tag = enabled ? "-D" : "-A";
            runCMD($"shell \"iptables {tag} INPUT -j DROP\"", deviceId);
            runCMD($"shell \"iptables {tag} FORWARD -j DROP\"", deviceId);
            runCMD($"shell \"iptables {tag} OUTPUT -j DROP\"", deviceId);
        }

        public static bool isWifiConnectedV2(string deviceId)
        {
            var rs = runCMDRoot("shell \"dumpsys netstats | grep -E 'iface=wlan.*networkId'\"", deviceId);
            return !string.IsNullOrEmpty(rs.Trim());
        }

        public static void prepareTun2Socks(int tableIpRuleId, string deviceId)
        {
            // Create TUN Interface (make sure root & remount permission granted)
            runCMD("shell \"mkdir /dev/net\"", deviceId);
            runCMD("shell \"mknod /dev/net/tun c 10 200\"", deviceId);
            runCMD("shell \"chmod 0666 /dev/net/tun\"", deviceId);
            runCMD("shell \"ip tuntap add dev tun0 mode tun\"", deviceId);
            runCMD($"shell \"echo '{tableIpRuleId} tun0' >> /data/misc/net/rt_tables\"", deviceId);
            Thread.Sleep(500);
            var hostIP = RandomService.generateGateway();
            var classABC = hostIP.Substring(0, hostIP.LastIndexOf('.') + 1);
            var classD = hostIP.Substring(hostIP.LastIndexOf('.') + 1);
            var tunIP = classABC + RandomService.randomInRangeButExclude(2, 254, int.Parse(classD), int.Parse(classD));
            //var gateway = hostIP.Substring(0, hostIP.LastIndexOf('.') + 1) + "1";
            var subnet = RandomService.generateSubnetMask();

            //Route tun inteface
            runCMD($"shell \"ifconfig tun0 {tunIP} netmask {subnet["mask"]} {tunIP} up\"", deviceId);
            runCMD("shell \"ip route add default dev tun0 proto static scope link table tun0\"", deviceId);
            runCMD($"shell \"ip route add {hostIP}/{subnet["length"]} dev tun0 proto static scope link table tun0\"", deviceId);
            runCMD("shell \"ip route flush cache\"", deviceId);
            Thread.Sleep(500);

            // Config rule tun interface
            runCMD("shell \"ip rule add iif tun0 lookup local_network\"", deviceId);
            runCMD("shell \"ip rule add fwmark 0x0/0x20000 iif lo uidrange 0-99999 lookup tun0\"", deviceId);
            runCMD("shell \"ip rule add fwmark 0xc0068/0xcffff lookup tun0\"", deviceId);
            runCMD("shell \"ip rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-99999 lookup tun0\"", deviceId);
            runCMD("shell \"ip rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-0 lookup tun0\"", deviceId);
            runCMD("shell \"ip rule add iif lo oif tun0 uidrange 0-99999 lookup tun0\"", deviceId);
            Thread.Sleep(500);

            // config iptables
            runCMD("shell \"iptables -t mangle -A routectrl_mangle_INPUT -i tun0 -j MARK --set-xmark 0x30068/0xffefffff\"", deviceId);
            runCMD("shell \"iptables -t nat -A OUTPUT -p udp --dport 53 -j DNAT --to-destination 8.8.8.8:53\"", deviceId);
            runCMD("shell \"iptables -t nat -A PREROUTING -p udp --dport 53 -j DNAT --to-destination 8.8.8.8:53\"", deviceId);
            runCMD("shell \"iptables -t nat -A PREROUTING -p tcp --dport 53 -j DNAT --to-destination 8.8.8.8:53\"", deviceId);
            runCMD("shell \"iptables -t nat -A POSTROUTING -j MASQUERADE\"", deviceId);

            //runCMD("shell \"iptables -t filter -A fw_INPUT -j fw_standby\"", deviceId);
            //runCMD("shell \"iptables -t filter -A fw_OUTPUT -j fw_standby\"", deviceId);
            //runCMD("shell \"iptables -t filter -P INPUT ACCEPT\"", deviceId);
            //runCMD("shell \"iptables -t filter -P FORWARD ACCEPT\"", deviceId);
            //runCMD("shell \"iptables -t filter -P OUTPUT ACCEPT\"", deviceId);
        }

        public static void startTun2Socks(ProxyMode mode, string deviceId, string proxyAddress = "", string user = "", string pass = "", string encryptType = "")
        {
            // Run in detach mode
            switch (mode)
            {
                case ProxyMode.DIRECT:
                    runCMD($"shell \"tun2socks -device tun://tun0 -proxy direct:// -interface wlan0 -mtu 1500 &> /dev/null &\"", deviceId);
                    break;
                case ProxyMode.SHADOWSOCKS:
                    runCMD($"shell \"tun2socks -device tun://tun0 -proxy ss://{encryptType}:{pass}@{proxyAddress} -interface wlan0 -mtu 1500 &> /dev/null &\"", deviceId);
                    break;
                case ProxyMode.SOCKS5:
                    var authen = !string.IsNullOrEmpty(user) ? $"{user}:{pass}@" : "";
                    runCMD($"shell \"tun2socks -device tun://tun0 -proxy socks5://{authen}{proxyAddress} -interface wlan0 -mtu 1500 &> /dev/null &\"", deviceId);
                    break;
                default:
                    break;
            }
        }

        public static void stopTun2Socks(int tableIpRuleId, string deviceId, bool onlyTun2socks = false)
        {
            runCMD($"shell \"kill -9 $(pgrep -f tun2socks)\"", deviceId);
            if (onlyTun2socks) return;
            runCMD($"shell \"ifconfig tun0 down\"", deviceId);
            runCMD($"shell \"ip tuntap del dev tun0 mode tun\"", deviceId);
            runCMD($"shell \"iptables -t nat -F\"", deviceId);
            runCMD($"shell \"iptables -t mangle -F\"", deviceId);
            runCMD($"shell \"ip route flush table {tableIpRuleId}\"", deviceId);
            runCMD($"shell \"ip rule flush\"", deviceId);
        }

        public static void startRedSocks(string proxyHostIP, string redsocksDeviceDir, string deviceId)
        {
            /*runCMD("shell \"iptables -A INPUT -j DROP\"", deviceId);
            runCMD("shell \"iptables -A FORWARD -j DROP\"", deviceId);
            runCMD("shell \"iptables -A OUTPUT -j DROP\"", deviceId);
            Thread.Sleep(3000);
            // wifi on
            ADBService.enableWifi(true, deviceId); //ensure wifi always on*/

            runCMDRoot("shell \"iptables -A INPUT -i ap+ -p tcp --dport 12345 -j ACCEPT\"", deviceId);

            runCMDRoot("shell \"iptables -A INPUT -i lo -p tcp --dport 12345 -j ACCEPT\"", deviceId);

            runCMDRoot("shell \"iptables -A INPUT -p tcp --dport 12345 -j DROP\"", deviceId);

            runCMDRoot("shell \"iptables -t nat -A PREROUTING -i ap+ -p tcp -d 192.168.1.0/24 -j RETURN\"", deviceId);

            runCMDRoot("shell \"iptables -t nat -A PREROUTING -i ap+ -p tcp -j REDIRECT --to 12345\"", deviceId);
            Thread.Sleep(2000);
            runCMDRoot(string.Format("shell \"iptables -t nat -A OUTPUT -p tcp -d {0} -j RETURN\"", proxyHostIP), deviceId);

            runCMDRoot("shell \"iptables -t nat -A OUTPUT -p tcp -j REDIRECT --to 12345\"", deviceId);
            runCMDRoot("shell \"iptables -t mangle -A PREROUTING -i wlan0 -p udp --dport 10000:65535 -j TPROXY --on-port 10053 --tproxy-mark 0x01/0x01\"", deviceId);

            bool isRedsocksStarted = false;
            var maximumRetry = 3;
            var retry = 0;
            while (!isRedsocksStarted && retry <= maximumRetry)
            {
                var resocksStartedResponse = runCMDRoot(String.Format("shell \"{0}/redsocks -c {0}/redsocks.conf\"", redsocksDeviceDir), deviceId);
                Console.WriteLine(resocksStartedResponse);
                Thread.Sleep(3000);
                isRedsocksStarted = resocksStartedResponse.Contains("Address already in use");
                ADBService.enableWifi(true, deviceId);

                /*runCMD("shell \"iptables -D INPUT -j DROP\"", deviceId);
                runCMD("shell \"iptables -D FORWARD -j DROP\"", deviceId);
                runCMD("shell \"iptables -D OUTPUT -j DROP\"", deviceId);*/
                retry++;
                Thread.Sleep(3000); // each retry would delay 0.3 sec
            }
        }

        public static void changeLanguage(string languageCode, string deviceIP)
        {
            // Grant permission fot app
            runCMD(String.Format("shell pm grant {0} {1}", PackageName.ADB_CHANGE_LANGUAGE, ChangeLanguage.PERMISSION), deviceIP);
            //change Language
            var activityName = string.Format("{0}/.AdbChangeLanguage -e language {1}", PackageName.ADB_CHANGE_LANGUAGE, languageCode);
            ADBService.openActivity(activityName, deviceIP);
        }

        public static void resetGAID(string deviceIP)
        {
            ADBService.removeFile(Package_Data.GAID, deviceIP);
            var activityName = string.Format("{0}/.ads.settings.AdsSettingsActivity", PackageName.GMS);
            ADBService.openActivity(activityName, deviceIP);
            Thread.Sleep(2000);
            runCMD(string.Format("shell cat {0}", Package_Data.GAID), deviceIP);
        }

        public static void changeTimezone(string timezone, string deviceIP)
        {
            runCMD(String.Format("shell setprop {0} {1}", BuildKey_SYSTEM.TIMEZONE, timezone), deviceIP);
        }
        public static List<String> getFilesInDir(string path, string deviceId)
        {
            return runCMD(String.Format("shell ls {0}", path), deviceId)
                .Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .ToList<String>();
        }
        public static List<AppData> getCurrentDirInfo(string path, string deviceId)
        {
            //get disk usage current path and sort by name
            string raw = runCMD(String.Format("shell \"du -h -a -d 1 {0} | sort -k2\"", path), deviceId);
            //Split by line
            var listRaw = raw.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
            //modified and parse data to List<>
            var listAppData = new List<AppData>();
            foreach (String line in listRaw)
            {
                var item = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                listAppData.Add(new AppData
                {
                    FileName = item[1].Substring(item[1].LastIndexOf("/") + 1),
                    AbsolutePath = string.Concat(item[1], '/'),
                    Size = item[0]
                }); ;
            }
            //Modified First item for backward
            try
            {
                var backAbsolutePath = listAppData.First().AbsolutePath.TrimEnd('/');
                listAppData.First().AbsolutePath = backAbsolutePath.Substring(0, backAbsolutePath.LastIndexOf("/") + 1);
                listAppData.First().FileName = "...";
                listAppData.First().Size = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return listAppData;
        }
        public static void appendToFiles(string content, string filePath, string deviceIP)
        {
            runCMD(String.Format("shell \"echo '\"{0}\"' >> {1}\"", content, filePath), deviceIP);
        }
        public static void makeAndWriteToFiles(string content, string filePath, string deviceIP)
        {
            runCMDRoot(String.Format("shell \"echo '{0}' > {1}\"", content, filePath), deviceIP);
        }
        public static string readFromFile(string path, string deviceIP)
        {
            return runCMD(String.Format("shell \"cat {0}\"", path), deviceIP);
        }
        public static void clearBrowser(string deviceIP)
        {
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/app_chrome", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/app_download_internal", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/app_tabs", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/app_textures", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/app_textures", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/cache", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/databases", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/secure_connect", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/swecode.bin", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/web_defender", deviceIP);
            runCMD("shell rm -rf /data/data/tugapower.codeaurora.browser/web_refiner", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/app_dex", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/app_chrome", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/app_tabs", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/app_textures", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/cache", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/database", deviceIP);
            runCMD("shell rm -rf /data/data/com.android.chrome/files", deviceIP);
        }
        public static void clearVending(string deviceIP)
        {
            runCMD(string.Format("shell pm clear {0}", PackageName.VENDING), deviceIP);
            runCMD(string.Format("shell pm clear {0}", PackageName.GMS), deviceIP);
            //runCMD(string.Format("shell pm clear {0}", PackageName.GSF), deviceIP);
            runCMD(string.Format("shell pm clear {0}", PackageName.PLAY_GAMES), deviceIP);
            runCMD(string.Format("shell pm clear {0}", PackageName.IMS), deviceIP);
            runCMD(string.Format("shell pm clear {0}", PackageName.PROVIDER_DOWNLOAD), deviceIP);
            runCMD(string.Format("shell pm clear {0}", PackageName.PROVIDER_DOWNLOAD_UI), deviceIP);
            //ADBService.removeFile(Package_Data.VENDING, deviceIP);
            //ADBService.removeFile(Package_Data.GMS, deviceIP);
            //ADBService.removeFile(Package_Data.GSF, deviceIP);
            //ADBService.removeFile(Package_Data.PLAY_GAME, deviceIP);
            //ADBService.removeFile(Package_Data.SYSTEM_SYNC, deviceIP);
            //runCMD("shell rm -rf /data/data/com.android.vending/databases/xternal_referrer_status.db-journal", deviceIP);
            //runCMD("shell rm -rf /data/data/com.android.vending/databases/xternal_referrer_status.db", deviceIP);

        }
        public static void clearSDCard(string deviceIP)
        {
            ADBService.forceStopPackage(PackageName.VENDING, deviceIP);
            runCMD("shell rm -rf ~/sdcard/*", deviceIP);
        }
        public static void clearPackage(string packageName, string deviceIP)
        {
            runCMD(string.Format("shell pm clear {0}", packageName), deviceIP);
        }
        public static string getOwnerIDFile(string path, string deviceIP)
        {
            var response = runCMD(string.Format("shell stat -c '%u%' {0}", path), deviceIP);
            return response.Substring(0, response.LastIndexOf('?'));
        }
        public static void setOwnerIDApp(string ownerID, string packageName, string deviceIP)
        {
            runCMD(string.Format("shell \"chown -R {0}:{1} /data/data/{2}/\"", ownerID, ownerID, packageName), deviceIP);
            runCMD(string.Format("shell \"chown -R {0} /mnt/sdcard/Android/data/{1}/\"", ownerID, packageName), deviceIP);
            runCMD(string.Format("shell \"chown -R {0} /mnt/sdcard/Android/obb/{1}/\"", ownerID, packageName), deviceIP);
        }
        public static void setOwnerFile(string UID, string groupID, string path, string deviceIP)
        {
            runCMD(string.Format("shell \"chown -R {0}:{1} {2}\"", UID, groupID, path), deviceIP);
        }
        public static void createTar(string[] dataFiles, string dstFile, string deviceIP)
        {
            var srcCapsulate = string.Join(" ", dataFiles);
            runCMD(string.Format("shell \"tar -cvf {0} {1}\"", dstFile, srcCapsulate), deviceIP);
        }
        public static void extractTar(string tarPath, string deviceIP)
        {
            runCMD(string.Format("shell \"tar -xvf {0}\"", tarPath), deviceIP);
        }
        public static string getSizeOfFile(string path, string deviceIP)
        {
            var raw = runCMD(string.Format("shell \"du -sh {0}\"", path), deviceIP);
            return raw.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }
        public static float getBatteryTemp(string deviceIP)
        {
            var value = runCMD("shell \"dumpsys battery | grep temperature\"", deviceIP);
            var rawTemp = value.Substring(value.IndexOf(':') + 1).Trim();
            return string.IsNullOrEmpty(rawTemp) ? 0f : float.Parse(rawTemp) / 10;
        }
        public static void setBrightness(decimal value, string deviceIP)
        {
            var realValue = Convert.ToInt32((float)value * 2.55);
            runCMD(string.Format("shell \"echo {0} > /sys/class/leds/lcd-backlight/brightness\"", realValue), deviceIP);

        }
        public static void turnOnOffScreen(bool isOn, string deviceIP)
        {
            var isAwake = int.Parse(runCMD(string.Format("shell \"cat /sys/class/leds/lcd-backlight/brightness\""), deviceIP)) != 0;
            if (isOn && !isAwake)
            {
                setBrightness(10, deviceIP);
            }
            else if (!isOn && isAwake)
            {
                setBrightness(0, deviceIP);
            }
        }
        public static string sqlite3Query(string pathDB, string query, string deviceIP)
        {
            return runCMD(string.Format("shell \"sqlite3 {0} \\\"{1}\\\"\"", pathDB, query), deviceIP);
        }
        public static bool hasPackageInVendingDB(string pkg, string deviceIP)
        {
            var query = string.Format("select count(*) pk from external_referrer_status_store where pk='{0}'", pkg);
            var result = sqlite3Query(Package_Data.VENDING_DB_PKG, query, deviceIP);
            return !string.IsNullOrEmpty(result) && int.Parse(result) > 0;
        }
        public static void backUpOfferGMS(string zipPath, string deviceId)
        {
            var zipAllPackagesCommand = string.Format("shell \"tar -cvzf ~/{0} ~/data/system/sync ~/data/system_ce ~/data/system_de\"", zipPath);
            runCMD(zipAllPackagesCommand, deviceId);
        }
        public static void restoreOfferGMS(string unzipPath, string deviceId)
        {
            var unZipAllPackagesCommand = string.Format("shell \"tar -xvzf {0}\"", unzipPath);
            runCMD(unZipAllPackagesCommand, deviceId);
        }
        //public static void cleanOfferGMSPackagesAndAccounts(string deviceId)
        //{
        //    runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.SYSTEM_SYNC), deviceId);
        //    runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.SYSTEM_CE), deviceId);
        //    runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.SYSTEM_DE), deviceId);
        //    runCMD(String.Format("shell pm clear {0}", "com.google.android.gms"), deviceId);
        //    runCMD(String.Format("shell pm clear {0}", "com.google.android.gsf"), deviceId);
        //    runCMD(String.Format("shell pm clear {0}", "com.android.vending"), deviceId);
        //    runCMD(String.Format("shell pm clear {0}", "com.google.android.gm"), deviceId);
        //}
        public static void openActivity(string activity, string deviceId)
        {
            runCMD(string.Format("shell am start -n {0}", activity), deviceId);
        }
        public static void openClassName(string className, string deviceId)
        {
            runCMD(string.Format("shell am start -a {0}", className), deviceId);
        }
        private static string getContentPackageXML(string package, string deviceId)
        {
            var exec = string.Format("{0} | grep '<package name=\\\"{1}\\\"'", Package_Data.PACKAGE_SYSTEM_XML, package);
            var rawResponse = ADBService.readFromFile(exec, deviceId);
            return string.Concat(rawResponse, "</package>");
        }
        public static string getPackageCodePathInXML(string package, string deviceId)
        {
            var response = getContentPackageXML(package, deviceId);
            return XmlService.getAttributePackagesXML("codePath", response);
        }
        public static string getPackageOwnerIDInXML(string package, string deviceIP)
        {
            var response = getContentPackageXML(package, deviceIP);
            return XmlService.getAttributePackagesXML("userId", response);
        }
        public static string getCurrentPackageCodePath(string package, string deviceId)
        {
            try
            {
                var apkPath = runCMD(string.Format("shell \"pm path {0} | head -n 1\"", package), deviceId);
                var indexFirstSlash = apkPath.IndexOf('/');
                var indexLastSlash = apkPath.LastIndexOf('/');
                return apkPath.Substring(indexFirstSlash, indexLastSlash - indexFirstSlash);
            }
            catch
            {
                return "";
            }
        }
        public static string getLocaleCountryCode(string deviceIP)
        {
            var locale = getProp(BuildKey_SYSTEM.LOCALE, deviceIP);
            return locale.Substring(locale.IndexOf('-') + 1).Trim();
        }
        public static void shellStringReplace(string filePath, string oldStr, string newStr, string deviceIP)
        {
            runCMD(string.Format("shell \"sed -i 's/{0}/{1}/g' {2}\"", oldStr, newStr, filePath), deviceIP);
        }
        public static string getGSF_ID(string deviceIP)
        {
            var query = "select value from main where name='android_id'";
            return sqlite3Query(Package_Data.GSERVICES_DB, query, deviceIP).Trim();
        }
        public static void fakeGSF_ID(string oldID, string newID, string deviceIP)
        {
            var updateQuery = string.Format("update main set value='{0}' where name='android_id'", newID);
            removeFile(Package_Data.GSERVICES_DB_WAL, deviceIP);
            removeFile(Package_Data.GSERVICES_DB_SHM, deviceIP);
            sqlite3Query(Package_Data.GSERVICES_DB, updateQuery, deviceIP);
            shellStringReplace(Package_Data.GMS_CHECKIN_ID, oldID, newID, deviceIP);
            shellStringReplace(Package_Data.GMS_CHECKIN_ID_TOKEN, oldID, newID, deviceIP);
        }
        public static string screenCapture(string deviceId, string additionalName = "")
        {
            var screenPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "capture");
            if (!Directory.Exists(screenPath))
            {
                Directory.CreateDirectory(screenPath);
            }
            runCMD("shell screencap /mnt/sdcard/screencap.png", deviceId);
            runCMD(string.Format("pull /mnt/sdcard/screencap.png {0}\\screencap{1}{2}.png", screenPath, deviceId, additionalName), deviceId);
            removeFile("/mnt/sdcard/screencap.png", deviceId);

            return $"{screenPath}\\screencap{deviceId}{additionalName}.png";
        }
        public static string GetMediaSession(string deviceId)
        {
            var mediaSession = runCMD("shell dumpsys media_session", deviceId);
            if (!String.IsNullOrEmpty(mediaSession))
            {
                return mediaSession;
            }

            return String.Empty;
        }
    }
    internal class WebConnection : WebClient
    {
        internal int Timeout { get; set; }
        protected override WebRequest GetWebRequest(Uri Address)
        {
            WebRequest WebReq = base.GetWebRequest(Address);
            WebReq.Timeout = Timeout * 1000; // Seconds
            return WebReq;
        }
    }
}