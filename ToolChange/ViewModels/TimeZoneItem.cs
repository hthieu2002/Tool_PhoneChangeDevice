using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolChange.Models;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class FakeTimeZoneViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<TimezoneInfoModel> allTimezones;

        public ObservableCollection<string> GmtOffsets { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Timezones { get; set; } = new ObservableCollection<string>();

        private string selectedGmtOffset;
        public string SelectedGmtOffset
        {
            get => selectedGmtOffset;
            set
            {
                if (selectedGmtOffset != value)
                {
                    selectedGmtOffset = value;
                    OnPropertyChanged(nameof(SelectedGmtOffset));
                    UpdateTimezones();
                }
            }
        }

        private string selectedTimeZone;
        public string SelectedTimeZone
        {
            get => selectedTimeZone;
            set
            {
                if (selectedTimeZone != value)
                {
                    selectedTimeZone = value;
                    OnPropertyChanged(nameof(SelectedTimeZone));
                    UpdateCountryLabel();
                }
            }
        }

        private string countryLabel;
        public string CountryLabel
        {
            get => countryLabel;
            set
            {
                if (countryLabel != value)
                {
                    countryLabel = value;
                    OnPropertyChanged(nameof(CountryLabel));
                }
            }
        }
        private bool deviceALL = false;
        public bool DeviceALL
        {
            get => deviceALL;
            set
            {
                deviceALL = value;
                OnPropertyChanged(nameof(DeviceALL));
            }
        }
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

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
        public ObservableCollection<DeviceModel> AllDevices { get; set; }
        public ObservableCollection<DeviceModel> SelectedDevices { get; set; }
        public ICommand RemoveDeviceCommand { get; }
        public FakeTimeZoneViewModel(IEnumerable<DeviceModel> devices)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            List<TimezoneInfoModel> data = LoadFromExcel("./Resources/timezone.xlsx");

            allTimezones = data;

            var offsets = allTimezones
                .Select(x => x.GmtOffset)
                .Distinct()
                .OrderBy(x => x);

            foreach (var offset in offsets)
            {
                GmtOffsets.Add(offset);
                Debug.WriteLine(GmtOffsets.ToString());
            }

            OKCommand = new RelayCommandCD(o => CloseAction?.Invoke(true));
            CancelCommand = new RelayCommandCD(o => CloseAction?.Invoke(false));

            AllDevices = new ObservableCollection<DeviceModel>(devices);
            SelectedDevices = new ObservableCollection<DeviceModel>();
            RemoveDeviceCommand = new RelayCommand<DeviceModel>(RemoveDevice);
        }
        public void RemoveDevice(DeviceModel device)
        {
            if (SelectedDevices.Contains(device))
                SelectedDevices.Remove(device);
        }
        public List<TimezoneInfoModel> LoadFromExcel(string filePath)
        {
            var result = new List<TimezoneInfoModel>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];

            int row = 2;
            while (worksheet.Cells[row, 1].Value != null)
            {
                result.Add(new TimezoneInfoModel
                {
                    STT = int.Parse(worksheet.Cells[row, 1].Text),
                    CountryCode = worksheet.Cells[row, 2].Text,
                    CountryName = worksheet.Cells[row, 3].Text,
                    TimeZone = worksheet.Cells[row, 4].Text,
                    GmtOffset = worksheet.Cells[row, 5].Text
                });
                row++;
            }

            return result;
        }
        private void UpdateTimezones()
        {
            Timezones.Clear();
            var filtered = allTimezones
                .Where(x => x.GmtOffset == SelectedGmtOffset)
                .Select(x => x.TimeZone)
                .Distinct()
                .OrderBy(x => x);

            foreach (var tz in filtered)
                Timezones.Add(tz);
        }

        private void UpdateCountryLabel()
        {
            var item = allTimezones.FirstOrDefault(x => x.TimeZone == SelectedTimeZone && x.GmtOffset == SelectedGmtOffset);
            CountryLabel = item?.CountryName ?? "N/A";
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    } 
}
