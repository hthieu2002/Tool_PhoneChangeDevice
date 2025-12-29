using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ToolChange.Models;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class FakeProxyViewModel : INotifyPropertyChanged
    {
        private string _proxyHost;
        private string _proxyPort;
        private string _proxyUsername;
        private string _proxyPassword;

        private bool deviceALL = false;
        private string _typeProxy;

        public string ProxyHost
        {
            get => _proxyHost;
            set
            {
                _proxyHost = value;
                OnPropertyChanged(nameof(ProxyHost));
            }
        }
        public string ProxyPort
        {
            get => _proxyPort;
            set
            {
                _proxyPort = value;
                OnPropertyChanged(nameof(ProxyPort));
            }
        }
        public string ProxyUsername
        {
            get => _proxyUsername;
            set
            {
                _proxyUsername = value;
                OnPropertyChanged(nameof(ProxyUsername));
            }
        }
        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                _proxyPassword = value;
                OnPropertyChanged(nameof(ProxyPassword));
            }
        }
        public bool DeviceALL
        {
            get => deviceALL;
            set
            {
                deviceALL = value;
                OnPropertyChanged(nameof(DeviceALL));
            }
        }
        public string TypeProxy
        {
            get => _typeProxy;
            set
            {
                _typeProxy = value;
                if (string.IsNullOrWhiteSpace(TypeProxy))
                {
                    System.Windows.MessageBox.Show("Vui lòng chọn loại Proxy!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                OnPropertyChanged(nameof(TypeProxy));
            }
        }
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }
        public ObservableCollection<string> ProxyTypes { get; set; } = new ObservableCollection<string> { "HTTP", "Socks 5" };

        public Action<bool> CloseAction { get; set; }
        public ObservableCollection<DeviceModel> AllDevices { get; set; }
        public ObservableCollection<DeviceModel> SelectedDevices { get; set; }

        private DeviceModel _selectedDevice;
        public DeviceModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (value == null) return;

                // Nếu chưa có trong danh sách thì thêm
                if (!SelectedDevices.Any(d => d.DeviceId == value.DeviceId))
                {
                    SelectedDevices.Add(value);
                }

                _selectedDevice = null; // 🔥 Reset lại để cho phép chọn lại
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public ICommand RemoveDeviceCommand { get; }
        public FakeProxyViewModel(IEnumerable<DeviceModel> devices)
        {
            TypeProxy = ProxyTypes.First();
            AllDevices = new ObservableCollection<DeviceModel>(devices);
            SelectedDevices = new ObservableCollection<DeviceModel>();
            RemoveDeviceCommand = new RelayCommand<DeviceModel>(RemoveDevice);

            OKCommand = new RelayCommandCD(o => CloseAction?.Invoke(true));
            CancelCommand = new RelayCommandCD(o => CloseAction?.Invoke(false));
        }
        public void RemoveDevice(DeviceModel device)
        {
            if (SelectedDevices.Contains(device))
                SelectedDevices.Remove(device);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
