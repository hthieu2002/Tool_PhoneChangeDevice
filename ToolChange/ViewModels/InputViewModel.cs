using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class InputViewModel : INotifyPropertyChanged
    {
        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText)); 
            }
        }
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

        public InputViewModel()
        {
            OKCommand = new RelayCommandCD(o => CloseAction?.Invoke(true));
            CancelCommand = new RelayCommandCD(o => CloseAction?.Invoke(false));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
