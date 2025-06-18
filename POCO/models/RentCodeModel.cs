using System.Collections.Generic;

namespace POCO.Models
{
    public class RentCodeModel
    {
        public int id { set; get; }
        public string phoneNumber { set; get; }
        public string statusString { set; get; }
        public string status { set; get; }
        public List<object> messages { set; get; }

        public string success { set; get; }
        public string message { set; get; }
    }
}
