using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// Interaction logic for FakeTimeZone.xaml
    /// </summary>
    public partial class FakeTimeZone : Window
    {
        private FakeTimeZoneViewModel model;
        public FakeTimeZone(List<DeviceModel> allDevices)
        {
            InitializeComponent();
           
            model = new FakeTimeZoneViewModel(allDevices);
            this.DataContext = model;
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as FakeTimeZoneViewModel;
            if (vm != null && vm.SelectedDevices != null)
            {
                vm.SelectedDevices.CollectionChanged += (s, args) => Dispatcher.Invoke(UpdateDeviceColumnVisibility);
                UpdateDeviceColumnVisibility(); // Cập nhật ban đầu
            }
        }
        private void UpdateDeviceColumnVisibility()
        {
            var vm = DataContext as FakeTimeZoneViewModel;
            if (vm != null)
            {
                if (vm.SelectedDevices == null || !vm.SelectedDevices.Any())
                {
                    DeviceColumn.Width = new GridLength(0); // Ẩn cột
                    DeviceColumnRol.Width = new GridLength(0); // Ẩn cột
                }
                else
                {
                    DeviceColumn.Width = new GridLength(200); // Hiện cột
                    DeviceColumnRol.Width = new GridLength(3); // Hiện cột
                }

            }
        }
    }
}
