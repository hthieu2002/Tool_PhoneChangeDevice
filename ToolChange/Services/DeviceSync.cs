namespace ToolChange.Services
{
    public static class DeviceSync
    {
        // Chỉ có 1 slot → bảo đảm độc quyền
        public static readonly SemaphoreSlim Mutex = new SemaphoreSlim(1, 1);
    }

}
