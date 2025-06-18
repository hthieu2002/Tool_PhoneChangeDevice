using System;

namespace POCO.Models
{
    public class LeadModel
    {
        public AndroidPhoneModel Device { get; set; }
        public PackageModel Package { get; set; }
        public string UserAgent { get; set; }
        public string Country { get; set; }
        public DateTime TransactionDate { get; set; }

      
    }
}
