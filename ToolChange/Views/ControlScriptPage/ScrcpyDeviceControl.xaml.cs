using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ToolChange.Models;
using ToolChange.Services;

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
            if (DataContext is ScrcpyDeviceModel vm && !string.IsNullOrEmpty(vm.DeviceId))
            {
                Debug.WriteLine($"[OpenScrcpy] Opening new scrcpy for device: {vm.DeviceId}");

                ShowDevice device = new ShowDevice(vm.DeviceId, vm.Index);
                device.ShowDialog();
            }

        }

        private void ScrcpyDeviceControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScrcpyDeviceModel vm && vm.Panel != null)
            {
                vm.Panel.Size = new System.Drawing.Size((int)ScrcpyHost.ActualWidth, (int)ScrcpyHost.ActualHeight);
                ScrcpyHost.Child = vm.Panel;
            }
        }

        private void LoadDevice_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ScrcpyDeviceModel model)
            {
                local.device = model;
                local.loadDevice = true;
            }
        }
    }
}
