using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ToolChange.Models
{
    public class ViewDeviceModel : INotifyPropertyChanged
    {
        private BitmapImage _screenshot;
        private bool _isActive;

        public string DeviceId { get; set; }
        public string DisplayName => $"Device {Index + 1}";
        private int _index;

        public BitmapImage Screenshot
        {
            get => _screenshot;
            set { _screenshot = value; OnPropertyChanged(nameof(Screenshot)); }
        }
        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
