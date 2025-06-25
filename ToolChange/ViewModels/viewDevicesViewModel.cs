using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToolChange.Language;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.Views;

namespace ToolChange.ViewModels
{
    public class viewDevicesViewModel : INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_SHOWWINDOW = 0x0040;
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public LocalizationViewModel LanguageVM { get; set; } // language
        public ObservableCollection<ViewDeviceModel> Devices { get; set; } = new();
        private readonly HashSet<string> ViewedDevices = new();

        private CancellationTokenSource _cancellationTokenSource;
        private readonly object _lock = new();
        private ObservableCollection<string> _selectedDeviceIds = new();
        private readonly Dictionary<string, int> _deviceIdOrder = new();

        public ObservableCollection<string> SelectedDeviceIds
        {
            get => _selectedDeviceIds;
            set
            {
                _selectedDeviceIds = value;
                OnPropertyChanged(nameof(SelectedDeviceIds));
            }
        }
   
        private DeviceInfo _countDevice;
        public DeviceInfo CountDevice
        {
            get => _countDevice;
            set
            {
                _countDevice = value;
                OnPropertyChanged(nameof(CountDevice));
            }
        }
        private int _selectedDeviceCount;
        public int SelectedDeviceCount
        {
            get => _selectedDeviceCount;
            set
            {
                _selectedDeviceCount = value;
                OnPropertyChanged(nameof(SelectedDeviceCount));
            }
        }
        private int _resolution = 1280;
        public int Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnPropertyChanged(nameof(Resolution));
            }
        }

        private int _scale = 150;
        public int Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }
        private bool _isBoxChecked = true;
        public bool IsBoxChecked
        {
            get => _isBoxChecked;
            set
            {
                _isBoxChecked = value;
                OnPropertyChanged(nameof(IsBoxChecked));
            }
        }

        public ICommand DeviceClickCommand { get; set; }
        public viewDevicesViewModel()
        {
            SelectedDeviceIds = new ObservableCollection<string>();
            SelectedDeviceCount = SelectedDeviceIds.Count;
            DeviceClickCommand = new RelayCommand<Models.ViewDeviceModel>(async (device) => await DeviceClick(device));
        }

        public ICommand SelectDeviceCommand => new RelayCommand<ViewDeviceModel>(ToggleDeviceSelection);
        public ICommand ViewCommand => new RelayCommandView(ViewSelectedDevices);
        public ICommand RefreshCommand => new RelayCommandView(Refresh);
        public ICommand PushFileCommand => new RelayCommandView(PushFileToDevices);
        public ICommand InstallApkCommand => new RelayCommandView(InstallApkToDevices);
       
        private void ToggleDeviceSelection(ViewDeviceModel device)
        {
            if (device == null) return;

            device.IsSelected = !device.IsSelected;

            if (device.IsSelected)
            {
                if (!SelectedDeviceIds.Contains(device.DeviceId))
                    SelectedDeviceIds.Add(device.DeviceId);
            }
            else
            {
                SelectedDeviceIds.Remove(device.DeviceId);
            }
            //MessageBox.Show(string.Join("\n", SelectedDeviceIds));
            SelectedDeviceCount = SelectedDeviceIds.Count;
            OnPropertyChanged(nameof(SelectedDeviceIds));
        }
        private async Task DeviceClick(Models.ViewDeviceModel model)
        {
            System.Windows.MessageBox.Show(model.DeviceId);
        }
        private async void PushFileToDevices()
        {
            if (SelectedDeviceIds == null || SelectedDeviceIds.Count == 0)
            {
                System.Windows.MessageBox.Show(ViewDeviceLang.logPushFile, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn tệp để push",
                Filter = "All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string localFilePath = openFileDialog.FileName;
                string remotePath = "/sdcard/" + Path.GetFileName(localFilePath);

                var tasks = SelectedDeviceIds.Select(deviceId => Task.Run(() =>
                {
                    ADBService.RunAdbCommand($"-s {deviceId} push \"{localFilePath}\" \"{remotePath}\"");
                }));

                await Task.WhenAll(tasks);
                System.Windows.MessageBox.Show(ViewDeviceLang.logPushFileSuccess, ViewDeviceLang.InfoSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void InstallApkToDevices()
        {
            if (SelectedDeviceIds == null || SelectedDeviceIds.Count == 0)
            {
                System.Windows.MessageBox.Show(ViewDeviceLang.logInstallAPK, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file APK",
                Filter = "APK Files|*.apk"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string apkPath = openFileDialog.FileName;

                var tasks = SelectedDeviceIds.Select(deviceId => Task.Run(() =>
                {
                    ADBService.RunAdbCommand($"-s {deviceId} install -r \"{apkPath}\"");
                }));

                await Task.WhenAll(tasks);
                System.Windows.MessageBox.Show(ViewDeviceLang.logInstallAPKSuccess, ViewDeviceLang.InfoSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void Refresh()
        {
            SelectedDeviceIds.Clear();
            ViewedDevices.Clear();
            
        }
        public async void ViewSelectedDevices()
        {
            if (SelectedDeviceIds == null || SelectedDeviceIds.Count == 0)
            {
                System.Windows.MessageBox.Show(ViewDeviceLang.logViewDevice, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CleanupClosedScrcpyWindows();
            string scrcpyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "scrcpy.exe");
            int startX = 0;
            int startY = 50;
            int windowWidth = 300;
            int windowHeight = 600;
            int spacing = 10;
            int currentX = startX;

            // Tạo danh sách thiết bị cần xử lý
            var pendingDevices = SelectedDeviceIds
                .Where(id => !ViewedDevices.Contains(id))
                .ToList();
            System.Windows.MessageBox.Show(ViewDeviceLang.InfoViewDevice,Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
            while (pendingDevices.Any())
            {
                foreach (var deviceId in pendingDevices.ToList())
                {
                   
                    var hwnd = FindWindow(null, deviceId);
                    if (hwnd == IntPtr.Zero)
                    {
                        var resolution = Resolution;
                        var valueScale = Scale / 100.0 * 480;
                        var bitrate = $"{valueScale}M";
                        var turnOffFlag = IsBoxChecked ? "--turn-screen-off" : "";
                        var arguments = $"-s {deviceId} " +
                                        $"--window-title=\"{deviceId}\" " +
                                        $"--max-size={resolution} " +
                                        $"--video-bit-rate={bitrate} "+
                                        $"{turnOffFlag}";

                        var psi = new ProcessStartInfo(scrcpyPath, arguments)
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        try { Process.Start(psi); }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Không thể chạy scrcpy cho thiết bị {deviceId}:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            pendingDevices.Remove(deviceId);
                            continue;
                        }

                        // Đợi tối đa 5s cho scrcpy mở
                        int retry = 0;
                        while (hwnd == IntPtr.Zero && retry < 50)
                        {
                            await Task.Delay(3000);
                            hwnd = FindWindow(null, deviceId);
                            retry++;
                        }
                    }

                    hwnd = FindWindow(null, deviceId);
                    if (hwnd != IntPtr.Zero)
                    {
                        MoveWindow(hwnd, currentX, startY, windowWidth, windowHeight, true);
                        currentX += windowWidth + spacing;

                        ViewedDevices.Add(deviceId);
                        pendingDevices.Remove(deviceId);
                    }
                }

                if (pendingDevices.Any())
                    await Task.Delay(2000); // Đợi trước khi retry lần nữa
            }
        }
        private void CleanupClosedScrcpyWindows()
        {
            var closedDevices = ViewedDevices
                .Where(id => FindWindow(null, id) == IntPtr.Zero)
                .ToList();

            foreach (var id in closedDevices)
            {
                ViewedDevices.Remove(id);
                
            }
        }

        public void CloseScrcpyWindows()
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                if (SelectedDeviceIds.Contains(title))
                {
                    PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }

                return true;
            }, IntPtr.Zero);
        }
        public void StartMonitoring()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var deviceList = ADBService.GetConnectedDevices();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        lock (_lock)
                        {
                            foreach (var dev in deviceList)
                            {
                                if (!Devices.Any(d => d.DeviceId == dev))
                                {
                                    // Gán thứ tự cố định cho DeviceId nếu chưa có
                                    if (!_deviceIdOrder.ContainsKey(dev))
                                        _deviceIdOrder[dev] = _deviceIdOrder.Count;

                                    var model = new ViewDeviceModel
                                    {
                                        DeviceId = dev,
                                        Index = _deviceIdOrder[dev],
                                        IsSelected = false
                                    };
                                    Devices.Add(model);
                                    StartScreencap(model);
                                }
                            }

                            // Gỡ thiết bị không còn kết nối
                            var toRemove = Devices.Where(d => !deviceList.Contains(d.DeviceId)).ToList();
                            foreach (var d in toRemove)
                            {
                                StopScreencap(d);
                                Devices.Remove(d);
                            }

                            // Cập nhật số lượng
                            CountDevice = new DeviceInfo($"{StaticLang.DeviceCount} {Devices.Count}", Devices.LastOrDefault()?.DeviceId ?? "");

                            // Cập nhật lại Index theo đúng thứ tự
                            RefreshDeviceIndexes();
                        }
                    });

                    await Task.Delay(2000);
                }
            });
        }

        private void RefreshDeviceIndexes()
        {
            // Gán lại index theo _deviceIdOrder
            foreach (var device in Devices)
            {
                if (_deviceIdOrder.TryGetValue(device.DeviceId, out int index))
                {
                    device.Index = index;
                }
            }

            //Sắp xếp Devices theo Index tăng dần
            var sorted = Devices.OrderBy(d => d.Index).ToList();

            Devices.Clear();
            foreach (var d in sorted)
                Devices.Add(d);
        }



        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            foreach (var device in Devices)
            {
                StopScreencap(device);
            }
        }

        private readonly Dictionary<string, CancellationTokenSource> _streamTokens = new();

        private void StartScreencap(ViewDeviceModel device)
        {
            var cts = new CancellationTokenSource();
            _streamTokens[device.DeviceId] = cts;

            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var psi = new ProcessStartInfo("./Resources/adb.exe", $"-s {device.DeviceId} exec-out screencap -p")
                        {
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using var proc = Process.Start(psi);
                        using var ms = new MemoryStream();
                        await proc.StandardOutput.BaseStream.CopyToAsync(ms);
                        ms.Position = 0;

                        var img = new BitmapImage();
                        img.BeginInit();
                        img.StreamSource = ms;
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.EndInit();
                        img.Freeze();

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (device != null && img != null && img.PixelHeight > 0)
                            {
                                device.Screenshot = img;
                                device.IsActive = true;
                            }
                        });
                    }
                    catch
                    {
                        if (System.Windows.Application.Current != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                device.IsActive = false;
                            });
                        }

                        await Task.Delay(500);
                    }
                }
            });
        }

        private void StopScreencap(ViewDeviceModel device)
        {
            if (_streamTokens.TryGetValue(device.DeviceId, out var cts))
            {
                cts.Cancel();
                _streamTokens.Remove(device.DeviceId);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

