using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Services
{
    public static class DeviceSync
    {
        // Chỉ có 1 slot → bảo đảm độc quyền
        public static readonly SemaphoreSlim Mutex = new SemaphoreSlim(1, 1);
    }

}
