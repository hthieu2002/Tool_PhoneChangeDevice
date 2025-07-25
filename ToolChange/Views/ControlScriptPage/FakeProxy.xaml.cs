using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using ToolChange.Models;
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for FakeProxy.xaml
    /// </summary>
    public partial class FakeProxy : Window
    {
        private FakeProxyViewModel model;
        public FakeProxy(List<DeviceModel> allDevices)
        {
            InitializeComponent();
            model = new FakeProxyViewModel(allDevices);
            this.DataContext = model;

            this.Loaded += MainWindow_Loaded;

        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as FakeProxyViewModel;
            if (vm != null && vm.SelectedDevices != null)
            {
                vm.SelectedDevices.CollectionChanged += (s, args) => Dispatcher.Invoke(UpdateDeviceColumnVisibility);
                UpdateDeviceColumnVisibility(); // Cập nhật ban đầu
            }
        }
        private void UpdateDeviceColumnVisibility()
        {
            var vm = DataContext as FakeProxyViewModel;
            if (vm != null)
            {
                if (vm.SelectedDevices == null || !vm.SelectedDevices.Any())
                {
                    DeviceColumn.Width = new GridLength(0); // Ẩn cột
                    DeviceColumnRol.Width = new GridLength(0); // Ẩn cột
                }
                else
                {
                    DeviceColumn.Width = new GridLength(210); // Hiện cột
                    DeviceColumnRol.Width = new GridLength(3); // Hiện cột
                }
                 
            }
        }

        private void HostTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (sender is System.Windows.Controls.TextBox tb)
                {
                    var pastedText = System.Windows.Clipboard.GetText();
                    if (!string.IsNullOrEmpty(pastedText) && pastedText.Contains(":"))
                    {
                        var parts = pastedText.Split(':');
                        tb.Text = parts[0];

                        if (parts.Length > 1 && FindName("PortTextBox") is System.Windows.Controls.TextBox portBox)
                            portBox.Text = parts[1];
                        if (parts.Length > 2 && FindName("UsernameTextBox") is System.Windows.Controls.TextBox userBox)
                            userBox.Text = parts[2];
                        if (parts.Length > 3 && FindName("PasswordTextBox") is System.Windows.Controls.TextBox passBox)
                            passBox.Text = parts[3];

                        e.Handled = true;
                    }

                }
            }
        }

        private void ScrollViewer_BadgeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
}
