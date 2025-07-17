using Newtonsoft.Json.Linq;
using Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ToolChange.Language;
using ToolChange.Models;
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for ShowDevice.xaml
    /// </summary>
    public partial class ShowDevice : Window
    {
        private string idDevice;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);



        private IntPtr _scrcpyHwnd = IntPtr.Zero;
        private System.Windows.Forms.Panel _panel;

        private Process scrcpyProcess;
        public ShowDevice(string idDevice, int deviceStt)
        {
            InitializeComponent();
            this.idDevice = idDevice;
            Loaded += ShowDevice_Loaded;
            deviceID.Content = "Device - " + deviceStt;

            var checkWifiStatus = RunAdbCommand($"-s {idDevice} shell settings get global wifi_on", 1000);

            if (checkWifiStatus == "1\r\n")
            {

                wifiSetting.Text = "Tắt Wi-Fi";
            }
            else
            {
                wifiSetting.Text = "Bật Wi-Fi";
            }
        }
        private bool isMenuVisible = false;
        private const int targetWidth = 150; // Width khi hiển thị menu
        private const int durationMs = 200;  // Thời gian chuyển đổi
        private async void ShowDevice_Loaded(object sender, RoutedEventArgs e)
        {
            MenuColumn.Width = new GridLength(0);        // Ẩn menu
            this.Width -= targetWidth;
            this.Title = "View device id : " + idDevice;

            InitPanel();
            await StartScrcpyAndAttach(idDevice);
        }
        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            int steps = 10;
            int durationMs = 200;
            int delay = durationMs / steps;
            int targetWidth = 150;

            int start = isMenuVisible ? targetWidth : 0;
            int end = isMenuVisible ? 0 : targetWidth;
            int delta = (end - start) / steps;

            var button = sender as System.Windows.Controls.Button; // Ép kiểu sender
            if (button != null)
                button.IsEnabled = false; // Vô hiệu hóa nút

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(delay)
            };

            int current = start;
            double windowStart = this.Width;
            double windowDelta = isMenuVisible ? -targetWidth : targetWidth;

            timer.Tick += (s, ev) =>
            {
                current += delta;

                bool reachedEnd = (delta > 0 && current >= end) || (delta < 0 && current <= end);
                if (reachedEnd)
                {
                    MenuColumn.Width = new GridLength(end);
                    this.Width = windowStart + windowDelta;
                    timer.Stop();
                    isMenuVisible = !isMenuVisible;

                    if (button != null)
                        button.IsEnabled = true; // Mở lại nút sau animation
                }
                else
                {
                    MenuColumn.Width = new GridLength(current);
                    this.Width = windowStart + ((double)(current - start));
                }
            };

            timer.Start();
        }
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button; // Ép kiểu sender
            if (button != null)
                button.IsEnabled = false; // Vô hiệu hóa nút

            if (scrcpyProcess != null && !scrcpyProcess.HasExited)
            {
                scrcpyProcess.Kill();
                scrcpyProcess.WaitForExit(); // Tuỳ chọn: đợi dừng hẳn
                scrcpyProcess.Dispose();
                scrcpyProcess = null;
            }

            await StartScrcpyAndAttach(idDevice);

            if (button != null)
                button.IsEnabled = true;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Cho phép kéo di chuyển cửa sổ
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }
        private void InitPanel()
        {
            _panel = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };
            ScrcpyHostView.Child = _panel;

        }
        private async Task StartScrcpyAndAttach(string device)
        {
            string scrcpyPath = @"./Resources/scrcpy.exe";
            string windowTitle = $"viewscrcpy";
            string args = $"-s {device} --window-title={windowTitle} --max-size 1080 --max-fps 15 " +
                          "--window-x 3000 --window-y 3000 --no-audio --window-width 350 --window-height 550 --lock-video-orientation=0";

            IntPtr existingWindow = FindWindow(null, windowTitle);
            if (existingWindow != IntPtr.Zero)
            {
                _ = GetWindowThreadProcessId(existingWindow, out uint pid);
                try
                {
                    var existingProcess = Process.GetProcessById((int)pid);
                    existingProcess.Kill();
                    existingProcess.WaitForExit();
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠ Không thể kill scrcpy cũ: {ex.Message}");
                }
            }

            var psi = new ProcessStartInfo(scrcpyPath, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };

            scrcpyProcess = Process.Start(psi);

            for (int i = 0; i < 50; i++) // ~10s timeout
            {
                await Task.Delay(500);
                _scrcpyHwnd = FindWindow(null, windowTitle);
                if (_scrcpyHwnd != IntPtr.Zero)
                    break;
            }

            if (_scrcpyHwnd != IntPtr.Zero)
            {
                var panelHandle = _panel.Handle;

                SetWindowLong(_scrcpyHwnd, -16, 0x40000000 | 0x10000000); // WS_CHILD | WS_VISIBLE
                SetParent(_scrcpyHwnd, panelHandle);
                SetWindowPos(_scrcpyHwnd, IntPtr.Zero, 0, 0, _panel.Width, _panel.Height, 0x0040); // SWP_SHOWWINDOW

                _panel.Resize += (_, __) => ResizeChild();
            }
            else
            {
                logDevice.Content = "Error";
                Debug.WriteLine("❌ Không tìm thấy cửa sổ scrcpy sau khi khởi động.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ResizeChild()
        {
            SetWindowPos(_scrcpyHwnd, IntPtr.Zero, 0, 0, _panel.Width, _panel.Height, 0x0040);
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (scrcpyProcess != null && !scrcpyProcess.HasExited)
            {
                scrcpyProcess.Kill();
                scrcpyProcess.WaitForExit(); // Tuỳ chọn: đợi dừng hẳn
                scrcpyProcess.Dispose();
                scrcpyProcess = null;
            }

            this.Close(); // Đóng cửa sổ hiện tại
        }
        private async void StartDevice_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            await Task.Run(() =>
            {
                SendAdbKeyEvent($"-s {idDevice} reboot"); // reboot
            });
        }

        private async void InstallApk_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file APK",
                Filter = "APK Files|*.apk"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string apkPath = openFileDialog.FileName;

                await Task.Run(() =>
                {
                    ADBService.RunAdbCommand($"-s {idDevice} install -r \"{apkPath}\"");
                });
                logDevice.Content = $"Install apk success";
                //System.Windows.MessageBox.Show($"Install apk success to id : {idDevice}", ViewDeviceLang.InfoSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AdbCommand_Click(object sender, RoutedEventArgs e)
        {
            logDevice.Content = "Adb shell";
            string commnadAdb = "";
            var vm = new InputViewModel();
            var dialog = new InputView
            {
                Title = "Adb",
                Height = 150,
                Width = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm,
            };

            vm.CloseAction = result =>
            {
                dialog.DialogResult = result;
                dialog.Close();
            };

            if (dialog.ShowDialog() == true)
            {
                commnadAdb = vm.InputText;
            }
            if (!string.IsNullOrEmpty(commnadAdb))
            {
                try
                {
                    logDevice.Content = $"Start adb command";
                    var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                    Task.Run(() =>
                    {
                    RunAdbCommand($"-s {idDevice} {commnadAdb}", 2000);
                        logDevice.Content = $"Success adb";
                    }).ContinueWith(task =>
                    {
                        logDevice.Content = $"Success adb";
                        //System.Windows.MessageBox.Show("Success fake ip", Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    }, currentTask);

                }
                catch (Exception ex)
                {
                    logDevice.Content = ex;
                    System.Windows.MessageBox.Show(ex.Message, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FakeProxy_Click(object sender, RoutedEventArgs e)
        {
            logDevice.Content = $"Fake proxy";
            string proxy = "";
            var vm = new InputViewModel();
            var dialog = new InputView
            {
                Title = DevicesLang.GetTitieProxy(deviceID.Content.ToString(), idDevice),
                Height = 150,
                Width = 300,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm,
            };

            vm.CloseAction = result =>
            {
                dialog.DialogResult = result;
                dialog.Close();
            };

            if (dialog.ShowDialog() == true)
            {
                proxy = vm.InputText;
            }

            if (!string.IsNullOrEmpty(proxy))
            {
                // ok
                try
                {
                    logDevice.Content = $"Start Fake proxy";

                    var peelProxy = proxy.Split(':');
                    var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                    Task.Run(() =>
                    {
                        var isFakeTimeZone = FakeTimeZone(proxy, idDevice);
                        if (isFakeTimeZone)
                        {
                            Thread.Sleep(10000);
                            string ip = peelProxy[0];
                            int port = int.Parse(peelProxy[1]);
                            string user = (peelProxy.Length >= 3) ? peelProxy[2] : "";
                            string password = (peelProxy.Length >= 4) ? peelProxy[3] : "";
                            ADBService.enableWifi(false, idDevice);
                            ADBService.rootAndRemount(idDevice);
                            ADBService.putSetting("http_proxy", ":0", idDevice);
                            RedSocksService.stop(idDevice);
                            if (ADBService.checkFileOnDevice("/data/local/tmp/redsocks.conf", idDevice))
                            {
                                RedSocksService.stop(idDevice);
                            }

                            RedSocksService.setUpRedSocksOnDevice("/data/local/tmp", idDevice);
                            RedSocksService.start(ip, port, "/data/local/tmp", idDevice, user, password);
                            ADBService.openWifiSettings(idDevice);
                            while (!ADBService.isWifiConnectedV2(idDevice) && !ADBService.isWifiConnected(idDevice))
                            {
                                ADBService.openWifiSettings(idDevice);
                                Thread.Sleep(3000);
                            }
                            Thread.Sleep(5000);
                            ADBService.OpenBrowserWithUrl("https://browserleaks.com/ip", idDevice);
                        }
                        else
                        {
                            return;
                        }
                        logDevice.Content = $"Success Fake proxy";
                    }).ContinueWith(task =>
                    {
                        logDevice.Content = $"Success Fake proxy";
                        //System.Windows.MessageBox.Show("Success fake ip", Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    }, currentTask);

                }
                catch (Exception ex)
                {
                    logDevice.Content = ex;
                    System.Windows.MessageBox.Show(ex.Message, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool FakeTimeZone(string proxy, string deviceId)
        {
            try
            {
                ADBService.enableWifi(false, deviceId);
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);
                if (proxyParts.Length == 4)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} --proxy-user {proxyParts[2]}:{proxyParts[3]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject jsonOblect = JObject.Parse(str);
                        ADBService.FakeTimezone(jsonOblect["timezone"].ToString(), deviceId);
                        return true;
                    }
                    else
                    {
                        // MessageBox.Show($"{ViewChangeStatic.logErrorFakeTimeZone} {proxyParts[0]} {ViewChangeStatic.logErrorFakeTimeZone1} {proxyParts[0]}", ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else if (proxyParts.Length == 2)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject jsonOblect = JObject.Parse(str);
                        ADBService.FakeTimezone(jsonOblect["timezone"].ToString(), deviceId);
                        return true;
                    }
                    else
                    {
                        // MessageBox.Show($"{ViewChangeStatic.logErrorFakeTimeZone} {proxyParts[0]} {ViewChangeStatic.logErrorFakeTimeZone1} {proxyParts[0]}", ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    //  MessageBox.Show(ViewChangeStatic.logFakeTimeZone, ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void Wifi_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                string deviceId = idDevice; // thay bằng ID thiết bị bạn đang dùng

                // Kiểm tra trạng thái Wi-Fi hiện tại
                var checkWifiStatus = RunAdbCommand($"-s {deviceId} shell settings get global wifi_on", 1000);

                if (checkWifiStatus == "1\r\n")
                {
                    Dispatcher.Invoke(() => wifiSetting.Text = "Bật Wi-Fi");
                    // Nếu đang bật → Tắt và trở về màn hình chính
                    RunAdbCommand($"-s {deviceId} shell svc wifi disable", 2000);
                   // RunAdbCommand($"-s {deviceId} shell input keyevent 3", 1000); // KEYCODE_HOME
                }
                else
                {
                    Dispatcher.Invoke(() => wifiSetting.Text = "Tắt Wi-Fi");
                    // Nếu đang tắt → Bật và mở màn hình Wi-Fi settings
                    RunAdbCommand($"-s {deviceId} shell svc wifi enable", 2000);
                    RunAdbCommand($"-s {deviceId} shell am start -a android.settings.WIFI_SETTINGS", 2000);
                }
            });
        }
        private string RunAdbCommand(string arguments, int timeoutMs = 5000)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "./Resources/adb.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(timeoutMs);
            return output;
        }

        private void SendAdbKeyEvent(string keyCode)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "./Resources/adb.exe",
                Arguments = $"{keyCode}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi gửi lệnh ADB: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendAdbKeyEvent($"-s {idDevice} shell input keyevent 4"); // KEYCODE_BACK
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SendAdbKeyEvent($"-s {idDevice} shell input keyevent 3"); // KEYCODE_HOME
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SendAdbKeyEvent($"-s {idDevice} shell input keyevent 187"); // KEYCODE_APP_SWITCH (overview)
        }
    }
}
