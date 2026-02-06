using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace Services
{
    [Obfuscation(Exclude = false)]
    public class Tun2socksService
    {
        public static void setUpTun2socksOnDevice(string tun2socksDir, string deviceId)
        {
            Directory.CreateDirectory(deviceId);
            Dictionary<string, string> files = new Dictionary<string, string>();
            files.Add(string.Concat(Directory.GetCurrentDirectory(), @"\Resources\tun2socks"), tun2socksDir);
            ADBService.pushFiles(files, deviceId);
            ADBService.setPermission("777", tun2socksDir + "/tun2socks", deviceId);
        }

        public static void start(string tun2socksDir, string proxyParams, string deviceId)
        {

            setUptun0(deviceId);
            Thread.Sleep(1000);
            startTun2socks(proxyParams, tun2socksDir, deviceId);
        }

        public static void stop(string deviceId)
        {
            stopTun2Socks(deviceId);
        }

        private static void setUptun0(string deviceId)
        {
            var randomSubnet = RandomService.generateSubnetMask();
            string localIP = RandomService.generateIpv4();

            ADBService.runCMDRoot($"shell \"mkdir /dev/net\"", deviceId);
            ADBService.runCMDRoot($"shell \"mknod /dev/net/tun c 10 200\"", deviceId);
            ADBService.runCMDRoot($"shell \"chmod 0666 /dev/net/tun\"", deviceId);

            ADBService.runCMDRoot($"shell \"sysctl -w net.ipv4.conf.all.rp_filter=0\"", deviceId);
            ADBService.runCMDRoot($"shell \"sysctl -w net.ipv4.conf.wlan0.rp_filter=0\"", deviceId);

            ADBService.runCMDRoot($"shell \"ip tuntap add dev tun0 mode tun\"", deviceId);
            ADBService.runCMDRoot($"shell \"ifconfig tun0 {localIP} netmask 255.255.0.0\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip route add default dev tun0 table 666 metric 1\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip rule add lookup 666 pref 10\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip rule add fwmark 4953 lookup 666 pref 10\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip -6 route add default dev tun0 table 666 metric 1\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip -6 rule add lookup 666 pref 10\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip link set tun0 up\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip route flush cache\"", deviceId);
            ADBService.runCMDRoot($"shell \"ip -6 route flush cache\"", deviceId);

            ADBService.runCMDRoot($"shell \"iptables -t mangle -N TUNSOCKS\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t mangle -F TUNSOCKS\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t mangle -I TUNSOCKS -d 192.168.0.0/16 -j RETURN\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t mangle -I TUNSOCKS -d 10.0.0.0/8 -j RETURN\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t mangle -I TUNSOCKS -p tcp -d 68.225.23.67 --j RETURN\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t mangle -A TUNSOCKS -j MARK --set-mark 4953\"", deviceId);
            ADBService.runCMDRoot($"shell \"iptables -t mangle -I OUTPUT -j TUNSOCKS\"", deviceId);
        }

        private static void startTun2socks(string proxyParams, string tun2socksDir, string deviceId)
        {
            ADBService.runCMDRoot($"shell \"nohup {tun2socksDir}/tun2socks -device tun://tun0 -proxy {proxyParams} -interface wlan0 -mtu 1500 &> /dev/null &\"", deviceId);
        }

        private static void stopTun2Socks(string deviceId)
        {
            ADBService.runCMDRoot("shell \"killall -q tun2socks\"", deviceId);
            ADBService.runCMDRoot("shell \"pkill -9 tun2socks\"", deviceId);
            ADBService.runCMDRoot("shell \"ifconfig tun0 down\"", deviceId);
            ADBService.runCMDRoot("shell \"ip tuntap del dev tun0 mode tun\"", deviceId);
            ADBService.runCMDRoot("shell \"ip link delete tun0\"", deviceId);
            ADBService.runCMDRoot("shell \"ip route del default dev tun0 table 666 metric 1\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule del lookup 666 pref 10\"", deviceId);
            ADBService.runCMDRoot("shell \"ip rule del fwmark 4953 lookup 666 pref 10\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 route del default dev tun0 table 666 metric 1\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 rule del lookup 666 pref 10\"", deviceId);
            ADBService.runCMDRoot("shell \"ip route flush cache\"", deviceId);
            ADBService.runCMDRoot("shell \"ip -6 route flush cache\"", deviceId);
            ADBService.runCMDRoot("shell \"iptables -t mangle -X TUNSOCKS\"", deviceId);
            ADBService.runCMDRoot("shell \"iiptables -t mangle -F TUNSOCKS\"", deviceId);
            ADBService.runCMDRoot("shell \"iptables -t mangle -F\"", deviceId);
            ADBService.runCMDRoot("shell \"iptables -t nat -F\"", deviceId);
            ADBService.runCMDRoot("shell \"iptables -F\"", deviceId);
        }
    }
}
