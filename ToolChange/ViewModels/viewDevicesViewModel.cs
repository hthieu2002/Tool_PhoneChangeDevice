using OpenCvSharp;
using OpenCvSharp.Internal;
using Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToolChange.Language;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.Views;
using Xamarin.Forms;

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
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
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
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

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

        private readonly ConcurrentQueue<BitmapImage> _frames = new();
        private const int MaxQueue = 50; // tránh tràn RAM

        public ObservableCollection<ScrcpyDeviceModel> ViewDevices { get; set; } = new();
        private HashSet<string> _currentDeviceIds = new();
        private int deviceIndexCounter = 0;

        private Dictionary<string, int> _deviceIdToIndexMap = new();
        private Dictionary<int, ScrcpyDeviceModel> _indexToDeviceMap = new();
        private int _nextIndex = 1;
        public static CancellationTokenSource tokenSource { get; set; }
        public ObservableCollection<ScrcpyDeviceModel> DeviceSlots { get; set; } = new();
        public ICommand SelectDeviceCommand1 { get; }


        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x00080000;
        const int WS_MINIMIZEBOX = 0x00020000;
        const int WS_MAXIMIZEBOX = 0x00010000;
        public ObservableCollection<string> SelectedDeviceIds
        {
            get => _selectedDeviceIds;
            set
            {
                _selectedDeviceIds = value;
                OnPropertyChanged(nameof(SelectedDeviceIds));
            }
        }
        public double ItemWidth { get; }
        public double ItemHeight { get; }


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
        public ICommand ToggleDeviceCommand { get; }
        public ICommand DeviceClickCommand { get; set; }
        public viewDevicesViewModel()
        {
            ToggleDeviceCommand = new RelayCommand<ScrcpyDeviceModel>(ToggleSelectDevice);
            SelectedDeviceIds = new ObservableCollection<string>();
            SelectedDeviceCount = SelectedDeviceIds.Count;
            DeviceClickCommand = new RelayCommand<Models.ViewDeviceModel>(async (device) => await DeviceClick(device));

            if (!IsWindows11)
            {
                ItemWidth = 160;
                ItemHeight = 350;
            }
            else
            {
                ItemWidth = 200;
                ItemHeight = 400;
            }
        }

        private void ToggleSelectDevice(ScrcpyDeviceModel model)
        {
            if (model.IsSelected)
            {
                model.IsSelected = false;
                if (model.DeviceId != null && SelectedDeviceIds.Contains(model.DeviceId))
                    SelectedDeviceIds.Remove(model.DeviceId);
            }
            else
            {
                model.IsSelected = true;
                if (model.DeviceId != null && !SelectedDeviceIds.Contains(model.DeviceId))
                    SelectedDeviceIds.Add(model.DeviceId);
            }

            SelectedDeviceCount = SelectedDeviceIds.Count;
            Debug.WriteLine($"[Toggle] Selected devices: {SelectedDeviceIds.Count} → {string.Join(", ", SelectedDeviceIds)}");
            OnPropertyChanged(nameof(DeviceSlots));
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
            if (model == null || string.IsNullOrWhiteSpace(model.DeviceId))
                return;

            var scrcpyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "scrcpy.exe");

            if (!File.Exists(scrcpyPath))
            {
                System.Windows.MessageBox.Show("Không tìm thấy scrcpy.exe");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = scrcpyPath,
                Arguments = $"-s {model.DeviceId}",
                UseShellExecute = false,
                CreateNoWindow = true
            };


            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi chạy scrcpy: {ex.Message}");
            }
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
            //   System.Windows.MessageBox.Show(ViewDeviceLang.InfoViewDevice,Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
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
                                        $"--video-bit-rate={bitrate} " +
                                        $"--window-x 3000 --window-y 3000 " +
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

                        int style = GetWindowLong(hwnd, GWL_STYLE);
                        style &= ~WS_MINIMIZEBOX;
                        style &= ~WS_MAXIMIZEBOX;
                        SetWindowLong(hwnd, GWL_STYLE, style);

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
                    if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                    {
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
                    }
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

        public async Task<BitmapImage> CaptureScreenAsync(string deviceId, CancellationToken ct)
        {

            var psi = new ProcessStartInfo
            {
                FileName = "./Resources/adb.exe",
                Arguments = $"-s {deviceId} exec-out screencap -p ",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            using var ms = new MemoryStream();
            await proc.StandardOutput.BaseStream.CopyToAsync(ms, ct);
            ms.Position = 0;

            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = ms;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }

        public void TestOpenCV()
        {
            var mat = new Mat(480, 640, MatType.CV_8UC3, Scalar.Red);
            Cv2.ImWrite("test.jpg", mat);
            Console.WriteLine("Saved test.jpg");
        }
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
                        var stopwatch = Stopwatch.StartNew();
                        var psi = new ProcessStartInfo("adb", $"-s {device.DeviceId} exec-out screencap -p")
                        {
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using var proc = Process.Start(psi);


                        if (proc == null)
                        {
                            await Task.Delay(200);
                            continue;
                        }

                        using var ms = new MemoryStream();
                        await proc.StandardOutput.BaseStream.CopyToAsync(ms, cts.Token);
                        ms.Position = 0;

                        var img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.StreamSource = ms;
                        img.EndInit();
                        img.Freeze();

                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            device.Screenshot = img;
                            device.IsActive = true;
                        });

                        stopwatch.Stop();
                        //     Debug.WriteLine($"✅ {device.DeviceId} capture done in {stopwatch.ElapsedMilliseconds} ms");

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Error: {ex.Message}");
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => device.IsActive = false);
                        await Task.Delay(500);
                    }
                }
            }, cts.Token);
        }
        public void StopScreencap(ViewDeviceModel device)
        {
            if (!_streamTokens.TryGetValue(device.DeviceId, out var cts)) return;

            _streamTokens.Remove(device.DeviceId);

            _ = Task.Run(() =>
            {
                try { cts.Cancel(); }
                catch { /* nuốt mọi ngoại lệ từ callbacks */ }
                finally { cts.Dispose(); }
            });
        }

        //view
        public void startViewDevice()
        {
            _ = MonitorDevicesAsync();
        }

        public void stopViewDevice()
        {
            tokenSource?.Cancel();
            Task.Run(async () =>
            {
                foreach (var proc in Process.GetProcessesByName("scrcpy"))
                {
                    try
                    {
                        proc.Kill();
                        Debug.WriteLine($"[Monitor] Killed existing scrcpy process (PID: {proc.Id})");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Monitor] Failed to kill scrcpy (PID: {proc.Id}): {ex.Message}");
                    }
                    Task.Delay(1000).Wait(); // Đợi 1 giây trước khi tiếp tục
                }
            });
        }
        CancellationTokenSource ipMonitorToken = new CancellationTokenSource();
        private async Task MonitorDevicesAsync()
        {
            Debug.WriteLine("[Monitor] Start monitoring devices...");
            foreach (var proc in Process.GetProcessesByName("scrcpy"))
            {
                try
                {
                    proc.Kill();
                    Debug.WriteLine($"[Monitor] Killed existing scrcpy process (PID: {proc.Id})");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Monitor] Failed to kill scrcpy (PID: {proc.Id}): {ex.Message}");
                }
                await Task.Delay(1000);
            }
            tokenSource = new CancellationTokenSource();
           
            while (!tokenSource.IsCancellationRequested)
            {
                var connected = await GetConnectedDeviceIdsAsync();
                Debug.WriteLine($"[Monitor] Devices connected: {string.Join(", ", connected)}");

                var added = connected.Except(_currentDeviceIds).ToList();
                var removed = _currentDeviceIds.Except(connected).ToList();

                // Xử lý thêm thiết bị
                foreach (var id in added)
                {

                    Debug.WriteLine($"[Monitor] New device detected: {id}");

                    int index;

                    if (_deviceIdToIndexMap.ContainsKey(id))
                    {
                        index = _deviceIdToIndexMap[id]; // Đã từng kết nối → dùng lại index cũ
                    }
                    else
                    {
                        index = _nextIndex++;            // Device mới hoàn toàn → gán index mới
                        _deviceIdToIndexMap[id] = index;
                    }

                    var vm = new ScrcpyDeviceModel
                    {
                        DeviceId = id,
                        Index = index,
                        Panel = new System.Windows.Forms.Panel
                        {
                            Dock = System.Windows.Forms.DockStyle.Fill,
                            BackColor = System.Drawing.Color.Black
                        }
                    };

                    _indexToDeviceMap[index] = vm;
                    _ = Task.Run(() => StartMonitoringPublicIP(vm, tokenSource)) ;
                    RefreshDeviceSlotsFromMap();
                    if (!ViewDevices.Any(d => d.Index == index))
                    {
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ViewDevices.Add(vm);
                        });
                    }

                    await MonitorScrcpyAttachAsync(vm);
                }
                if (local.loadDevice)
                {
                    await MonitorScrcpyAttachAsync(local.device);
                    local.loadDevice = false;
                }
                // Xử lý ngắt thiết bị
                foreach (var id in removed)
                {
                    Debug.WriteLine($"[Monitor] Device removed: {id}");

                    ScrcpyDeviceModel? vm = null;
                    int? indexToRemove = null;

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        vm = ViewDevices.FirstOrDefault(d => d.DeviceId == id);
                        if (vm != null)
                        {
                            indexToRemove = vm.Index;
                            ViewDevices.Remove(vm);
                        }
                    });

                    if (indexToRemove != null)
                        _indexToDeviceMap.Remove(indexToRemove.Value);

                    if (vm != null)
                    {
                        try
                        {
                            vm.ScrcpyProcess?.Kill();
                            Debug.WriteLine($"[Monitor] Killed scrcpy process for {id}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[Monitor] Error killing process for {id}: {ex.Message}");
                        }
                    }
                }

                _currentDeviceIds = connected.ToHashSet();
                await Task.Delay(1000);
            }
        }
        private void RefreshDeviceSlotsFromMap()
        {
            DeviceSlots.Clear();
            foreach (var kv in _indexToDeviceMap.OrderBy(kv => kv.Key)) // đảm bảo thứ tự tăng dần
            {
                DeviceSlots.Add(kv.Value);
            }
        }

        private async Task<List<string>> GetConnectedDeviceIdsAsync()
        {
            var result = new List<string>();
            var psi = new ProcessStartInfo("./Resources/adb.exe", "devices")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            using var reader = process.StandardOutput;

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.EndsWith("\tdevice"))
                {
                    var id = line.Split('\t')[0];
                    result.Add(id);
                }
            }

            return result;
        }
        public static bool IsWindows11
        {
            get
            {
                var productName = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "ProductName", "")?.ToString();

                return productName != null && productName.Contains("Windows 11");
            }
        }
        public static async Task StartMonitoringPublicIP(ScrcpyDeviceModel device, CancellationTokenSource token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Debug.WriteLine($"[Connect] Đang lấy ip và quốc gia ip");
                    string command = $"adb -s {device.DeviceId} shell curl -s http://ip-api.com/json";
                    string output = ADBService.ExecuteAdbCommandString(command);

                    var ipMatch = Regex.Match(output, @"""query"":\s*""([^""]+)""");
                    var countryMatch = Regex.Match(output, @"""countryCode"":\s*""([^""]+)""");

                    string ip = ipMatch.Success ? ipMatch.Groups[1].Value : "0.0.0.0";
                    string country = countryMatch.Success ? countryMatch.Groups[1].Value : "Unknown";
                    device.Ip = $"{country} : {ip}";
                    Debug.WriteLine($"[Success] ip {ip} quốc gia {country}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LỖI] Không thể lấy IP/quốc gia từ thiết bị {device.DeviceId}: {ex.Message}");
                    device.Ip = $"Unknown : 0.0.0.0";
                }
                await Task.Delay(2000); // Kiểm tra mỗi 10 giây
            }
        }
        private async Task MonitorScrcpyAttachAsync(ScrcpyDeviceModel vm)
        {
            Debug.WriteLine($"[scrcpy] Starting monitor for device: {vm.DeviceId} (Index: {vm.Index})");

            int retry = 0;
            const int maxRetry = 5;
            const int delay = 2000;

            while (retry < maxRetry)
            {
                if (local.loadDevice)
                {
                    vm.AttachStatus = ScrcpyAttachStatus.None;
                }
                if (vm.AttachStatus == ScrcpyAttachStatus.Attaching || vm.AttachStatus == ScrcpyAttachStatus.Attached)
                {
                    Debug.WriteLine($"[scrcpy] Already attaching/attached device: {vm.DeviceId}, skipping duplicate attach.");
                    return;
                }
                vm.AttachStatus = ScrcpyAttachStatus.Attaching;


                if (vm.ScrcpyProcess != null && !vm.ScrcpyProcess.HasExited)
                {
                    try
                    {
                        vm.ScrcpyProcess.Kill();
                        Debug.WriteLine($"[scrcpy] Killed existing scrcpy process for {vm.DeviceId}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[scrcpy] Failed to kill scrcpy: {ex.Message}");
                    }

                    await Task.Delay(1000);
                }

                if (NativeMethods.FindWindow(null, vm.WindowTitle) != IntPtr.Zero)
                {
                    Debug.WriteLine($"[scrcpy] scrcpy window for {vm.DeviceId} already exists. Skipping start.");
                    return;
                }

                string scrcpyPath = @"./Resources/scrcpy.exe";
                //string args = $"-s {vm.DeviceId} --window-title={vm.WindowTitle} --max-size {Math.Min(1080, 2220)} --max-fps 15 " +
                //              "--window-borderless --window-x 3000 --window-y 3000 --no-control --no-audio --window-width 200 --window-height 400";
                string args = $"-s {vm.DeviceId} --window-title={vm.WindowTitle} --max-size 1080 --max-fps 15 " +
                        "--window-x 3000 --window-y 3000 --no-audio --window-width 200 --window-height 400 --lock-video-orientation=0";

                Debug.WriteLine($"[scrcpy] Starting process with args: {args}");

                var psi = new ProcessStartInfo(scrcpyPath, args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    vm.ScrcpyProcess = Process.Start(psi);
                    Debug.WriteLine($"[scrcpy] Process started for {vm.DeviceId}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[scrcpy] Failed to start scrcpy: {ex.Message}");
                }

                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);

                    vm.ScrcpyHwnd = NativeMethods.FindWindow(null, vm.WindowTitle);
                    if (vm.ScrcpyHwnd != IntPtr.Zero)
                    {
                        Debug.WriteLine($"[scrcpy] Found HWND for {vm.DeviceId}: {vm.ScrcpyHwnd}");

                        var panelHandle = vm.Panel?.Handle ?? IntPtr.Zero;

                        if (panelHandle != IntPtr.Zero)
                        {
                            NativeMethods.SetWindowLong(vm.ScrcpyHwnd, -16, 0x40000000 | 0x10000000);
                            NativeMethods.SetParent(vm.ScrcpyHwnd, panelHandle);
                            NativeMethods.SetWindowPos(vm.ScrcpyHwnd, IntPtr.Zero, 0, 0, vm.Panel.Width, vm.Panel.Height, 0x0040);
                         //   SetWindowPos(_scrcpyHwnd, IntPtr.Zero, 0, 0, _panel.Width, _panel.Height, 0x0040); // SWP_SHOWWINDOW
                            vm.Panel.Resize += (_, __) =>
                            {
                                NativeMethods.SetWindowPos(vm.ScrcpyHwnd, IntPtr.Zero, 0, 0,
                                    vm.Panel.Width, vm.Panel.Height, 0x0040);
                            };

                            Debug.WriteLine($"[scrcpy] Successfully attached scrcpy to panel for {vm.DeviceId}");
                            vm.AttachStatus = ScrcpyAttachStatus.Attached;

                            _ = MonitorScrcpyProcessHealthAsync(vm); // Start watchdog
                            return;
                        }
                        else
                        {
                            Debug.WriteLine($"[scrcpy] Panel handle is zero for {vm.DeviceId}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[scrcpy] HWND not found (attempt {i + 1}/10) for {vm.DeviceId}");
                    }
                }

                retry++;
                Debug.WriteLine($"[scrcpy] Retry {retry}/{maxRetry} for {vm.DeviceId}");
                await Task.Delay(delay);
            }

            Debug.WriteLine($"[scrcpy] Failed to attach scrcpy for {vm.DeviceId} after {maxRetry} retries.");
        }

        private async Task MonitorScrcpyProcessHealthAsync(ScrcpyDeviceModel vm)
        {
            while (true)
            {
                await Task.Delay(3000);

                if (vm.ScrcpyProcess == null || vm.ScrcpyProcess.HasExited || vm.ScrcpyHwnd == IntPtr.Zero)
                {
                    Debug.WriteLine($"[watchdog] scrcpy crashed or not attached for {vm.DeviceId}. Restarting...");
                    _ = MonitorScrcpyAttachAsync(vm);
                    return;
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);
    }
    public enum ScrcpyAttachStatus
    {
        None,
        Attaching,
        Attached
    }

}

