using System.Collections.Generic;

namespace POCO.Models
{
    public class LicensesModel
    {
        public List<License> Licenses { get; set; }
        public string NextToken { get; set; }

        public override string ToString()
        {
            return string.Join("================\n", Licenses);
        }
    }
    public class License
    {
        public int DurationInDay { get; set; }
        public string SerialNo { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public long DateEnd { get; set; }
        public long DateStart { get; set; }
        public string Model { get; set; } = "TISSOT";


        public override string ToString()
        {
            return $"Duration In Day: {DurationInDay} \n SerialNo: {SerialNo} \n Type: {Type} \n Email: {Email}";
        }
    }

}
