using AuthenticationService;
using Dynamitey.Internal.Optimization;
using Microsoft.Win32;
using MiHttpClient;
using Newtonsoft.Json.Linq;
using OpenPop.Mime;
using POCO.Models;
using Services;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.ViewModels.Constants;
using ToolChange.Views;
using ToolChange.Views.ControlScriptPage;

namespace ToolChange.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        public LocalizationViewModel LanguageVM { get; set; }
        public DeviceViewModel DeviceListVM { get; set; }

        private MiChangerGraphQLClient miChangerGraphQLClient;
        private POCO.Models.DeviceModel tempDeviceAll;
        private readonly HashSet<string> _processingDeviceIds = new();
        private string selectedFilePath;
        private string selectedFilePathJson;
        public ObservableCollection<SimCarrier> Countries { get; set; } = new();
        public ObservableCollection<POCO.Models.ComboBoxItem> SimOptions { get; set; } = new();
        private SimCarrier _selectedCountry;
        public SimCarrier SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (_selectedCountry != value)
                {
                    _selectedCountry = value;
                    OnPropertyChanged(nameof(SelectedCountry));
                    LoadSimOptions();
                }
            }
        }
        public POCO.Models.ComboBoxItem SelectedSim
        {
            get => _selectedSim;
            set
            {
                _selectedSim = value;
                OnPropertyChanged(nameof(SelectedSim));
            }
        }
        private POCO.Models.ComboBoxItem _selectedSim;
        private bool _checkSim = true;
        public bool IsCheckedSim
        {
            get => _checkSim;
            set
            {
                _checkSim = value;
                OnPropertyChanged(nameof(IsCheckedSim));
            }
        }
        private bool _checkKeyBox = false;
        public bool IsCheckedKeyBox
        {
            get => _checkKeyBox;
            set
            {
                _checkKeyBox = value;
                OnPropertyChanged(nameof(IsCheckedKeyBox));
            }
        }
        private bool _checkpif = false;
        public bool IsCheckedpif
        {
            get => _checkpif;
            set
            {
                _checkpif = value;
                OnPropertyChanged(nameof(IsCheckedpif));
            }
        }
        private List<SimCarrier> _telecomDataSource;
        private bool _isRandomButtonEnabled = true;
        private bool _isRandomButtonSimEnabled = true;
        private bool _isButtonChangeDevice = false;
        private bool _isButtonChangeFull = true;
        private bool _isButtonChangeSim = false;
        private bool _isButtonChangeSimFull = true;
        public bool IsRandomButtonEnabled
        {
            get => _isRandomButtonEnabled;
            set
            {
                if (_isRandomButtonEnabled != value)
                {
                    _isRandomButtonEnabled = value;
                    OnPropertyChanged(nameof(IsRandomButtonEnabled));
                }
            }
        }
        public bool IsRandomButtonSimEnabled
        {
            get => _isRandomButtonSimEnabled;
            set
            {
                if (_isRandomButtonSimEnabled != value)
                {
                    _isRandomButtonSimEnabled = value;
                    OnPropertyChanged(nameof(IsRandomButtonSimEnabled));
                }
            }
        }

        public bool IsButtonChangeDevice
        {
            get => _isButtonChangeDevice;
            set
            {
                if (_isButtonChangeDevice != value)
                {
                    _isButtonChangeDevice = value;
                    OnPropertyChanged(nameof(IsButtonChangeDevice));
                }
            }
        }

        public bool IsButtonChangeFull
        {
            get => _isButtonChangeFull;
            set
            {
                if (_isButtonChangeFull != value)
                {
                    _isButtonChangeFull = value;
                    OnPropertyChanged(nameof(IsButtonChangeFull));
                }
            }
        }

        public bool IsButtonChangeSim
        {
            get => _isButtonChangeSim;
            set
            {
                if (_isButtonChangeSim != value)
                {
                    _isButtonChangeSim = value;
                    OnPropertyChanged(nameof(IsButtonChangeSim));
                }
            }
        }

        public bool IsButtonChangeSimFull
        {
            get => _isButtonChangeSimFull;
            set
            {
                if (_isButtonChangeSimFull != value)
                {
                    _isButtonChangeSimFull = value;
                    OnPropertyChanged(nameof(IsButtonChangeSimFull));
                }
            }
        }
        private static readonly List<string> AvailableBrands = new List<string>
{
    "samsung",
    "OPPO",
    "vivo",
    "realme",
    "Google",
    "Xiaomi"
};
        private static readonly List<string> AvailableOs = new List<string>
{
    "24",
    "26",
    "27",
    "28",
    "29",
    "30",
    "31",
    "32"
};
        private void RandomizeBrand()
        {
            var random = new Random();
            int index = random.Next(AvailableBrands.Count);
            BrandValue = AvailableBrands[index];
        }
        private void RandomizeOs()
        {
            var random = new Random();
            int index = random.Next(AvailableOs.Count);
            OsValue = AvailableOs[index];
        }
        public ObservableCollection<string> DeviceTypes { get; } = new ObservableCollection<string>

{
    "Random",
    "Samsung",
    "Xiaomi",
    "Oppo",
    "Vivo",
    "Realme",
    "Google"
};
        public ObservableCollection<string> DeviceTypesOs { get; } = new ObservableCollection<string>

