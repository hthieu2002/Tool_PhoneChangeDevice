using System.Collections.Generic;

namespace POCO.ResponseModels

{
    public class GetProxySummaryResponse
    {
        public ProxySummaryModel GetProxySummary { get; set; }


    }
    public class ProxySummaryModel
    {
        public List<SingleProxySummary> Data { get; set; }
    }
    public class SingleProxySummary
    {
        public string Type { get; set; }
        public int Quantity { get; set; }
    }
}
