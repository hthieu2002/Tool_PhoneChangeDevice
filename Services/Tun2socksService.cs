using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    [Obfuscation(Exclude = false)]
    public class Tun2socksService
    {
        public static void setUpTun2socksOnDevice(string tun2socksDir, string deviceId, string tun2socksTableId = "9988", string tun2socksInterface = "tun0")
        {
            Directory.CreateDirectory(deviceId);
            Dictionary<string, string> files = new Dictionary<string, string>();
            files.Add(string.Concat(Directory.GetCurrentDirectory(), @"\Resources\tun2socks"), tun2socksDir);
            ADBService.pushFiles(files, deviceId);
            ADBService.setPermission("777", tun2socksDir + "/tun2socks", deviceId);
            ADBService.runCMDRoot($"shell \"echo {tun2socksTableId} > /data/local/tmp/tun2socksTableId.txt\"", deviceId);
            ADBService.runCMDRoot($"shell \"echo {tun2socksInterface} > /data/local/tmp/tun2socksInterface.txt\"", deviceId);
        }

        public static void start(string tun2socksDir, string proxyParams, string ipProxyV4, string deviceId,
                                 string tun2socksTableId = "9988", string tun2socksInterface = "tun0", string localWifiInterface = "wlan0")
        {
            if(string.IsNullOrEmpty(ipProxyV4))
            {
                ipProxyV4 = randomLocalIPv4ForTun2socks();
            }
            setUpTun2socksInterface(deviceId, ipProxyV4, tun2socksTableId, tun2socksInterface);
            Thread.Sleep(1000);
            configIptablesTun2socks(deviceId, tun2socksInterface, localWifiInterface);
            Thread.Sleep(1000);
            configureSysctlForTun(deviceId, tun2socksInterface, localWifiInterface);
            Thread.Sleep(1000);
            startTun2socks(proxyParams, tun2socksDir, deviceId, tun2socksInterface, localWifiInterface);
        }

        /**
         * Dừng toàn bộ tiến trình tun2socks và hạ interface TUN hiện tại.
         *
         * @param deviceId     ID của thiết bị được sử dụng cho các lệnh ADB.
         * @param tun2socksTableId  (tuỳ chọn) id của tun2socks, mặc định là "9988".
         * @param tun2socksInterface  (tuỳ chọn) interface name của tun2socks, mặc định là "9988".
         *
         * Lưu ý:
         * - Khi gọi hàm này từ bên ngoài, bạn nên kiểm tra xem file `/data/local/tmp/tun2socksTableId.txt`
         *   và `/data/local/tmp/tun2socksInterface.txt`
         *   đã tồn tại và có giá trị phù hợp hay chưa.
         * - Có thể đọc giá trị thực tế bằng lệnh:
         *     string tun2socksTableId = ADBService.readFromFile("/data/local/tmp/tun2socksTableId.txt", deviceId);
         *     string tun2socksInterface = ADBService.readFromFile("/data/local/tmp/tun2socksInterface.txt", deviceId);
         * - Nếu file tồn tại và chứa giá trị khác "9988" và "tun0", hãy truyền giá trị đó vào `stop()`
         *   để đảm bảo xóa đúng bảng định tuyến tương ứng.
         *
         * Ví dụ:
         *     string tun2socksTableId = ADBService.readFromFile("/data/local/tmp/tun2socksTableId.txt", deviceId);
         *     string tun2socksTableId = ADBService.readFromFile("/data/local/tmp/tun2socksInterface.txt", deviceId);
         *     Tun2socksService.stop(deviceId, tun2socksTableId, tun2socksInterface);
         */
        public static void stop(string deviceId, string tun2socksTableId = "9988", string tun2socksInterface = "tun0", string localWifiInterface = "wlan0")
        {
            stopTun2Socks(deviceId, tun2socksTableId, tun2socksInterface, localWifiInterface);
        }

        private static void setUpTun2socksInterface(string deviceId, string ipProxyV4, string tun2socksTableId, string tun2socksInterface)
        {
            ADBService.runCMDRoot("shell \"mkdir /dev/net\"", deviceId);
            ADBService.runCMDRoot("shell \"mknod /dev/net/tun c 10 200\"", deviceId);
            ADBService.runCMDRoot("shell \"chmod 0666 /dev/net/tun\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip tuntap add dev {tun2socksInterface} mode tun\"", deviceId);
            ADBService.runCMDRoot($"shell \"echo '{tun2socksTableId} {tun2socksInterface}' >> /data/misc/net/rt_tables\"", deviceId);
            Thread.Sleep(1000);
            //ADBService.runCMDRoot($"shell \"ifconfig {tun2socksInterface} {ipProxyV4} pointopoint {randomLocalIPv4ForTun2socks()} netmask 255.255.0.0 up\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip addr add {ipProxyV4}/16 peer {randomLocalIPv4ForTun2socks()} dev {tun2socksInterface}\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell \"ip route add default dev {tun2socksInterface} table {tun2socksTableId}\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip rule add from all lookup {tun2socksTableId} pref 100\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell \"ip -6 route add default dev {tun2socksInterface} table {tun2socksTableId}\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip -6 rule add from all lookup {tun2socksTableId} pref 100\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot("shell \"ip route del default\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip route add default dev {tun2socksInterface}\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip link set {tun2socksInterface} up\"", deviceId);
            ADBService.runCMDRoot("shell \"ip route flush cache\"", deviceId);
        }

        private static void configureSysctlForTun(string deviceId, string tun2socksInterface, string localWifiInterface)
        {
            ADBService.runCMDRoot("shell \"sysctl -w net.ipv4.conf.all.rp_filter=0\"", deviceId);
            ADBService.runCMDRoot($"shell \"sysctl -w net.ipv4.conf.{localWifiInterface}.rp_filter=0\"", deviceId);
            ADBService.runCMDRoot($"shell \"sysctl -w net.ipv6.conf.{tun2socksInterface}.disable_ipv6=0\"", deviceId);
            ADBService.runCMDRoot("shell \"sysctl -w net.ipv6.conf.all.forwarding=1\"", deviceId);
            ADBService.runCMDRoot("shell \"sysctl -w net.ipv6.conf.default.forwarding=1\"", deviceId);
        }

        private static string randomLocalIPv4ForTun2socks()
        {
            var rnd = new Random();
            int secondOctet = rnd.Next(1, 255);
            int thirdOctet = rnd.Next(1, 255);

            return $"192.168.{secondOctet}.{thirdOctet}";
        }

        private static void configIptablesTun2socks(string deviceId, string tun2socksInterface, string localWifiInterface)
        {
            ADBService.runCMDRoot($"shell \"iptables -t nat -A PREROUTING -i {localWifiInterface} -p udp --dport 53 -j DNAT --to 8.8.8.8:53\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t nat -A POSTROUTING -p udp -d 8.8.8.8 --dport 53 -o {tun2socksInterface} -j MASQUERADE\"", deviceId);
        }

        private static void startTun2socks(string proxyParams, string tun2socksDir, string deviceId, string tun2socksInterface, string localWifiInterface)
        {
            ADBService.runCMDRoot($"shell \"nohup {tun2socksDir}/tun2socks -device tun://{tun2socksInterface} -proxy {proxyParams} -interface {localWifiInterface} -mtu 1500 &> /dev/null &\"", deviceId);
        }

        private static void stopTun2Socks(string deviceId, string tun2socksTableId, string tun2socksInterface, string localWifiInterface)
        {
            ADBService.runCMDRoot("shell \"killall -q tun2socks\"", deviceId);
            ADBService.runCMDRoot("shell \"pkill -9 tun2socks\"", deviceId);
            ADBService.runCMDRoot($"shell \"ifconfig {tun2socksInterface} down\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip tuntap del dev {tun2socksInterface} mode tun\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip link delete {tun2socksInterface}\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell \"ip route del add default dev {tun2socksInterface} table {tun2socksTableId}\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip rule del add from all lookup {tun2socksTableId} pref 100\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip -6 route del add default dev {tun2socksInterface} table {tun2socksTableId}\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip -6 rule del add from all lookup {tun2socksTableId} pref 200\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip route del default\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip route add default dev {localWifiInterface}\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip route flush cache\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"iptables -t nat -F\"", deviceId);
            ADBService.runCMDRoot($"shell  \"iptables -t mangle -F\"", deviceId);
            ADBService.runCMDRoot($"shell  \"iptables -F\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"sed -i '/{tun2socksTableId} {tun2socksInterface}/d' /data/misc/net/rt_tables\"", deviceId);
            ADBService.runCMDRoot($"shell  \"rm -rf /dev/net/tun\"", deviceId);
        }

        public static string getIpv4SocksProxy(string proxy, string deviceId)
        {
            try
            {
                ADBService.enableWifi(false, deviceId);
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);
                if (proxyParts.Length == 4)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} --proxy-user {proxyParts[2]}:{proxyParts[3]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    string ipV4 = getIpv4(commandline); 
                    if (!string.IsNullOrEmpty(ipV4))
                    {
                        return ipV4;
                    }
                }
                else if (proxyParts.Length == 2)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    string ipV4 = getIpv4(commandline);
                    if (!string.IsNullOrEmpty(ipV4))
                    {
                        return ipV4;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string getIpv4HttpProxy(string proxy, string deviceId)
        {
            try
            {
                ADBService.enableWifi(false, deviceId);
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);

                string commandline;
                string str;

                if (proxyParts.Length == 4)
                {
                    commandline = $"curl --proxy http://{proxyParts[2]}:{proxyParts[3]}@{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                    str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    string ipV4 = getIpv4(commandline);
                    if (!string.IsNullOrEmpty(ipV4))
                    {
                        return ipV4;
                    }
                }
                else if (proxyParts.Length == 2)
                {
                    commandline = $"curl --proxy http://{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                    string ipV4 = getIpv4(commandline);
                    if (!string.IsNullOrEmpty(ipV4))
                    {
                        return ipV4;
                    }
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string getIpv4(string commandline) 
        {
            string str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
            if (!string.IsNullOrEmpty(str))
            {
                JObject jsonObject = JObject.Parse(str);
                return jsonObject["query"]?.ToString();
            }
            return "";
        }

        public static string randomTun2socksInterface()
        {
            Random random = new Random();

            string[] networkInterfacePrefixes = {
                "wlan",
                "eth",
                "rmnet",
                "ccmni",
                "wwan"
            };

            string prefix = networkInterfacePrefixes[random.Next(networkInterfacePrefixes.Length)];

            int number = random.Next(10, 100); // 10–99

            return $"{prefix}{number}";
        }
    }
}
