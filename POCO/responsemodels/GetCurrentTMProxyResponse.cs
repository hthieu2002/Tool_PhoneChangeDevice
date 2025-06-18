using Newtonsoft.Json;

namespace POCO.ResponseModels

{
    public class GetCurrentTMProxyRequest
    {
        [JsonProperty("api_key")]
        public string Api_Key { get; set; }
    }
    public class GetCurrentTMProxyResponse
    {

        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public DataTMProxy Data { get; set; }

    }
    public class DataTMProxy
    {
        [JsonProperty("ip_allow")]
        public string Ip_Allowed { get; set; }
        [JsonProperty("location_name")]
        public string Location_Name { get; set; }
        [JsonProperty("https")]
        public string Https { get; set; }
        [JsonProperty("timeout")]
        public int Timeout { get; set; }
        [JsonProperty("next_request")]
        public int Next_Request { get; set; }
        [JsonProperty("expired_at")]
        public string Expired_At { get; set; }
    }
}
