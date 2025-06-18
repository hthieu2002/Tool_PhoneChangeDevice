using Newtonsoft.Json;
namespace POCO.Models
{
    public class GmailCheckerModelRequest
    {
        [JsonProperty("data")]
        public string[] Data { get; set; }
        [JsonProperty("transactionId")]
        public int TransactionId { get; set; } = 0;

    }
    public class GmailCheckerModelResponse
    {
        [JsonProperty("result")]
        public ResultModelResponse Result { get; set; }
    }

    public class ResultModelResponse
    {
        [JsonProperty("list")]
        public string[] List { get; set; }
    }

}
