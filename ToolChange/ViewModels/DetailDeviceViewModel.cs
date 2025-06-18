using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.ViewModels
{
    public class DetailDeviceViewModel : INotifyPropertyChanged
    {
        private string _brand = "null";
        private string _name = "null";
        private string _model = "null";
        private string _os = "null";
        private string _serial = "null";
        private string _code = "null";
        private string _phone = "null";
        private string _imei = "null";
        private string _imsi = "null";
        private string _iccid = "null";
        private string _mac = "null";
        private string _country = "null";
        private string _sim = "null";
        private string _title = "null";

        public string Brand
        {
            get => _brand;
            set
            {
                _brand = value;
                OnPropertyChanged(nameof(Brand));
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
            }
        }
        public string Os
        {
            get => _os;
            set
            {
                if (value == "7")
                {
                    value = "Android 7";
                }
                if (value == "8")
                {
                    value = "Android 8";
                }
                if (value == "9")
                {
                    value = "Android 9";
                }
                if (value == "10")
                {
                    value = "Android 10";
                }
                if (value == "11")
                {
                    value = "Android 11";
                }
                if (value == "12")
                {
                    value = "Android 12";
                }
                if (value == "13")
                {
                    value = "Android 13";

                }
                _os = value;
                OnPropertyChanged(nameof(Os));
            }
        }
        public string Serial
        {
            get => _serial;
            set
            {
                _serial = value;
                OnPropertyChanged(nameof(Serial));
            }
        }
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                OnPropertyChanged(nameof(Code));
            }
        }
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }
        public string Imei
        {
            get => _imei;
            set
            {
                _imei = value;
                OnPropertyChanged(nameof(Imei));
            }
        }
        public string Imsi
        {
            get => _imsi;
            set
            {
                _imsi = value;
                OnPropertyChanged(nameof(Imsi));
            }
        }
        public string Iccid
        {
            get => _iccid;
            set
            {
                _iccid = value;
                OnPropertyChanged(nameof(Iccid));
            }
        }
        public string Mac
        {
            get => _mac;
            set
            {
                _mac = value;
                OnPropertyChanged(nameof(Mac));
            }
        }
        public string Country
        {
            get => _country;
            set
            {
                _country = value;
                OnPropertyChanged(nameof(Country));
            }
        }
        public string Sim
        {
            get => _sim;
            set
            {
                _sim = value;
                OnPropertyChanged(nameof(Sim));
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
