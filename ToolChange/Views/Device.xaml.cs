using System.Windows;
using System.Windows.Controls;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.ViewModels;
using ComboBox = System.Windows.Controls.ComboBox;

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
            this.Unloaded += DevicePage_Unloaded;
            this.IsVisibleChanged += DevicePage_IsVisibleChanged;

        }
        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeviceViewModel vm)
            {
                var checkbox = sender as System.Windows.Controls.CheckBox;
                if (checkbox?.IsChecked is bool value)
                {
                    //   vm.DeviceListVM.OnSelectAllCheckboxClicked(value);
                }
            }
        }

        private void DevicePage_Unloaded(object sender, RoutedEventArgs e)
        {
            DeviceViewModel.StopLoop();
        }
        private void BrandComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            var selected = combo?.SelectedItem?.ToString();
            var vm = DataContext as DeviceViewModel;
            if (selected == "Random")
            {
                if (vm != null)
                {
                    vm.DeviceListVM.BrandRandom = true;
                }
            }
            else
            {
                if (vm != null)
                {
                    vm.DeviceListVM.BrandRandom = false;
                }
            }

        }
        private void OsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            var selected = combo?.SelectedItem?.ToString();
            var vm = DataContext as DeviceViewModel;
            if (selected == "Random")
            {
                if (vm != null)
                {
                    vm.DeviceListVM.OsRandom = true;
                }
            }
            else
            {
                if (vm != null)
                {
                    vm.DeviceListVM.OsRandom = false;
                }
            }

        }
        private void DevicePage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AutomationViewModel.StopLoop();
            Task.Delay(2000);
            ViewModelLocator.DeviceVM.DeviceListVM.AsyncTask();
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

                _ = ViewModelLocator.DeviceVM.DeviceListVM.SaveDevices();
            }
        }
        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            Setiing setting = new Setiing();
            setting.ShowDialog();
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            DeepDroid.Properties.Settings.Default.user = string.Empty;
            DeepDroid.Properties.Settings.Default.password = string.Empty;
            DeepDroid.Properties.Settings.Default.token = string.Empty;
            DeepDroid.Properties.Settings.Default.Save();

            var app = System.Windows.Application.Current;

            var login = new Login();
            app.MainWindow = login;

            if (Window.GetWindow(this) is Home home)
            {
                home.SuppressClosePrompt = true;
                home.Close();
            }
            foreach (Window w in app.Windows)
                if (!ReferenceEquals(w, login)) w.Close();

            login.Show();
        }

    }
}
