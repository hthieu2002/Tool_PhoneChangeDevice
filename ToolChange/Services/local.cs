using ToolChange.Models;

namespace ToolChange.Services
{
    public static class local
    {
        public static bool loadDevice { get; set; } = false;
        public static ScrcpyDeviceModel device { get; set; }
    }
}
