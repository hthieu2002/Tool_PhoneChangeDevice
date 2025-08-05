using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Models
{
    public class TimezoneInfoModel
    {
        public int STT { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string TimeZone { get; set; }
        public string GmtOffset { get; set; }
    }

}
