using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class InputCoordinateDialogViewModel : INotifyPropertyChanged
    {
        private string _x;
        public string X
        {
            get => _x;
            set
            {
                if (IsValidLongitude(value))
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
                else
                {
                    System.Windows.MessageBox.Show("Giá trị X (kinh độ) phải nằm trong khoảng -180 đến 180.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private string _y;
        public string Y
        {
            get => _y;
            set
            {
                if (IsValidLatitude(value))
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
                else
                {
                    System.Windows.MessageBox.Show("Giá trị Y (vĩ độ) phải nằm trong khoảng -90 đến 90.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

        public InputCoordinateDialogViewModel()
        {
            OKCommand = new RelayCommandCD(o => CloseAction?.Invoke(true));
            CancelCommand = new RelayCommandCD(o => CloseAction?.Invoke(false));
        }
      
        private bool IsValidLongitude(string value)
        {
            return double.TryParse(value, out double number) && number >= -180.0 && number <= 180.0;
        }

        private bool IsValidLatitude(string value)
        {
            return double.TryParse(value, out double number) && number >= -90.0 && number <= 90.0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
