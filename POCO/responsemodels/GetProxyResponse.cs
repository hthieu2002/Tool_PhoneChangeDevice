using POCO.Models;

namespace POCO.ResponseModels

{
    public class GetProxyResponse
    {
        public ProxyModel GetProxy { get; set; }

    }
    public class ProxyModel
    {
        public string Id { get; set; }
        public string Proxy { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string ApiKey { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string ResetWebHook { get; set; } = "";
        public string Protocol { get; set; } = ProxyProtocol.SOCKS.ToString();
    }
}
