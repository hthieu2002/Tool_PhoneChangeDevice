using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Services
{
    [Obfuscation(Exclude = false)]
    public class RedSocksService
    {
        // install redsocks and iptables first
        private static string defaultRedsocksFileName = "redsocks.conf";
        private static string defaultRedsocksLocalPort = "12345";

        public static void setUpRedSocksOnDevice(string redsocksDir, string deviceId)
        {
            Directory.CreateDirectory(deviceId);
            Dictionary<string, string> files = new Dictionary<string, string>();
            files.Add(string.Concat(Directory.GetCurrentDirectory(), @"\Resources\iptables"), redsocksDir);
            files.Add(string.Concat(Directory.GetCurrentDirectory(), @"\Resources\redsocks"), redsocksDir);
            ADBService.pushFiles(files, deviceId);
            ADBService.setPermission("777", redsocksDir + "/redsocks", deviceId);
        }

        public static void start(string proxyHostIP, int proxyHostPort, string redsocksDir, string deviceId, string username = "", string password = "", string newIp = "", string defaultConnectionMethod = "socks5")
        {
            string hostIP = Dns.GetHostAddresses(proxyHostIP)[0].ToString();
            // 1. create a textfile redsocks.conf with IP and Port. By default, local redsocks port = 12345, type = socks5
            string config = string.Format("base {{log_debug = off;log_info = off;log = \"stderr\";daemon = on;redirector = iptables;}} " +
                "redsocks {{ local_ip = 0.0.0.0; local_port = {0}; ip = {1}; port = {2}; type = {3}; }}"
                , defaultRedsocksLocalPort
                , hostIP
                , proxyHostPort
                , defaultConnectionMethod);
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                config = string.Format("base {{log_debug = off;log_info = off;log = \"stderr\";daemon = on;redirector = iptables;}} " +
                "redsocks {{ local_ip = 0.0.0.0; local_port = {0}; ip = {1}; port = {2}; type = {3}; login = \"{4}\"; password = \"{5}\"; }} "
                , defaultRedsocksLocalPort
                , hostIP
                , proxyHostPort
                , defaultConnectionMethod
                , username
                , password);
            }
            using (StreamWriter sw = System.IO.File.CreateText(defaultRedsocksFileName))
            {
                sw.WriteLine(config);
            }

            // 2. startRedSocks from adb
            var redsocksFileConfigPath = string.Concat(Directory.GetCurrentDirectory(), @"\", defaultRedsocksFileName);
            // push file redsocks.conf to the device
            ADBService.pushFiles(new Dictionary<string, string>() { { redsocksFileConfigPath, redsocksDir } }, deviceId);
            ADBService.startRedSocks(hostIP, redsocksDir, deviceId);
            Thread.Sleep(5000);
            ADBService.FakeLocalIP(deviceId, newIp);
        }

        public static void stop(string deviceId)
        {
            ADBService.stopRedSocks(deviceId);
        }
    }


}