{
    "Random",
    "Android 7",
    "Android 8",
    "Android 8.1.0",
    "Android 9",
    "Android 10",
    "Android 11",
    "Android 12",
    "Android 13"
};
        private string _user = Properties.Settings.Default.user;
        private string _brand;
        private string _name;
        private string _model;
        private string _os;
        private string _serial;
        private string _code;
        private string _phone;
        private string _imei;
        private string _imsi;
        private string _iccid;
        private string _mac;
        private string _brandValue;
        private string _osValue;
        private string _osValueMax;
        public string BrandValue
        {
            get => _brandValue;
            set
            {
                _brandValue = value;
                OnPropertyChanged(nameof(BrandValue));
            }
        }
        public string OsValue
        {
            get => _osValue;
            set
            {
                _osValue = value;
                OnPropertyChanged(nameof(OsValue));
            }
        }
        public string OsValueMax
        {
            get => _osValueMax;
            set
            {
                _osValueMax = value;
                OnPropertyChanged(nameof(OsValueMax));
            }
        }
        public string Brand
        {
            get => _brand;
            set
            {
                if (value == "samsung")
                {
                    value = "Samsung";
                }
                if (value == "Xiaomi")
                {
                    value = "Xiaomi";
                }
                if (value == "OPPO")
                {
                    value = "Oppo";
                }
                if (value == "vivo")
                {
                    value = "Vivo";
                }
                if (value == "realme")
                {
                    value = "Realme";
                }
                if (value == "Google")
                {
                    value = "Google";
                }
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
                if (value == "7.1.0")
                {
                    value = "Android 7.1.0";
                }
                if (value == "8")
                {
                    value = "Android 8";
                }
                if (value == "8.1.0")
                {
                    value = "Android 8.1.0";
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
        public string User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    Properties.Settings.Default.user = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(User));
                }
            }
        }
        public ObservableCollection<Models.DeviceModel> Devices { get; private set; } = new ObservableCollection<Models.DeviceModel>();
        public ICommand DeleteDeviceCommand { get; private set; }
        public ICommand CopyDeviceIdCommand { get; private set; }
        public ICommand RandomDeviceCommand { get; private set; }
        public ICommand RandomSimCommand { get; private set; }
        public ICommand ChangeDeviceCommand { get; private set; }
        public ICommand IsCheckBoxDevice { get; private set; }
        public ICommand AutoChangeFullCommand { get; private set; }
        public ICommand ChangeSimCommand { get; private set; }
        public ICommand AutoChangeSimCommand { get; private set; }
        public ICommand ScreenshotCommand { get; private set; }
        public ICommand FakeLocationCommand { get; private set; }
        public ICommand DetailsDeviceIdCommand { get; private set; }
        public ICommand FakeProxyDeviceIdCommand { get; private set; }
        public ICommand OpenUrlCommand { get; private set; }

        private readonly string jsonFilePath = Path.Combine("Resources", "Devices", "devices.json");

        public event PropertyChangedEventHandler PropertyChanged;
        public DeviceViewModel()
        {
            LoadDevices();
            LoadData();
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += async (s, e) =>
            {
                await AddNewDevicesFromAdb();
                await UpdateDevicesStatus();
            };
            timer.Start();
            Brand = DeviceTypes.First();

            DeleteDeviceCommand = new RelayCommand<object>(DeleteDevice, CanDeleteDevice);
            CopyDeviceIdCommand = new RelayCommand<Models.DeviceModel>(CopyDeviceId);
            DetailsDeviceIdCommand = new RelayCommand<Models.DeviceModel>(DetailsDevices);
            FakeProxyDeviceIdCommand = new RelayCommand<Models.DeviceModel>(FakeProxyDeviceId);
            RandomDeviceCommand = new RelayCommand(async () => await RandomDevice());
            RandomSimCommand = new RelayCommand(async () => await RandomSim());
            ChangeDeviceCommand = new RelayCommand(async () => await ChangeDevice());
            AutoChangeFullCommand = new RelayCommand(async () => await AutoChangeFull());
            ChangeSimCommand = new RelayCommand(async () => await ChangeSim());
            AutoChangeSimCommand = new RelayCommand(async () => await AutoChangeSim());
            ScreenshotCommand = new RelayCommand(async () => await Screenshot());
            FakeLocationCommand = new RelayCommand(async () => await FakeLocation());
            OpenUrlCommand = new RelayCommand(async () => await OpenUrl());
            IsCheckBoxDevice = new RelayCommand<Models.DeviceModel>(CheckBoxDevice);
        }
        private void LoadDevices()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(jsonFilePath));

                if (File.Exists(jsonFilePath))
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    Devices = string.IsNullOrWhiteSpace(jsonContent)
                        ? new ObservableCollection<Models.DeviceModel>()
                        : JsonSerializer.Deserialize<ObservableCollection<Models.DeviceModel>>(jsonContent) ?? new ObservableCollection<Models.DeviceModel>();
                }
                else
                {
                    Devices = new ObservableCollection<Models.DeviceModel>();
                }

                // Update index for loaded devices
                int index = 1;
                foreach (var device in Devices)
                {
                    device.Index = index++;
                }

                OnPropertyChanged(nameof(Devices));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading devices: {ex.Message}");
            }
        }
        private async Task AddNewDevicesFromAdb()
        {
            try
            {
                var adbDevices = await GetDevicesFromAdbAsync();
                var existingDeviceIds = Devices.Select(d => d.DeviceId).ToHashSet();
                int maxIndex = Devices.Any() ? Devices.Max(d => d.Index) : 0;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var adbDevice in adbDevices)
                    {
                        if (!existingDeviceIds.Contains(adbDevice.DeviceId))
                        {
                            adbDevice.Index = ++maxIndex;
                            adbDevice.IsChecked = true;
                            Devices.Add(adbDevice);
                        }
                    }
                });

                if (adbDevices.Any(d => !existingDeviceIds.Contains(d.DeviceId)))
                {
                    SaveDevices();
                    // AddLog("Added new devices successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task UpdateDevicesStatus()
        {
            try
            {
                var adbDevices = await GetDevicesFromAdbAsync();
                var adbDeviceDict = adbDevices.ToDictionary(d => d.DeviceId, d => d);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var device in Devices)
                    {
                        if (adbDeviceDict.TryGetValue(device.DeviceId, out var adbDevice))
                        {
                            string newStatus = adbDevice.Status;
                            string newActive = ADBService.CheckDeviceActive(device.DeviceId);

                            if (device.Status != newStatus || device.Active != newActive)
                            {
                                device.Status = newStatus;
                                device.Active = newActive;
                            }
                        }
                        else if (device.Status != "Offline" || device.Active != "NO")
                        {
                            device.Status = "Offline";
                            device.Active = "NO";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task<ObservableCollection<Models.DeviceModel>> GetDevicesFromAdbAsync()
        {
            var devices = new ObservableCollection<Models.DeviceModel>();
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "./Resources/adb",
                        Arguments = "devices",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split('\t');
                    if (parts.Length == 2)
                    {
                        string deviceId = parts[0];
                        string status = parts[1].ToLower() == "device" ? "Online" : "Offline";
                        devices.Add(new Models.DeviceModel
                        {
                            DeviceId = deviceId,
                            Status = status,
                            Name = "",
                            Percentage = "0%",
                            Progress = "",
                            Active = status == "Online" ? ADBService.CheckDeviceActive(deviceId) : "NO"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return devices;
        }
        public void SaveDevices()
        {
            try
            {
                var json = JsonSerializer.Serialize(Devices, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving devices: {ex.Message}");
            }
        }
        private void DeleteDevice(object parameter)
        {
            if (parameter is Models.DeviceModel device)
            {
                var deviceToRemove = Devices.FirstOrDefault(d => d.DeviceId == device.DeviceId);
                if (deviceToRemove != null)
                {
                    Devices.Remove(deviceToRemove);
                    SaveDevices();
                    LoadDevices();
                }
            }
        }
        private void CopyDeviceId(Models.DeviceModel device)
        {
            if (device != null && !string.IsNullOrEmpty(device.DeviceId))
            {
                System.Windows.Clipboard.SetText(device.DeviceId);
            }
        }
        private void DetailsDevices(Models.DeviceModel device)
        {
            var vm = new DetailDeviceViewModel();
            var dialog = new DetailDevicesView
            {
                Title = "Details device " + device.DeviceId,
                Height = 500,
                Width = 350,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm,
            };

            string brand = GetDeviceInfoFromADB(device.DeviceId, "getprop ro.product.brand");
            string name = GetDeviceInfoFromADB(device.DeviceId, "getprop ro.android.board");
            string model = GetDeviceInfoFromADB(device.DeviceId, "getprop ro.product.model");
            string os1 = GetDeviceInfoFromADB(device.DeviceId, "getprop ro.bootimage.build.version.release");
            string os2 = GetDeviceInfoFromADB(device.DeviceId, "getprop ro.build.version.sdk");
            string country = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_sim_operator_country");
            string sim = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_sim_operator_name");
            string serial = GetDeviceInfoFromADB(device.DeviceId, "getprop ro.serialno");
            string codeSim = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_sim_operator_numeric");
            string phone = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_line1_number");
            string imei = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_imei_number");
            string imsi = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_imsi");
            string iccid = GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_iccid");
            string mac = GetDeviceMACAddress(device.DeviceId);

            vm.Brand = brand;
            vm.Name = name;
            vm.Model = model;
            vm.Os = os1;
            vm.Country = country;
            vm.Serial = serial;
            vm.Code = codeSim;
            vm.Phone = phone;
            vm.Imei = imei;
            vm.Imsi = imsi;
            vm.Iccid = iccid;
            vm.Mac = mac;
            vm.Title = "Thông tin thiết bị";
            dialog.ShowDialog();

        }
        private void FakeProxyDeviceId(Models.DeviceModel device)
        {
            string proxy = "";
            var vm = new InputViewModel();
            var dialog = new InputView
            {
                Title = "Nhập proxy cho thiết bị " + device.Name + " có id " + device.DeviceId,
                Height = 150,
                Width = 300,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm,
            };

            vm.CloseAction = result =>
            {
                dialog.DialogResult = result;
                dialog.Close();
            };

            if (dialog.ShowDialog() == true)
            {
                proxy = vm.InputText;
            }

            if (!string.IsNullOrEmpty(proxy))
            {
                // ok
                try
                {
                    var peelProxy = proxy.Split(':');
                    var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                    Task.Run(() =>
                    {
                        var isFakeTimeZone = FakeTimeZone(proxy, device.DeviceId);
                        if (isFakeTimeZone)
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "Faking proxy...");
                            Thread.Sleep(10000);
                            UpdateDeviceStatus(device.DeviceId, "10%", "Faking proxy...");
                            string ip = peelProxy[0];
                            int port = int.Parse(peelProxy[1]);
                            string user = (peelProxy.Length >= 3) ? peelProxy[2] : "";
                            string password = (peelProxy.Length >= 4) ? peelProxy[3] : "";
                            ADBService.enableWifi(false, device.DeviceId);
                            ADBService.rootAndRemount(device.DeviceId);
                            ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                            UpdateDeviceStatus(device.DeviceId, "30%", "Faking proxy...");
                            RedSocksService.stop(device.DeviceId);
                            if (ADBService.checkFileOnDevice("/data/local/tmp/redsocks.conf", device.DeviceId))
                            {
                                RedSocksService.stop(device.DeviceId);
                            }

                            UpdateDeviceStatus(device.DeviceId, "40%", "Faking proxy...");
                            RedSocksService.setUpRedSocksOnDevice("/data/local/tmp", device.DeviceId);

                            RedSocksService.start(ip, port, "/data/local/tmp", device.DeviceId, user, password);
                            UpdateDeviceStatus(device.DeviceId, "50%", "Faking proxy...");
                            ADBService.openWifiSettings(device.DeviceId);
                            UpdateDeviceStatus(device.DeviceId, "60%", "Faking proxy...");
                            while (!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId))
                            {
                                ADBService.openWifiSettings(device.DeviceId);
                                Thread.Sleep(3000);
                            }
                            Thread.Sleep(5000);
                            ADBService.OpenBrowserWithUrl("https://browserleaks.com/ip", device.DeviceId);
                            UpdateDeviceStatus(device.DeviceId, "99%", "Faking proxy Done");

                        }
                        else
                        {
                            return;
                        }
                    }).ContinueWith(task =>
                    {
                        UpdateDeviceStatus(device.DeviceId, "100%", "Faking proxy Done");
                    }, currentTask);

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool CanDeleteDevice(object parameter)
        {
            return parameter is Models.DeviceModel;
        }
        private async void LoadData()
        {
            _telecomDataSource = await Task.Run(() => JsonService<SimCarrier>.loadConfigurationFromResource("carriers.json"));

            var simCarriers = _telecomDataSource
                .GroupBy(c => c.CountryName)
                .Select(c => c.First())
                .OrderBy(c => c.CountryName)
                .ToList();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Countries.Clear();
                foreach (var country in simCarriers)
                    Countries.Add(country);
                SelectedCountry = Countries.FirstOrDefault(c => c.CountryName == "Abkhazia");
            });
        }
        private void LoadSimOptions()
        {
            if (SelectedCountry == null || SelectedCountry.Attribute == null) return;

            var simList = _telecomDataSource
                .FindAll(c => c.Attribute.Mcc == SelectedCountry.Attribute.Mcc)
                .Select(c => new POCO.Models.ComboBoxItem
                {
                    Name = c.Name + "-" + c.Attribute.Mnc,
                    Value = c.Attribute.Mnc
                }).ToList();

            SimOptions.Clear();
            foreach (var sim in simList)
                SimOptions.Add(sim);

            if (SimOptions.Any())
            {
                SelectedSim = SimOptions.First();
            }
        }
        public async Task<POCO.Models.DeviceModel> RandomDevicePrivate()
        {
            POCO.Models.DeviceModel tempDevice = null;
            if (miChangerGraphQLClient == null)
            {
                CreateService();
            }

            var currentSelectedCarrier = SelectedSim;
            var currentSelectedCountry = SelectedCountry;
            var mcc = SelectedCountry?.Attribute?.Mcc;
            var mnc = SelectedSim?.Value;

            Console.WriteLine("Country Code = {0}. MCC = {1} while carrier name = {2} MNC = {3}"
                , currentSelectedCountry.CountryCode
                , mcc
                , currentSelectedCarrier.Name
                , mnc);

            try
            {
                if (Brand == "Random")
                {
                    // brand random
                    RandomizeBrand();
                }
                else
                {
                    if (Brand == "Samsung")
                    {
                        BrandValue = "samsung";
                    }
                    else if (Brand == "Oppo")
                    {
                        BrandValue = "OPPO";
                    }
                    else if (Brand == "Vivo")
                    {
                        BrandValue = "vivo";
                    }
                    else if (Brand == "Realme")
                    {
                        BrandValue = "realme";
                    }
                    else if (Brand == "Google")
                    {
                        BrandValue = "Google";
                    }
                    else
                    {
                        BrandValue = "Xiaomi";
                    }
                }
                if (Os == "Random")
                {
                    // brand random
                    RandomizeOs();
                }
                else
                {
                    if (Os == "Android 8.1.0")
                    {
                        OsValue = "27";
                    }
                    else if (Os == "Android 7")
                    {
                        OsValue = "30";
                        // OsValueMax = "25";
                    }
                    else if (Os == "Android 8")
                    {
                        OsValue = "26";
                        // OsValueMax = "27";
                    }
                    else if (Os == "Android 9")
                    {
                        OsValue = "28";
                        //   OsValueMax = "28";
                    }
                    else if (Os == "Android 10")
                    {
                        OsValue = "29";
                        //  OsValueMax = "29";
                    }
                    else if (Os == "Android 11")
                    {
                        OsValue = "30";
                        //   OsValueMax = "30";
                    }
                    else if (Os == "Android 12")
                    {
                      //  System.Windows.MessageBox.Show("Hiện tại chưa random android 12");
                        OsValue = "31";
                        // OsValueMax = "31";
                    }
                    else if (Os == "Android 13")
                    {
                       // System.Windows.MessageBox.Show("Hiện tại chưa random android 13");
                        OsValue = "32";
                        // OsValueMax = "32";
                    }
                    else
                    {
                        OsValue = "29";
                    }
                }
                if (BrandValue == "Google")
                {
                    OsValue = "30";
                }
                if (BrandValue == "realme" || BrandValue == "vivo")
                {
                    if (OsValue == "29" || OsValue == "30")
                    {

                    }
                    else
                    {
                        OsValue = "29";
                    }
                }
                if (BrandValue == "OPPO" || BrandValue == "Xiaomi")
                {
                    if (OsValue == "29" || OsValue == "30" || OsValue == "28" || OsValue == "27")
                    {

                    }
                    else
                    {
                        OsValue = "29";
                    }
                }

                tempDevice = await miChangerGraphQLClient.GetRandomDeviceV3(brand: BrandValue,sdkMin: int.Parse(OsValue), sdkMax: int.Parse(OsValue));
                if (tempDevice.Model == null)
                {
                    throw new Exception("Devices not existed, please try again");
                }

                tempDevice.IMSI = RandomService.generateIMSI(mcc, mnc);
                tempDevice.ICCID = RandomService.generateICCID(currentSelectedCountry.CountryCode, mnc);
                tempDevice.SerialNo = RandomService.getRandomStringHex16Digit().Substring(0, RandomService.randomInRange(8, 13));
                tempDevice.SimPhoneNumber = string.Format("+{0}{1}", currentSelectedCountry.CountryCode, RandomService.generatePhoneNumber());
                tempDevice.SimOperatorNumeric = string.Concat(mcc, mnc);
                tempDevice.SimOperatorCountry = currentSelectedCountry.CountryIso;
                tempDevice.SimOperatorName = currentSelectedCarrier.Name.Substring(0, currentSelectedCarrier.Name.LastIndexOf("-")).Replace("&", "^&");
                tempDevice.AndroidId = RandomService.getRandomStringHex16Digit();
                tempDevice.WifiMacAddress = RandomService.generateWifiMacAddress();
                tempDevice.BlueToothMacAddress = RandomService.generateMacAddress();
            }
            catch (Exception ex)
            {
                //ignored
                Console.WriteLine(ex);
            }
            finally
            {

            }
            return tempDevice;
        }
        private async Task RandomDevice()
        {
            IsRandomButtonEnabled = false;

            if (miChangerGraphQLClient == null)
            {
                CreateService();
            }
            var currentSelectedCarrier = SelectedSim;
            var currentSelectedCountry = SelectedCountry;
            var mcc = SelectedCountry?.Attribute?.Mcc;
            var mnc = SelectedSim?.Value;

            Console.WriteLine("Country Code = {0}. MCC = {1} while carrier name = {2} MNC = {3}"
                , currentSelectedCountry.CountryName
                , mcc
                , currentSelectedCarrier.Name
                , mnc);

            try
            {

                if (Brand == "Random")
                {
                    // brand random
                    RandomizeBrand();
                }
                else
                {
                    if (Brand == "Samsung")
                    {
                        BrandValue = "samsung";
                    }
                    else if (Brand == "Oppo")
                    {
                        BrandValue = "OPPO";
                    }
                    else if (Brand == "Vivo")
                    {
                        BrandValue = "vivo";
                    }
                    else if (Brand == "Realme")
                    {
                        BrandValue = "realme";
                    }
                    else if (Brand == "Google")
                    {
                        BrandValue = "Google";
                    }
                    else
                    {
                        BrandValue = "Xiaomi";
                    }
                }
                if (Os == "Random")
                {
                    // brand random
                    RandomizeOs();
                }
                else
                {
                   if (Os == "Android 8.1.0")
                    {
                        OsValue = "27";
                    }
                    else if (Os == "Android 7")
                    {
                        OsValue = "30";
                        // OsValueMax = "25";
                    }
                    else if (Os == "Android 8")
                    {
                        OsValue = "26";
                        // OsValueMax = "27";
                    }
                    else if (Os == "Android 9")
                    {
                        OsValue = "28";
                        //   OsValueMax = "28";
                    }
                    else if (Os == "Android 10")
                    {
                        OsValue = "29";
                        //  OsValueMax = "29";
                    }
                    else if (Os == "Android 11")
                    {
                        OsValue = "30";
                        //   OsValueMax = "30";
                    }
                    else if (Os == "Android 12")
                    {
                      //  System.Windows.MessageBox.Show("Hiện tại chưa random android 12");
                         OsValue = "31";
                        // OsValueMax = "31";
                    }
                    else if (Os == "Android 13")
                    {
                      // System.Windows.MessageBox.Show("Hiện tại chưa random android 13");
                         OsValue = "32";
                        // OsValueMax = "32";
                    }
                    else
                    {
                        OsValue = "29";
                    }
                }
                if (BrandValue == "Google")
                {
                    OsValue = "30";
                }
                if (BrandValue == "realme" || BrandValue == "vivo")
                {
                    if (OsValue == "29" || OsValue == "30")
                    {

                    }
                    else
                    {
                        return;
                    }
                }
                if (BrandValue == "OPPO" || BrandValue == "Xiaomi")
                {
                    if (OsValue == "29" || OsValue == "30" || OsValue == "28" || OsValue == "27")
                    {

                    }
                    else
                    {
                        return;
                    }
                }

                tempDeviceAll = await miChangerGraphQLClient.GetRandomDeviceV3(brand: BrandValue, sdkMin: int.Parse(OsValue), sdkMax: int.Parse(OsValue));
                if (tempDeviceAll.Model == null)
                {
                    throw new Exception("Devices not existed, please try again");
                }
                Brand = tempDeviceAll.Manufacturer;
                Name = tempDeviceAll.Board;
                Model = tempDeviceAll.Model;
                Os = tempDeviceAll.Release;
                Serial = tempDeviceAll.SerialNo = RandomService.getRandomStringHex16Digit().Substring(0, RandomService.randomInRange(8, 13));
                Code = tempDeviceAll.SimOperatorNumeric = string.Concat(mcc, mnc);
                Phone = tempDeviceAll.SimPhoneNumber = string.Format("+{0}{1}", currentSelectedCountry.CountryCode, RandomService.generatePhoneNumber());
                Imei = tempDeviceAll.Imei;
                Imsi = tempDeviceAll.IMSI = RandomService.generateIMSI(mcc, mnc);
                Iccid = tempDeviceAll.ICCID = RandomService.generateICCID(currentSelectedCountry.CountryCode, mnc);
                Mac = RandomService.generateMacAddress();

                tempDeviceAll.SimOperatorCountry = currentSelectedCountry.CountryIso;
                tempDeviceAll.SimOperatorName = currentSelectedCarrier.Name.Substring(0, currentSelectedCarrier.Name.LastIndexOf("-")).Replace("&", "^&");
                tempDeviceAll.AndroidId = RandomService.getRandomStringHex16Digit();
                tempDeviceAll.WifiMacAddress = RandomService.generateWifiMacAddress();
                tempDeviceAll.BlueToothMacAddress = RandomService.generateMacAddress();

                IsButtonChangeDevice = true;
                IsButtonChangeFull = true;

            }
            catch (Exception ex)
            {
                //ignored
                Console.WriteLine(ex);
            }
            finally
            {
                IsRandomButtonEnabled = true;
            }
        }
        private async Task RandomSim()
        {
            IsRandomButtonSimEnabled = false;
            if (miChangerGraphQLClient == null)
            {
                CreateService();
            }
            var currentSelectedCarrier = SelectedSim;
            var currentSelectedCountry = SelectedCountry;
            var mcc = SelectedCountry?.Attribute?.Mcc;
            var mnc = SelectedSim?.Value;

            Console.WriteLine("Country Code = {0}. MCC = {1} while carrier name = {2} MNC = {3}"
                , currentSelectedCountry.CountryName
                , mcc
                , currentSelectedCarrier.Name
                , mnc);

            try
            {

                tempDeviceAll = await miChangerGraphQLClient.GetRandomDeviceV3(sdkMin: 30);
                if (tempDeviceAll.Model == null)
                {
                    throw new Exception("Devices not existed, please try again");
                }
                tempDeviceAll.IMSI = RandomService.generateIMSI(mcc, mnc);
                tempDeviceAll.ICCID = RandomService.generateICCID(currentSelectedCountry.CountryCode, mnc);
                tempDeviceAll.SimPhoneNumber = string.Format("+{0}{1}", currentSelectedCountry.CountryCode, RandomService.generatePhoneNumber());
                tempDeviceAll.SimOperatorNumeric = string.Concat(mcc, mnc);

                Code = tempDeviceAll.SimOperatorNumeric;
                Phone = tempDeviceAll.SimPhoneNumber;
                Imsi = tempDeviceAll.IMSI;
                Iccid = tempDeviceAll.ICCID;

                tempDeviceAll.SimOperatorCountry = currentSelectedCountry.CountryIso;
                tempDeviceAll.SimOperatorName = currentSelectedCarrier.Name.Substring(0, currentSelectedCarrier.Name.LastIndexOf("-")).Replace("&", "^&");
                tempDeviceAll.WifiMacAddress = RandomService.generateWifiMacAddress();
                tempDeviceAll.BlueToothMacAddress = RandomService.generateMacAddress();

            }
            catch (Exception ex)
            {
                //ignored
                Console.WriteLine(ex);
            }
            finally
            {
                IsRandomButtonSimEnabled = true;
                IsButtonChangeSim = true;
                IsButtonChangeSimFull = true;
            }
        }
        private async Task ChangeDevice()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show("Are you sure to proceed with these changes and reboot ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var messageBoxPushFile = MessageBoxResult.No;
                    var messageBoxPushFileJson = MessageBoxResult.No;
                    if (IsCheckedKeyBox == true)
                    {
                        messageBoxPushFile = System.Windows.MessageBox.Show("Are you push file keybox.xml to phone ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }

                    if (messageBoxPushFile == MessageBoxResult.Yes)
                    {
                        bool validFileSelected = false;
                        while (!validFileSelected)
                        {
                            var openFileDialog = new Microsoft.Win32.OpenFileDialog
                            {
                                Filter = "XML files (*.xml)|*.xml",
                                Title = "Select keybox.xml file"
                            };
                            bool? dialogResult = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => openFileDialog.ShowDialog());
                            if (dialogResult == true)
                            {
                                var fileName = Path.GetFileName(openFileDialog.FileName);
                                if (string.Equals(fileName, "keybox.xml", StringComparison.OrdinalIgnoreCase))
                                {
                                    selectedFilePath = openFileDialog.FileName;
                                    validFileSelected = true;
                                }
                                else
                                {
                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        System.Windows.MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    });
                                }
                            }
                            else
                            {
                                validFileSelected = true;
                            }
                        }
                    }

                    if (IsCheckedpif == true)
                    {
                        messageBoxPushFileJson = System.Windows.MessageBox.Show("Are you push file pif.json to phone ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }

                    if (messageBoxPushFileJson == MessageBoxResult.Yes)
                    {
                        bool validFileSelected = false;
                        while (!validFileSelected)
                        {
                            var openFileDialog = new Microsoft.Win32.OpenFileDialog
                            {
                                Filter = "JSON files (*.json)|*.json",
                                Title = "Select pif.json file"
                            };
                            bool? dialogResult = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => openFileDialog.ShowDialog());
                            if (dialogResult == true)
                            {
                                var fileName = Path.GetFileName(openFileDialog.FileName);
                                if (string.Equals(fileName, "pif.json", StringComparison.OrdinalIgnoreCase))
                                {
                                    selectedFilePathJson = openFileDialog.FileName;
                                    validFileSelected = true;
                                }
                                else
                                {
                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        System.Windows.MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    });
                                }
                            }
                            else
                            {
                                validFileSelected = true;
                            }
                        }
                    }

                    foreach (var device in selectedDevices)
                    {
                        UpdateDeviceStatus(device.DeviceId, "1%", "Change device start");
                        if (device.Status == "Offline")
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                            continue;

                        _processingDeviceIds.Add(device.DeviceId);
                        if (messageBoxPushFile == MessageBoxResult.Yes && selectedFilePath != null)
                        {
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePath} /data/local/tmp/",
                                device.DeviceId
                            );
                        }
                        if (messageBoxPushFileJson == MessageBoxResult.Yes && selectedFilePathJson != null)
                        {
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePathJson} /data/local/tmp/",
                                device.DeviceId
                            );
                        }

                        tasks.Add(ProcessChangeDeviceAsync(device));
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();
            }
        }
        private async Task AutoChangeFull()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show("Are you sure to proceed with these changes and reboot ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var messageBoxPushFile = MessageBoxResult.No;
                    var messageBoxPushFileJson = MessageBoxResult.No;
                    if (IsCheckedKeyBox == true)
                    {
                        messageBoxPushFile = System.Windows.MessageBox.Show("Are you push file keybox.xml to phone ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }

                    if (messageBoxPushFile == MessageBoxResult.Yes)
                    {
                        bool validFileSelected = false;
                        while (!validFileSelected)
                        {
                            var openFileDialog = new Microsoft.Win32.OpenFileDialog
                            {
                                Filter = "XML files (*.xml)|*.xml",
                                Title = "Select keybox.xml file"
                            };
                            bool? dialogResult = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => openFileDialog.ShowDialog());
                            if (dialogResult == true)
                            {
                                var fileName = Path.GetFileName(openFileDialog.FileName);
                                if (string.Equals(fileName, "keybox.xml", StringComparison.OrdinalIgnoreCase))
                                {
                                    selectedFilePath = openFileDialog.FileName;
                                    validFileSelected = true;
                                }
                                else
                                {
                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        System.Windows.MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    });
                                }
                            }
                            else
                            {
                                validFileSelected = true;
                            }
                        }
                    }

                    if (IsCheckedpif == true)
                    {
                        messageBoxPushFileJson = System.Windows.MessageBox.Show("Are you push file pif.json to phone ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }

                    if (messageBoxPushFileJson == MessageBoxResult.Yes)
                    {
                        bool validFileSelected = false;
                        while (!validFileSelected)
                        {
                            var openFileDialog = new Microsoft.Win32.OpenFileDialog
                            {
                                Filter = "JSON files (*.json)|*.json",
                                Title = "Select pif.json file"
                            };
                            bool? dialogResult = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => openFileDialog.ShowDialog());
                            if (dialogResult == true)
                            {
                                var fileName = Path.GetFileName(openFileDialog.FileName);
                                if (string.Equals(fileName, "pif.json", StringComparison.OrdinalIgnoreCase))
                                {
                                    selectedFilePathJson = openFileDialog.FileName;
                                    validFileSelected = true;
                                }
                                else
                                {
                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        System.Windows.MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    });
                                }
                            }
                            else
                            {
                                validFileSelected = true;
                            }
                        }
                    }

                    foreach (var device in selectedDevices)
                    {
                        UpdateDeviceStatus(device.DeviceId, "1%", "Change device start");
                        if (device.Status == "Offline")
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                            continue;

                        _processingDeviceIds.Add(device.DeviceId);
                        if (messageBoxPushFile == MessageBoxResult.Yes && selectedFilePath != null)
                        {
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePath} /data/local/tmp/",
                                device.DeviceId
                            );
                        }
                        if (messageBoxPushFileJson == MessageBoxResult.Yes && selectedFilePathJson != null)
                        {
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePathJson} /data/local/tmp/",
                                device.DeviceId
                            );
                        }

                        tasks.Add(ProcessChangeDeviceAsync(device, 1));
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();
            }
        }
        private async Task ProcessChangeDeviceAsync(Models.DeviceModel device, int checkChange = 0)
        {
            UpdateDeviceStatus(device.DeviceId, "0%", "Change device start");
            POCO.Models.DeviceModel deviceTemp = null;
            if (checkChange == 1)
            {
                deviceTemp = await RandomDevicePrivate();
            }
            var uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var saveResult = true;
            UpdateDeviceStatus(device.DeviceId, "5%", "Change device start");
            await Task.Run(async () =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {

                });

                ADBService.enableWifi(false, device.DeviceId);
                await Task.Delay(2000);
                UpdateDeviceStatus(device.DeviceId, "15%", "Change device ....");
                Console.WriteLine(IsCheckedSim);
                saveResult = Services.Util.SaveDeviceInfo(checkChange == 0 ? tempDeviceAll : deviceTemp, device.DeviceId, AppDomain.CurrentDomain.BaseDirectory, IsCheckedSim);
                UpdateDeviceStatus(device.DeviceId, "75%", "Change device ....");
                if (saveResult)
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {

                    });
                    UpdateDeviceStatus(device.DeviceId, "85%", "Change device ....");
                    var packagesWipeAfterChanger = loadWipeListConfig();
                    wipePackagesChanger(packagesWipeAfterChanger, device.DeviceId);

                    ADBService.cleanGMSPackagesAndAccounts(device.DeviceId);
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {

                    });
                    if (device.DeviceId.Length >= 12)
                    {
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        Thread.Sleep(10000);
                        // FakeDevicePixelAction(device, checkBoxFakeSimInfo.Checked);
                    }
                    UpdateDeviceStatus(device.DeviceId, "100%", "Change device success");
                }
            }).ContinueWith(task =>
            {
                if (!saveResult)
                {
                    UpdateDeviceStatus(device.DeviceId, "0%", "Error!");
                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show("This selected device cannot be changed, please check your rom and developer setting and try loading again."
                                            , "Device Error"
                                            , MessageBoxButton.OK
                                            , MessageBoxImage.Error);
                }
            }, uiThreadScheduler);

        }
        private async Task ChangeSim()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show("Are you sure to proceed with these changes and reboot ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var device in selectedDevices)
                    {
                        UpdateDeviceStatus(device.DeviceId, "1%", "Change sim start");
                        if (device.Status == "Offline")
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                            continue;

                        _processingDeviceIds.Add(device.DeviceId);

                        tasks.Add(ProcessChangeSimAsync(device));
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();
            }
        }
        private async Task AutoChangeSim()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show("Are you sure to proceed with these changes and reboot ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var device in selectedDevices)
                    {
                        UpdateDeviceStatus(device.DeviceId, "1%", "Change sim start");
                        if (device.Status == "Offline")
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                            continue;

                        _processingDeviceIds.Add(device.DeviceId);

                        tasks.Add(ProcessChangeSimAsync(device, 1));
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();
            }
        }
        private async Task ProcessChangeSimAsync(Models.DeviceModel device, int checkChange = 0)
        {
            POCO.Models.DeviceModel deviceTemp = null;
            if (checkChange == 1)
            {
                deviceTemp = await RandomDevicePrivate();
            }
            var uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var saveResult = true;
            UpdateDeviceStatus(device.DeviceId, "5%", "Change sim device start");
            await Task.Run(async () =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {

                });

                ADBService.enableWifi(false, device.DeviceId);
                await Task.Delay(2000);
                UpdateDeviceStatus(device.DeviceId, "15%", "Change sim device ....");
                saveResult = Services.Util.SaveDeviceSIm(checkChange == 0 ? tempDeviceAll : deviceTemp, device.DeviceId, AppDomain.CurrentDomain.BaseDirectory);
                UpdateDeviceStatus(device.DeviceId, "75%", "Change sim device ....");
                if (saveResult)
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {

                    });
                    UpdateDeviceStatus(device.DeviceId, "85%", "Change sim device ....");
                    var packagesWipeAfterChanger = loadWipeListConfig();
                    wipePackagesChanger(packagesWipeAfterChanger, device.DeviceId);

                    ADBService.cleanGMSPackagesAndAccounts(device.DeviceId);
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {

                    });
                    if (device.DeviceId.Length >= 12)
                    {
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        Thread.Sleep(10000);
                        // FakeDevicePixelAction(device, checkBoxFakeSimInfo.Checked);
                    }
                    UpdateDeviceStatus(device.DeviceId, "100%", "Change sim device success");
                }
            }).ContinueWith(task =>
            {
                if (!saveResult)
                {

                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show("This selected device cannot be changed, please check your rom and developer setting and try loading again."
                                            , "Device Error"
                                            , MessageBoxButton.OK
                                            , MessageBoxImage.Error);
                }
            }, uiThreadScheduler);

        }
        private async Task Screenshot()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    UpdateDeviceStatus(device.DeviceId, "1%", "Đang chụp màn hình thiết bị");
                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "100%", "Thiết bị offline");
                        continue;
                    }

                    if (_processingDeviceIds.Contains(device.DeviceId))
                        continue;

                    _processingDeviceIds.Add(device.DeviceId);

                    tasks.Add(ProcessScreenshotAsync(device));

                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();

            }
        }
        private async Task ProcessScreenshotAsync(Models.DeviceModel device)
        {
            try
            {
                UpdateDeviceStatus(device.DeviceId, "50%", "Đang thực hiện ... ");
                ADBService.ScreenShotDevice(device.DeviceId);
                UpdateDeviceStatus(device.DeviceId, "100%", "Đã chụp màn hình xong");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private async Task FakeLocation()
        {
            try
            {
                string x = "0";
                string y = "0";
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var vm = new InputCoordinateDialogViewModel();
                var dialog = new DialogView
                {
                    Title = "Fake location",
                    Height = 170,
                    Width = 250,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = vm
                };

                vm.CloseAction = result =>
                {
                    dialog.DialogResult = result;
                    dialog.Close();
                };

                if (dialog.ShowDialog() == true)
                {
                    x = vm.X;
                    y = vm.Y;
                }

                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    UpdateDeviceStatus(device.DeviceId, "1%", "Đang fake localtion");
                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "100%", "Thiết bị offline");
                        continue;
                    }

                    if (_processingDeviceIds.Contains(device.DeviceId))
                        continue;

                    _processingDeviceIds.Add(device.DeviceId);

                    tasks.Add(ProcessFakeLocationAsync(device, x, y));

                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();

            }
        }
        private async Task OpenUrl()
        {
            try
            {
                string url = "";
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show("No devices selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var vm = new InputViewModel();
                var dialog = new InputView
                {
                    Title = "Nhập Url ",
                    Height = 150,
                    Width = 300,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = vm,
                };

                vm.CloseAction = result =>
                {
                    dialog.DialogResult = result;
                    dialog.Close();
                };

                if (dialog.ShowDialog() == true)
                {
                    url = NormalizeUrl(vm.InputText);
                }


                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    tasks.Add(ProcessOpenUrlAsync(device, url));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();

            }
        }
        private async Task ProcessFakeLocationAsync(Models.DeviceModel device, string x, string y)
        {
            if (x != "" && x != "")
            {
                UpdateDeviceStatus(device.DeviceId, "10%", "Đang fake localtion");
                ADBService.FakeLocation(x, y, device.DeviceId);
                UpdateDeviceStatus(device.DeviceId, "100%", "Fake location success");
            }
            else { UpdateDeviceStatus(device.DeviceId, "0%", "Giá trị latitude và longitude không có !"); }

        }
        private async Task ProcessOpenUrlAsync(Models.DeviceModel device, string url)
        {
            if (url != null || url != "")
            {
                ADBService.OpenUrl(url, device.DeviceId);
            }
            else { UpdateDeviceStatus(device.DeviceId, "0%", ""); }

        }
        private void CheckBoxDevice(Models.DeviceModel device)
        {
            if (device != null)
            {
                if (device.IsChecked)
                {
                    device.IsChecked = true;
                }
                else
                {
                    device.IsChecked = false;
                }
            }
            SaveDevices();
        }
        public void UpdateDeviceStatus(string deviceId, string newPercentage, string newProgress)
        {
            var device = Devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device != null)
            {
                device.Percentage = newPercentage;
                device.Progress = newProgress;
            }
        }
        private string[] loadWipeListConfig()
        {
            var defaultConfigPath = string.Format("{0}/config/wipe-packages.config", AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                LocalFileService.createFileIfNotExist(defaultConfigPath);
                return LocalFileService.readAllLinesTextFile(defaultConfigPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new string[] { };
            }
        }
        private void wipePackagesChanger(string[] packages, string deviceId)
        {
            var packageXmlPathInAndroid = "/data/system/packages.xml";
            var pathXml = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packages.xml");


            ADBService.pullOrPushFile(FileTransferAction.PULL, packageXmlPathInAndroid, AppDomain.CurrentDomain.BaseDirectory, deviceId);

            foreach (string pack in packages)
            {
                ADBService.forceStopPackage(pack, deviceId);
                Thread.Sleep(1000);
                ADBService.wipePackage(pack, deviceId);
                var base64Str = RandomService.generateBase64String();
                XmlService.editPackagesInfo(pathXml, base64Str, pack);
                var source = string.Format("/data/app/$(ls /data/app | grep {0})", pack);
                var destination = string.Format("/data/app/{0}-{1}/", pack, base64Str);
                ADBService.moveFile(source, destination, deviceId);
            }
            ADBService.pullOrPushFile(FileTransferAction.PUSH, pathXml, "/data/system/", deviceId);
            File.Delete(pathXml);
        }
        private void CreateService()
        {
            var poolId = AppConfigService.ReadSetting("poolId");
            var clientId = AppConfigService.ReadSetting("clientId");
            var cognito = new CognitoService("ap-southeast-1_Cha6gy7Ui", "4h21ba0at8flinn9iq351if381");
            var username = AppConfigService.ReadSetting("email");
            var password = AppConfigService.ReadSetting("password");
            var endpoint = "https://nievrqo2rbdtfhmhzc2bg2epka.appsync-api.ap-southeast-1.amazonaws.com/graphql";//AppConfigService.ReadSetting("endpoint");
            var refreshToken = cognito.getIdToken("mistplay@yopmail.com", "12345678");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                miChangerGraphQLClient = new MiChangerGraphQLClient(endpoint, ApiAuthenticationType.TOKEN, refreshToken);
            }
        }
        private string GetDeviceInfoFromADB(string deviceID, string property)
        {
            string result = ADBService.ExecuteADBCommandDetail(deviceID, $"shell {property}");
            return result.Trim();
        }
        private string GetDeviceMACAddress(string deviceID)
        {
            string result = ADBService.ExecuteADBCommandDetail(deviceID, "shell cat /sys/class/net/wlan0/address");
            return result.Trim();
        }
        private bool FakeTimeZone(string proxy, string deviceId)
        {
            try
            {
                ADBService.enableWifi(false, deviceId);
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);
                if (proxyParts.Length == 4)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} --proxy-user {proxyParts[2]}:{proxyParts[3]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject jsonOblect = JObject.Parse(str);
                        ADBService.FakeTimezone(jsonOblect["timezone"].ToString(), deviceId);
                        return true;
                    }
                    else
                    {
                        // MessageBox.Show($"{ViewChangeStatic.logErrorFakeTimeZone} {proxyParts[0]} {ViewChangeStatic.logErrorFakeTimeZone1} {proxyParts[0]}", ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else if (proxyParts.Length == 2)
                {
                    var commandline = $"curl --socks5 {proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                    var str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject jsonOblect = JObject.Parse(str);
                        ADBService.FakeTimezone(jsonOblect["timezone"].ToString(), deviceId);
                        return true;
                    }
                    else
                    {
                        // MessageBox.Show($"{ViewChangeStatic.logErrorFakeTimeZone} {proxyParts[0]} {ViewChangeStatic.logErrorFakeTimeZone1} {proxyParts[0]}", ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    //  MessageBox.Show(ViewChangeStatic.logFakeTimeZone, ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, ViewChangeStatic.TitleErrorFakeTimeZone, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            url = url.Trim();

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            return "https://" + url;
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}