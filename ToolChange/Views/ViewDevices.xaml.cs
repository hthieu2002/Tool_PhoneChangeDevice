using Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Media;
using ToolChange.Services;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    public class ScrcpyEmbedHost : HwndHost
    {
        private IntPtr _hostHwnd = IntPtr.Zero;
        private IntPtr _scrcpyHwnd = IntPtr.Zero;
        private Process _scrcpyProcess;
        private readonly string _windowTitle;
        private readonly string _deviceId;

        public ScrcpyEmbedHost(string deviceId)
        {
            _deviceId = deviceId;
            _windowTitle = $"WPF_SCRCPY_{deviceId}";
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _hostHwnd = NativeMethods.CreateWindowEx(
                0, "static", "",
                0x40000000 | 0x10000000, // WS_CHILD | WS_VISIBLE
                0, 0, 0, 0,
                hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero
            );

            StartScrcpyAndAttach();
            return new HandleRef(this, _hostHwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd) =>
            NativeMethods.DestroyWindow(hwnd.Handle);

        private async void StartScrcpyAndAttach()
        {
            string exe = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "scrcpy.exe");
            string args = $"-s {_deviceId} --window-title=\"{_windowTitle}\"";

            _scrcpyProcess = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (_scrcpyProcess == null) return;

            // Chờ scrcpy tạo cửa sổ
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(2000);
                _scrcpyHwnd = NativeMethods.FindWindow(null, _windowTitle);
                if (_scrcpyHwnd != IntPtr.Zero)
                    break;
            }

            if (_scrcpyHwnd != IntPtr.Zero)
            {
                NativeMethods.SetWindowLong(_scrcpyHwnd, -16, 0x40000000 | 0x10000000); // WS_CHILD | WS_VISIBLE
                NativeMethods.SetParent(_scrcpyHwnd, _hostHwnd);
                ResizeChild();
            }
        }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox) => ResizeChild();

        private void ResizeChild()
        {
            if (_scrcpyHwnd != IntPtr.Zero)
            {
                NativeMethods.SetWindowPos(_scrcpyHwnd, IntPtr.Zero, 0, 0,
                    (int)ActualWidth, (int)ActualHeight, 0x0040); // SWP_SHOWWINDOW
            }
        }

        public void Close()
        {
            if (_scrcpyProcess != null && !_scrcpyProcess.HasExited)
                _scrcpyProcess.Kill();

            if (_scrcpyHwnd != IntPtr.Zero)
                NativeMethods.SetParent(_scrcpyHwnd, IntPtr.Zero);

            _scrcpyHwnd = IntPtr.Zero;
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowEx(int exStyle, string lpClassName,
                string lpWindowName, int style, int x, int y, int w, int h,
                IntPtr parent, IntPtr menu, IntPtr hInstance, IntPtr param);

            [DllImport("user32.dll")] public static extern bool DestroyWindow(IntPtr hWnd);
            [DllImport("user32.dll")] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll")] public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
            [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, uint uFlags);
        }
    }
    public partial class ViewDevices : System.Windows.Controls.Page
    {
        private viewDevicesViewModel ViewModel => DataContext as viewDevicesViewModel;
        private ScrcpyEmbedHost _scrcpyHost;
        public ViewDevices()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ViewDevicesViewModel;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string deviceId = "93JAY0BKPF"; // ← Thay bằng deviceId thực tế
            _scrcpyHost = new ScrcpyEmbedHost(deviceId);
            HostGrid.Children.Add(_scrcpyHost);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _scrcpyHost?.Close();
        }
        /*  private void Page_Loaded(object sender, RoutedEventArgs e)
          {
              ViewModel?.StartMonitoring();
          }

          private void Page_Unloaded(object sender, RoutedEventArgs e)
          {
              ViewModel?.StopMonitoring();
          }*/
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                if (slider.Name == "ResolutionValueSlider")
                    ResolutionValue.Text = ((int)slider.Value).ToString();
                else if (slider.Name == "ScaleValueSlider")
                    ScaleValue.Text = ((int)slider.Value).ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Tắt màn hình khi xem đã được kích hoạt!");
        }
        private void CloseAllScrcpy_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CloseScrcpyWindows();
        }

    }
}