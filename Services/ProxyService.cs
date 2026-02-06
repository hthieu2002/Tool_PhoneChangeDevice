using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ProxyService
    {
        public static string getDeviceIPv4(string deviceId)
        {
            return ADBService.runCMDRoot(string.Format("shell curl -s https://api64.ipify.org"), deviceId).Trim();
        }

        public static string getIpv4SocksProxy(string proxy, string deviceId)
        {
            try
            {
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);

                string commandLine = "";
                switch (proxyParts.Length)
                {
                    case 2:
                        commandLine = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                        break;
                    case 4:
                        commandLine = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} --proxy-user {proxyParts[2]}:{proxyParts[3]} \"{url}\"";
                        break;
                    default:
                        return "";
                }
                return getIpv4FromResultCurl(commandLine);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string getIpv4HttpProxy(string proxy, string deviceId)
        {
            try
            {
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);

                string commandLine = "";
                switch (proxyParts.Length)
                {
                    case 2:
                        commandLine = $"curl --proxy http://{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                        break;
                    case 4:
                        commandLine = $"curl --proxy http://{proxyParts[2]}:{proxyParts[3]}@{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                        break;
                    default:
                        return "";
                }
                return getIpv4FromResultCurl(commandLine);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string getIpv4FromResultCurl(string commandline)
        {
            try
            {
                string result = CmdProcess.ExecuteCommand($"/C {commandline}");
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jsonObject = JObject.Parse(result);
                    return jsonObject["query"]?.ToString();
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string getTimeZoneSocksProxy(string proxy, string deviceId)
        {
            try
            {
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);

                string commandLine = "";
                switch (proxyParts.Length)
                {
                    case 2:
                        commandLine = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                        break;
                    case 4:
                        commandLine = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} --proxy-user {proxyParts[2]}:{proxyParts[3]} \"{url}\"";
                        break;
                    default:
                        return "";
                }
                return getTimeZoneFromResultCurl(commandLine);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string getTimeZoneHttpProxy(string proxy, string deviceId)
        {
            try
            {
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);

                string commandLine = "";
                switch(proxyParts.Length)
                {
                    case 2:
                        commandLine = $"curl --proxy http://{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                        break;
                    case 4:
                        commandLine = $"curl --proxy http://{proxyParts[2]}:{proxyParts[3]}@{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                        break;
                    default:
                        return "";
                }
                return getTimeZoneFromResultCurl(commandLine);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string getTimeZoneFromResultCurl(string commandline)
        {
            try
            {
                string result = CmdProcess.ExecuteCommand($"/C {commandline}");
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jsonObject = JObject.Parse(result);
                    return jsonObject["timezone"]?.ToString();
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
