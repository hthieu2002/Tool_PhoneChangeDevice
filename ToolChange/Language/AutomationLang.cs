using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Language
{
    public static class AutomationLang
    {
        public static string logRunScript { get; set; } = "Chọn script run !";
        public static string logLoadScript { get; set; } = "Không có script tồn tại!";
        public static string logUntimateRunSctiptInfo { get; set; } = "Chạy script vĩnh viễn lần ";
        public static string logRunSctiptInfo { get; set; } = "Chạy script lần ";
        public static string logUntimateRunSctiptInfoSuccess { get; set; } = "Hoàn thành chạy vĩnh viễn ";
        public static string logRunSctiptInfoSuccess { get; set; } = "Hoàn thành script ";
    }
}
