using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace POCO.Models
{
    public enum ProxyType
    {
        PUBLIC,
        PRIVATE,
        FIXED
    }
    public enum ProxyState
    {
        IDLE,
        USING,
        USED,
        RESETTING,
        ERROR
    }
    public enum ProxyProtocol
    {
        HTTP,
        HTTPS,
        SOCKS
    }
    public class ProxyInfoBase
    {
        private string host = "";
        private string user = "";
        private string pass = "";
        private string rotateLink = "";
        public string Host { get => host.Trim(); set => host = value; }
        public int Port { get; set; }
        public string User { get => user.Trim(); set => user = value; }
        public string Pass { get => pass.Trim(); set => pass = value; }
        public bool HasCredential => !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Pass);
        public ProxyProtocol ProxyProtocol { get; set; } = ProxyProtocol.SOCKS;
        public ProxyType Type { get; set; } = ProxyType.PUBLIC;
        public string RotateLink { get => rotateLink.Trim(); set => rotateLink = value; }
        public bool IsUse { get; set; } = false;
        public bool IsUsed { get; set; } = false;
        public bool IsRotateProxy => !string.IsNullOrEmpty(RotateLink);

        public string ToSchema()
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Pass))
                return string.Format("{0}:{1}", Host, Port);
            return string.Format("{0}:{1}:{2}:{3}:{4}:{5}", Host, Port, User, Pass, ProxyProtocol.ToString(), RotateLink);
        }
        public string ToCredentialString()
        {
            return string.Format("{0}:{1}", User, Pass);
        }
        public override string ToString()
        {
            return string.Format("{0}:{1}", Host, Port);
        }

        public static ProxyInfoBase FromString(string schema)
        {
            // host:port[:user:pass][:protocol][:type]
            /*var splitted = schema.Split(':', '|');*/
            var splitted = schema.Split(':');

            if (splitted.Length < 2) return null;

            ProxyProtocol protocol = ProxyProtocol.SOCKS;
            ProxyType type = ProxyType.PUBLIC;
            string user = splitted.ElementAtOrDefault(2) ?? "";
            string pass = splitted.ElementAtOrDefault(3) ?? "";
            Enum.TryParse(splitted.ElementAtOrDefault(4), true, out protocol);
            Enum.TryParse(splitted.ElementAtOrDefault(5), true, out type);
            /*string rotateLink = schema.Substring(schema.LastIndexOf("http://")) ?? "";*/
            string rotateLink = string.Join(":", splitted, 6, splitted.Length - 6);

            return new ProxyInfoBase
            {
                Host = splitted[0],
                Port = int.Parse(splitted[1]),
                ProxyProtocol = protocol,
                Type = type,
                User = user,
                Pass = pass,
                RotateLink = rotateLink
            };
        }
    }
    public class AutoProxyInfo : ProxyInfoBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int ResetErrorCount { get; set; } = 0;
        public DateTime LockToTime { get; set; }
        public DateTime ExpiredTime { get; set; } = DateTime.Now.AddMinutes(15);
        public ProxyState State { get; set; } = ProxyState.IDLE;
        public int CountUsed { get; set; } = 1;
        public AutoProxyInfo() { }
        public AutoProxyInfo(string rawString)
        {
            var proxyInfo = FromString(rawString);
            if (proxyInfo == null) return;

            foreach (PropertyInfo prop in proxyInfo.GetType().GetProperties())
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(this, prop.GetValue(proxyInfo));
                }
            }
                

            //this.Host = proxyInfo.Host;
            //this.Port = proxyInfo.Port;
            //this.User = proxyInfo.User;
            //this.Pass = proxyInfo.Pass;
            //this.ProxyProtocol = proxyInfo.ProxyProtocol;
            //this.Type = proxyInfo.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is AutoProxyInfo proxy &&
                   Host == proxy.Host &&
                   Port == proxy.Port &&
                   User == proxy.User &&
                   Pass == proxy.Pass;
        }

        public override int GetHashCode()
        {
            int hashCode = 1137869079;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Host);
            hashCode = hashCode * -1521134295 + Port.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(User);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Pass);
            return hashCode;
        }
    }
}
