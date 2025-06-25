using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ToolChange.Models
{
    public static class DeviceUpdater
    {
        public static void UpdateProgress(
      ObservableCollection<DeviceModel> list,
      string deviceId, string percent, string progress)
        {
            var dev = list.FirstOrDefault(d => d.DeviceId == deviceId);
            if (dev == null) return;

            var disp = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            disp.Invoke(() =>
            {
                dev.Percentage = percent;
                dev.Progress = progress;
            });
        }

    }

}
