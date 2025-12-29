using ToolChange.Models;

namespace ToolChange.Services
{
    public static class TempMemoryStorage
    {
        public static List<AppItem> CachedListB { get; set; } = new();
    }

}
