using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolChange.Models;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for ScrcpyDeviceControl.xaml
    /// </summary>
    public partial class ScrcpyDeviceControl : System.Windows.Controls.UserControl
    {
        public ScrcpyDeviceControl()
        {
            InitializeComponent();
            Loaded += ScrcpyDeviceControl_Loaded;
        }
        private void DeviceInfoClicked(object sender, MouseButtonEventArgs e)
        {
            //if (DataContext is ScrcpyDeviceModel vm && !string.IsNullOrEmpty(vm.DeviceId))
            //{
            //    Debug.WriteLine($"[OpenScrcpy] Opening new scrcpy for device: {vm.DeviceId}");

            //    var scrcpyPath = @"./Resources/scrcpy.exe";
            //    var args = $"-s {vm.DeviceId}";

            //    try
            //    {
            //        var psi = new ProcessStartInfo(scrcpyPath, args)
            //        {
            //            UseShellExecute = false,
            //            CreateNoWindow = true
            //        };

            //        Process.Start(psi);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Windows.MessageBox.Show($"Không thể mở scrcpy cho thiết bị {vm.DeviceId}:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //}
            ShowDevice device = new ShowDevice();
            device.ShowDialog();
        }

        private void ScrcpyDeviceControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScrcpyDeviceModel vm && vm.Panel != null)
            {
                vm.Panel.Size = new System.Drawing.Size((int)ScrcpyHost.ActualWidth, (int)ScrcpyHost.ActualHeight);
                ScrcpyHost.Child = vm.Panel;
            }
        }
    }
}
