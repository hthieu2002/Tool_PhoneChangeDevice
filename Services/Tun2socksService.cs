using Newtonsoft.Json.Linq;
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
        public static void setUpTun2socksOnDevice(string tun2socksDir, string deviceId, string tun0TableId = "9988")
        {
            Directory.CreateDirectory(deviceId);
            Dictionary<string, string> files = new Dictionary<string, string>();
            files.Add(string.Concat(Directory.GetCurrentDirectory(), @"\Resources\tun2socks"), tun2socksDir);
            ADBService.pushFiles(files, deviceId);
            ADBService.setPermission("777", tun2socksDir + "/tun2socks", deviceId);
            ADBService.runCMDRoot($"shell \"echo {tun0TableId} > /data/local/tmp/tun0TableId.txt\"", deviceId);
        }

        public static void start(string tun2socksDir, string proxyParams, string ipLocalV4, string deviceId,
                                 string tun0TableId = "9988")
        {
            if(string.IsNullOrEmpty(ipLocalV4))
            {
                ipLocalV4 = randomLocalIPv4ForTun2socks();
            }
            enableIPv6(deviceId);
            Thread.Sleep(1000);
            setUpTun0(deviceId, ipLocalV4, tun0TableId);
            Thread.Sleep(1000);
            configIptablesTun2socks(deviceId);
            Thread.Sleep(1000);
            startTun2socks(proxyParams, tun2socksDir, deviceId);
        }

        /**
         * Dừng toàn bộ tiến trình tun2socks và hạ interface TUN hiện tại.
         *
         * @param deviceId     ID của thiết bị được sử dụng cho các lệnh ADB.
         * @param tun0TableId  (tuỳ chọn) ID của bảng định tuyến tun0, mặc định là "9988".
         *
         * Lưu ý:
         * - Khi gọi hàm này từ bên ngoài, bạn nên kiểm tra xem file `/data/local/tmp/tun0TableId.txt`
         *   đã tồn tại và có giá trị `tun0TableId` phù hợp hay chưa.
         * - Có thể đọc giá trị thực tế bằng lệnh:
         *     string tun0TableId = ADBService.readFromFile("/data/local/tmp/tun0TableId.txt", deviceId);
         * - Nếu file tồn tại và chứa giá trị khác "9988", hãy truyền giá trị đó vào `stop()`
         *   để đảm bảo xóa đúng bảng định tuyến tương ứng.
         *
         * Ví dụ:
         *     string tun0TableId = ADBService.readFromFile("/data/local/tmp/tun0TableId.txt", deviceId);
         *     Tun2socksService.stop(deviceId, tun0TableId);
         */
        public static void stop(string deviceId, string tun0TableId = "9988")
        {
            stopTun2Socks(deviceId, tun0TableId);
        }

        private static void setUpTun0(string deviceId, string ipLocalV4, string tun0TableId)
        {
            var subnet = RandomService.generateSubnetMask();
            ADBService.runCMDRoot("shell \"mkdir /dev/net\"", deviceId);
            ADBService.runCMDRoot("shell \"mknod /dev/net/tun c 10 200\"", deviceId);
            ADBService.runCMDRoot("shell \"chmod 0666 /dev/net/tun\"", deviceId);
            ADBService.runCMDRoot("shell \"ip tuntap add dev tun0 mode tun\"", deviceId);
            ADBService.runCMDRoot((string.Format($"shell \"echo '{tun0TableId} tun0' >> /data/misc/net/rt_tables\"")), deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot(string.Format($"shell \"ifconfig tun0 {ipLocalV4} netmask {subnet["mask"]} {ipLocalV4} up\""), deviceId);
            ADBService.runCMDRoot("shell \"ip route add default dev tun0 proto static scope link table tun0\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot("shell \"ip -6 route add default dev tun0 proto static scope link table tun0\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot("shell \"ip link set tun0 up\"", deviceId);
            ADBService.runCMDRoot("shell \"ip route flush cache\"", deviceId);
            Thread.Sleep(1000);
            routingRulesIPv4(deviceId);
            Thread.Sleep(1000);
            routingRulesIPv6(deviceId);
        }

        private static void enableIPv6(string deviceId)
        {
            ADBService.runCMDRoot("shell \"sysctl -w net.ipv6.conf.tun0.disable_ipv6=0\"", deviceId);
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

        private static void routingRulesIPv4(string deviceId)
        {
            ADBService.runCMDRoot("shell \"ip rule add iif tun0 lookup local_network pref 100\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add fwmark 0xc0068/0xcffff lookup tun0 pref 110\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add fwmark 0x0/0x20000 iif lo uidrange 0-99999 lookup tun0 pref 120\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-9999 lookup tun0 pref 121\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-99999 lookup tun0 pref 122\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-0 lookup tun0 pref 123\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add iif lo oif tun0 uidrange 0-9999 lookup tun0 pref 130\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule add iif lo oif tun0 uidrange 0-99999 lookup tun0 pref 131\"", deviceId);
        }

        private static void routingRulesIPv6(string deviceId)
        {
            ADBService.runCMDRoot("shell \"ip -6 rule add iif tun0 lookup local_network pref 200\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add fwmark 0xc0068/0xcffff lookup tun0 pref 210\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add fwmark 0x0/0x20000 iif lo uidrange 0-99999 lookup tun0 pref 220\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-9999 lookup tun0 pref 221\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-99999 lookup tun0 pref 222\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add fwmark 0x10068/0x1ffff iif lo uidrange 0-0 lookup tun0 pref 223\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add iif lo oif tun0 uidrange 0-9999 lookup tun0 pref 230\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule add iif lo oif tun0 uidrange 0-99999 lookup tun0 pref 231\"", deviceId);
        }

        private static void configIptablesTun2socks(string deviceId)
        {
            ADBService.runCMDRoot("shell \"iptables -A OUTPUT ! -o tun0 -j DROP\"", deviceId);
            ADBService.runCMDRoot("shell \"iptables -I OUTPUT -m owner --uid-owner 0 -j ACCEPT\"", deviceId);
            ADBService.runCMDRoot("shell \"ip6tables -A OUTPUT ! -o tun0 -j DROP\"", deviceId);
            ADBService.runCMDRoot("shell \"ip6tables -I OUTPUT -m owner --uid-owner 0 -j ACCEPT\"", deviceId);
        }

        private static void startTun2socks(string proxyParams, string tun2socksDir, string deviceId)
        {
            ADBService.runCMDRoot($"shell \"{tun2socksDir}/tun2socks -device tun://tun0 -proxy {proxyParams} -interface wlan0 -mtu 1500 &> /dev/null &\"", deviceId);
        }

        private static void stopTun2Socks(string deviceId, string tun0TableId)
        {
            ADBService.runCMDRoot("shell  \"killall -q tun2socks\"", deviceId);
            ADBService.runCMDRoot("shell  \"pkill -9 tun2socks\"", deviceId);
            ADBService.runCMDRoot("shell  \"ifconfig tun0 down\"", deviceId);
            ADBService.runCMDRoot("shell  \"ip tuntap del dev tun0 mode tun\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"ip route flush table {tun0TableId}\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del iif tun0 lookup local_network\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del fwmark 0xc0068/0xcffff lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del fwmark 0x0/0x20000 iif lo uidrange 0-99999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del fwmark 0x10068/0x1ffff iif lo uidrange 0-9999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del fwmark 0x10068/0x1ffff iif lo uidrange 0-99999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del fwmark 0x10068/0x1ffff iif lo uidrange 0-0 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del iif lo oif tun0 uidrange 0-9999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip rule del iif lo oif tun0 uidrange 0-99999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip route flush cache\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"ip -6 route flush table {tun0TableId}\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del iif tun0 lookup local_network\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del fwmark 0xc0068/0xcffff lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del fwmark 0x0/0x20000 iif lo uidrange 0-99999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del fwmark 0x10068/0x1ffff iif lo uidrange 0-9999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del fwmark 0x10068/0x1ffff iif lo uidrange 0-99999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del fwmark 0x10068/0x1ffff iif lo uidrange 0-0 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del iif lo oif tun0 uidrange 0-9999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 rule del iif lo oif tun0 uidrange 0-99999 lookup tun0\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip -6 route flush cache\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"iptables -t nat -F\"", deviceId);
            ADBService.runCMDRoot($"shell  \"iptables -t mangle -F\"", deviceId);
            ADBService.runCMDRoot($"shell  \"iptables -F\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"ip6tables -t nat -F\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip6tables -t mangle -F\"", deviceId);
            ADBService.runCMDRoot($"shell  \"ip6tables -F\"", deviceId);
            Thread.Sleep(1000);
            ADBService.runCMDRoot($"shell  \"sed -i '/{tun0TableId} tun0/d' /data/misc/net/rt_tables\"", deviceId);
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
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject jsonOblect = JObject.Parse(str);
                        ADBService.FakeTimezone(jsonOblect["query"].ToString(), deviceId);
                        return jsonOblect["query"].ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                else if (proxyParts.Length == 2)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject jsonOblect = JObject.Parse(str);
                        ADBService.FakeTimezone(jsonOblect["query"].ToString(), deviceId);
                        return jsonOblect["query"].ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
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
                }
                else if (proxyParts.Length == 2)
                {
                    commandline = $"curl --proxy http://{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                }
                else
                {
                    return "";
                }

                str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));

                if (!string.IsNullOrEmpty(str))
                {
                    JObject jsonObject = JObject.Parse(str);
                    return jsonObject["query"]?.ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
