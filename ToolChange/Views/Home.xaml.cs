using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        private Dictionary<string, object> viewCache = new Dictionary<string, object>();
        public MainViewModel ViewModel { get; set; }
        public Home()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            BtnDevice.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            HomeFrame.Content = new Device();
        }
        
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private void CloseAllScrcpyWindows()
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                if (!string.IsNullOrEmpty(title) && title.StartsWith("device", StringComparison.OrdinalIgnoreCase))
                {
                    PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
                else if (!string.IsNullOrEmpty(title) && title.Length >= 8 && title.All(char.IsLetterOrDigit)) // Hoặc bạn sửa theo định dạng deviceId của bạn
                {
                    PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }

                return true;
            }, IntPtr.Zero);
        }

        private void BtnDevice_Click(object sender, RoutedEventArgs e)
        {
            BtnDevice.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            string viewKey = "Device";
            if (!viewCache.ContainsKey(viewKey))
            {
                viewCache[viewKey] = new Device();
            }
            HomeFrame.Content = viewCache[viewKey];
        }

        private void BtnAutomation_Click(object sender, RoutedEventArgs e)
        {
            BtnAutomation.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            string viewKey = "Automation";
            if (!viewCache.ContainsKey(viewKey))
            {
                viewCache[viewKey] = new Automation();
            }
            HomeFrame.Content = viewCache[viewKey];
        }

        private void BtnScreen_Click(object sender, RoutedEventArgs e)
        {
            BtnScreen.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            HomeFrame.Content = new ViewDevices();

        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            BtnSetting.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            Setiing setting= new Setiing();
            setting.ShowDialog();
        }

        private void BtnDocument_Click(object sender, RoutedEventArgs e)
        {
            BtnDocument.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            string viewKey = "Document";
            if (!viewCache.ContainsKey(viewKey))
            {
                viewCache[viewKey] = new Document();
            }
            HomeFrame.Content = viewCache[viewKey];
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseAllScrcpyWindows();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
