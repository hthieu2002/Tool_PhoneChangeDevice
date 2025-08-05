using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolChange.Models;

namespace ToolChange.Services
{
    public static class TempMemoryStorage
    {
        public static List<AppItem> CachedListB { get; set; } = new();
    }

}
