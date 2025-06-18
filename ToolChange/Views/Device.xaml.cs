using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Device.xaml
    /// </summary>
    public partial class Device : Page
    {
     //   public MainViewModel mainMV { get; set; }
        public Device()
        {
            InitializeComponent();
            //  mainMV = new MainViewModel();
            DataContext = ViewModelLocator.DeviceVM;
        }
        private void DeviceDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                DeviceContextMenu.PlacementTarget = dataGrid;
                DeviceContextMenu.IsOpen = true;
            }
            else
            {
                e.Handled = true; 
            }
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb && tb.DataContext is DeviceModel device)
            {
                string newName = tb.Text;
                string id = device.DeviceId;
                device.Name = newName;

                ViewModelLocator.DeviceVM.DeviceListVM.SaveDevices();
            }
        }

    }
}
