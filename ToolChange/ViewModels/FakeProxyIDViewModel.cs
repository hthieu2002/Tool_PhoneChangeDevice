using System.ComponentModel;
using System.Windows.Input;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class FakeProxyIDViewModel : INotifyPropertyChanged
    {

        private string _proxyHost;
        private string _proxyPort;
        private string _proxyUsername;
        private string _proxyPassword;

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
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

        public FakeProxyIDViewModel()
        {
            OKCommand = new RelayCommandCD(o => CloseAction?.Invoke(true));
            CancelCommand = new RelayCommandCD(o => CloseAction?.Invoke(false));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
