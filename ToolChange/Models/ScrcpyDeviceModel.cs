using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ToolChange.ViewModels;

namespace ToolChange.Models
{
    public class ScrcpyDeviceModel : INotifyPropertyChanged
    {
        public ScrcpyAttachStatus AttachStatus { get; set; } = ScrcpyAttachStatus.None;
        public string DisplayName => DeviceId == null ? $"Slot {Index} (Empty)" : $"Device {Index}";
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        private string _ip;
        public string Ip
        {
            get => _ip;
            set
            {
                if (_ip != value)
                {
                    _ip = value;
                    OnPropertyChanged(nameof(Ip));
                }
            }
        }
        public string DeviceId { get; set; }
        public int Index { get; set; }
        public string WindowTitle => $"{DeviceId}_{Index}";

        public IntPtr ScrcpyHwnd { get; set; }
        public Process ScrcpyProcess { get; set; }
        public Panel Panel { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
