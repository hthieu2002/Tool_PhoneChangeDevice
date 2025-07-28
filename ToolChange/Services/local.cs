using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolChange.Models;

namespace ToolChange.Services
{
    public static class local
    {
        public static bool loadDevice { get; set; } = false;
        public static ScrcpyDeviceModel device { get; set; } 
    }
}
