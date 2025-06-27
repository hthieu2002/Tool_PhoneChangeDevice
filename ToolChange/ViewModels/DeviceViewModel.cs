using AuthenticationService;
using Dynamitey.Internal.Optimization;
using Microsoft.Win32;
using MiHttpClient;
using Newtonsoft.Json;
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
using ToolChange.Language;
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
        private string _fakeProxyData;
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
        public string FakeProxyData
        {
            get => _fakeProxyData;
            set
            {
                if (_fakeProxyData != value)
                {
                    _fakeProxyData = value;
                    OnPropertyChanged(nameof(FakeProxyData));
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

                if (value == "Samsung" || value == "Random")
                {
                    var newList = new[] { "Random", "Android 7", "Android 8", "Android 9", "Android 10", "Android 11", "Android 12", "Android 13" };

                    DeviceTypesOs.Clear();
                    foreach (var item in newList)
                    {
                        DeviceTypesOs.Add(item);
                    }

                }
                if (value == "Xiaomi")
                {
                    var newList = new[] { "Random", "Android 8", "Android 9", "Android 10", "Android 11" , "Android 12" };

                    DeviceTypesOs.Clear();
                    foreach (var item in newList)
                    {
                        DeviceTypesOs.Add(item);
                    }

                }
                if (value == "Oppo")
                {
                    var newList = new[] { "Random", "Android 8", "Android 9", "Android 10", "Android 11", "Android 12" };

                    DeviceTypesOs.Clear();
                    foreach (var item in newList)
                    {
                        DeviceTypesOs.Add(item);
                    }

                }
                if (value == "Vivo")
                {
                    var newList = new[] { "Random", "Android 10", "Android 11", "Android 12" };

                    DeviceTypesOs.Clear();
                    foreach (var item in newList)
                    {
                        DeviceTypesOs.Add(item);
                    }

                }
                if (value == "Realme")
                {
                    var newList = new[] { "Random", "Android 10", "Android 11", "Android 12" };

                    DeviceTypesOs.Clear();
                    foreach (var item in newList)
                    {
                        DeviceTypesOs.Add(item);
                    }

                }
                if (value == "Google")
                {
                    var newList = new[] { "Random", "Android 11", "Android 12" };

                    DeviceTypesOs.Clear();
                    foreach (var item in newList)
                    {
                        DeviceTypesOs.Add(item);
                    }

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
                if (value == "7" || value == "6.0.1" || value == "7.1.2")
                {
                    value = "Android 7";
                }
                if (value == "7.1.0")
                {
                    value = "Android 7.1.0";
                }
                if (value == "8" || value == "8.0.0")
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
        public ICommand ViewDevicesCommand { get; private set; }
        public ICommand FakeProxyDeviceIdCommand { get; private set; }
        public ICommand OpenUrlCommand { get; private set; }
        public ICommand FakeProxyAllCommand { get; private set; }

        private readonly string jsonFilePath = Path.Combine("Resources", "Devices", "devices.json");

        public event PropertyChangedEventHandler PropertyChanged;
        public DeviceViewModel()
        {
            ResetDeviceJson();
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
            // DetailsDeviceIdCommand = new RelayCommand<Models.DeviceModel>(DetailsDevices);
            DetailsDeviceIdCommand = new RelayCommand<Models.DeviceModel>(async (device) => await DetailsDevices(device));
            ViewDevicesCommand = new RelayCommand<Models.DeviceModel>(async (device) => await ViewDevicesIC(device));

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
            FakeProxyAllCommand = new RelayCommand(async () => await FakeProxyAll());
            IsCheckBoxDevice = new RelayCommand<Models.DeviceModel>(CheckBoxDevice);
        }
        private void ResetDeviceJson()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Resources\\Devices\\devices.json");
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<Models.DeviceModel>>(json);
            if (list == null) return;

            foreach (var d in list)
            {
                d.Percentage = "0%";
                d.Progress = "...";
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(list, Formatting.Indented));
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
                        : System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<Models.DeviceModel>>(jsonContent) ?? new ObservableCollection<Models.DeviceModel>();
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
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "...");
                                device.Status = newStatus;
                                device.Active = newActive;
                            }
                        }
                        else if (device.Status != "Offline" || device.Active != "NO")
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Device offline");
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
                var json = System.Text.Json.JsonSerializer.Serialize(Devices, new JsonSerializerOptions { WriteIndented = true });
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
        private async Task ViewDevicesIC(Models.DeviceModel device)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.DeviceId))
                return;

            var scrcpyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "scrcpy.exe");

            if (!File.Exists(scrcpyPath))
            {
                System.Windows.MessageBox.Show("Không tìm thấy scrcpy.exe");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = scrcpyPath,
                Arguments = $"-s {device.DeviceId}",
                UseShellExecute = false,
                CreateNoWindow = true
            };


            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi chạy scrcpy: {ex.Message}");
            }
        }

        private async Task DetailsDevices(Models.DeviceModel device)
        {
            if (device == null) return;

            // 1️⃣  Khởi tạo dialog & VM trước
            var vm = new DetailDeviceViewModel
            {
                Title = DevicesLang.TitleDetailDevice
            };

            var dialog = new DetailDevicesView
            {
                Title = $"{DevicesLang.TitleDetailDevice} {device.DeviceId}",
                Height = 500,
                Width = 350,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm
            };

            // 2️⃣  Chạy song song các lệnh ADB trên thread nền
            var brandTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.product.brand"));
            var nameTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.android.board"));
            var modelTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.product.model"));
            var os1Task = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.bootimage.build.version.release"));
            var countryTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_sim_operator_country"));
            var simTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_sim_operator_name"));
            var serialTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.serialno"));
            var codeTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_sim_operator_numeric"));
            var phoneTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_line1_number"));
            var imeiTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_imei_number"));
            var imsiTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_imsi"));
            var iccidTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "settings get global mi_iccid"));
            var macTask = Task.Run(() => GetDeviceMACAddress(device.DeviceId));

            await Task.WhenAll(brandTask, nameTask, modelTask, os1Task, countryTask,
                               simTask, serialTask, codeTask, phoneTask,
                               imeiTask, imsiTask, iccidTask, macTask);

            // 3️⃣  Gán dữ liệu (đang ở UI-thread vì await đã về Dispatcher)
            vm.Brand = brandTask.Result;
            vm.Name = nameTask.Result;
            vm.Model = modelTask.Result;
            vm.Os = os1Task.Result;
            vm.Country = countryTask.Result;
            vm.Sim = simTask.Result;
            vm.Serial = serialTask.Result;
            vm.Code = codeTask.Result;
            vm.Phone = phoneTask.Result;
            vm.Imei = imeiTask.Result;
            vm.Imsi = imsiTask.Result;
            vm.Iccid = iccidTask.Result;
            vm.Mac = macTask.Result;

            // 4️⃣  Hiển thị dialog
            dialog.ShowDialog();
        }

        private void FakeProxyDeviceId(Models.DeviceModel device)
        {
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", DevicesLang.logTitleProxy);
            string proxy = "";
            var vm = new InputViewModel();
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "1%", DevicesLang.logTitleProxy);
            var dialog = new InputView
            {
                Title = DevicesLang.GetTitieProxy(device.Name, device.DeviceId),
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
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", DevicesLang.logTitleProxy);
                    var peelProxy = proxy.Split(':');
                    var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                    Task.Run(() =>
                    {
                        var isFakeTimeZone = FakeTimeZone(proxy, device.DeviceId);
                        if (isFakeTimeZone)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", DevicesLang.logTitleProxy);
                            Thread.Sleep(10000);
                            string ip = peelProxy[0];
                            int port = int.Parse(peelProxy[1]);
                            string user = (peelProxy.Length >= 3) ? peelProxy[2] : "";
                            string password = (peelProxy.Length >= 4) ? peelProxy[3] : "";
                            ADBService.enableWifi(false, device.DeviceId);
                            ADBService.rootAndRemount(device.DeviceId);
                            ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                            RedSocksService.stop(device.DeviceId);
                            if (ADBService.checkFileOnDevice("/data/local/tmp/redsocks.conf", device.DeviceId))
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "40%", Lang.LogError);
                                RedSocksService.stop(device.DeviceId);
                            }

                            RedSocksService.setUpRedSocksOnDevice("/data/local/tmp", device.DeviceId);
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", DevicesLang.logTitleProxy);
                            RedSocksService.start(ip, port, "/data/local/tmp", device.DeviceId, user, password);
                            ADBService.openWifiSettings(device.DeviceId);
                            while (!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId))
                            {
                                ADBService.openWifiSettings(device.DeviceId);
                                Thread.Sleep(3000);
                            }
                            Thread.Sleep(5000);
                            ADBService.OpenBrowserWithUrl("https://browserleaks.com/ip", device.DeviceId);
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", DevicesLang.logCheckProxy);
                        }
                        else
                        {
                            return;
                        }
                    }).ContinueWith(task =>
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", DevicesLang.logTitleProxySuccess);
                    }, currentTask);

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    OsValue = "24";
                }
                else
                {
                    if (Os == "Android 8.1.0")
                    {
                        OsValue = "27";
                    }
                    else if (Os == "Android 7")
                    {
                        OsValue = "25";
                    }
                    else if (Os == "Android 8")
                    {
                        OsValue = "26";
                    }
                    else if (Os == "Android 9")
                    {
                        OsValue = "28";
                    }
                    else if (Os == "Android 10")
                    {
                        OsValue = "29";
                    }
                    else if (Os == "Android 11")
                    {
                        OsValue = "30";
                    }
                    else if (Os == "Android 12")
                    {
                        OsValue = "31";
                    }
                    else if (Os == "Android 13")
                    {
                        OsValue = "32";
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
                    if ((OsValue == "29" || OsValue == "30") || (Os == "Random" || OsValue == "24"))
                    {

                    }
                    else
                    {
                        OsValue = "29";
                    }
                }
                if (BrandValue == "OPPO" || BrandValue == "Xiaomi")
                {
                    if ((OsValue == "29" || OsValue == "30" || OsValue == "28" || OsValue == "27") || (Os == "Random" || OsValue == "24"))
                    {

                    }
                    else
                    {
                        OsValue = "29";
                    }
                }
                await Task.Delay(1000);
                tempDevice = await miChangerGraphQLClient.GetRandomDeviceV3(brand: BrandValue, sdkMin: int.Parse(OsValue), sdkMax: Os == "Random" ? 32 : int.Parse(OsValue));
                if (tempDevice.Model == null)
                {
                    throw new Exception(DevicesLang.logDeviceRandomEx);
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
                    OsValue = "24";
                }
                else
                {
                    if (Os == "Android 8.1.0")
                    {
                        OsValue = "27";
                    }
                    else if (Os == "Android 7")
                    {
                        OsValue = "25";
                    }
                    else if (Os == "Android 8")
                    {
                        OsValue = "26";
                    }
                    else if (Os == "Android 9")
                    {
                        OsValue = "28";
                    }
                    else if (Os == "Android 10")
                    {
                        OsValue = "29";
                    }
                    else if (Os == "Android 11")
                    {
                        OsValue = "30";
                    }
                    else if (Os == "Android 12")
                    {
                        OsValue = "31";
                    }
                    else if (Os == "Android 13")
                    {
                        OsValue = "32";
                    }
                    else
                    {
                        OsValue = "29";
                    }
                }

                if (BrandValue == "Google" && (Os == "Random" || OsValue == "24"))
                {
                    OsValue = "30";
                }
                if ((BrandValue == "realme" || BrandValue == "vivo") && (Os == "Random" || OsValue == "24"))
                {
                    if ((OsValue == "29" || OsValue == "30"))
                    {

                    }
                    else
                    {
                        return;
                    }
                }
                if ((BrandValue == "OPPO" || BrandValue == "Xiaomi") && (Os == "Random" || OsValue == "24"))
                {
                    if ((OsValue == "29" || OsValue == "30" || OsValue == "28" || OsValue == "27"))
                    {

                    }
                    else
                    {
                        return;
                    }
                }

                tempDeviceAll = await miChangerGraphQLClient.GetRandomDeviceV3(brand: BrandValue, sdkMin: int.Parse(OsValue), sdkMax: Os == "Random" ? 32 : int.Parse(OsValue));
                if (tempDeviceAll.Model == null)
                {
                    throw new Exception(DevicesLang.logDeviceRandomEx);
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
                tempDeviceAll.WifiMacAddress = Mac;
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
                    throw new Exception(DevicesLang.logDeviceRandomEx);
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
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show(DevicesLang.logChangeDevice, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var messageBoxPushFile = MessageBoxResult.No;
                    var messageBoxPushFileJson = MessageBoxResult.No;
                    if (IsCheckedKeyBox == true)
                    {
                        messageBoxPushFile = System.Windows.MessageBox.Show(DevicesLang.logChangeDeviceKeyBox, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                                        System.Windows.MessageBox.Show(Lang.LogError, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                        messageBoxPushFileJson = System.Windows.MessageBox.Show(DevicesLang.logChangeDevicePif, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                                        System.Windows.MessageBox.Show(Lang.LogError, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                        if (device.Status == "Offline")
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                        {
                            continue;
                        }

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
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
               // _processingDeviceIds.Clear();
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
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show(DevicesLang.logChangeDevice, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var messageBoxPushFile = MessageBoxResult.No;
                    var messageBoxPushFileJson = MessageBoxResult.No;
                    if (IsCheckedKeyBox == true)
                    {
                        messageBoxPushFile = System.Windows.MessageBox.Show(DevicesLang.logChangeDeviceKeyBox,Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                                        System.Windows.MessageBox.Show(Lang.LogError, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                        messageBoxPushFileJson = System.Windows.MessageBox.Show(DevicesLang.logChangeDevicePif, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                                        System.Windows.MessageBox.Show(Lang.LogError, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                        if (device.Status == "Offline")
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                        { 
                            continue;
                        }
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
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processingDeviceIds.Clear();
            }
        }
        private async Task ProcessChangeDeviceAsync(Models.DeviceModel device, int checkChange = 0)
        {
            //  UpdateDeviceStatus(device.DeviceId, "0%", "Change device start");
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Change device start");
            POCO.Models.DeviceModel deviceTemp = null;
            if (checkChange == 1)
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Random device .. ");
                deviceTemp = await RandomDevicePrivate();
                while (deviceTemp == null)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Random device again .");
                    deviceTemp = await RandomDevicePrivate();
                    await Task.Delay(2000);
                    if (deviceTemp != null)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Random device success");
                        break;
                    }
                }

            }
            var uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var saveResult = true;
            //UpdateDeviceStatus(device.DeviceId, "5%", "Change device start");
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", "Change device start");
            await Task.Run(async () =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {

                });
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "7%", "Enable Wifi");
              
                ADBService.enableWifi(false, device.DeviceId);
                await Task.Delay(2000);
              //  UpdateDeviceStatus(device.DeviceId, "15%", "Change device ....");
                Console.WriteLine(IsCheckedSim);
                await Task.Delay(1000);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "9%", "Start change device ...");
                saveResult = Services.Util.SaveDeviceInfo(this, Devices, checkChange == 0 ? tempDeviceAll : deviceTemp, device.DeviceId, AppDomain.CurrentDomain.BaseDirectory, IsCheckedSim);
          
                //    UpdateDeviceStatus(device.DeviceId, "75%", "Change device ....");
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "75%", "Change device check");
                if (saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "75%", "Change device Success");
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {

                    });
                    //       UpdateDeviceStatus(device.DeviceId, "85%", "Change device ....");
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "85%", "Wipe device");
                    var packagesWipeAfterChanger = loadWipeListConfig();
                    wipePackagesChanger(packagesWipeAfterChanger, device.DeviceId);

                    ADBService.cleanGMSPackagesAndAccounts(device.DeviceId);
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {

                    });
                    if (device.DeviceId.Length >= 12)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "95%", "Reboot!");
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        await Task.Delay(10000);
                    }
                    else
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "95%", "Reboot!");
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        await Task.Delay(10000);
                        // FakeDevicePixelAction(device, checkBoxFakeSimInfo.Checked);
                    }
                    //      UpdateDeviceStatus(device.DeviceId, "100%", "Change device success");
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "OK");
                    await Task.Delay(2000);
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "...");
                }
            }).ContinueWith(task =>
            {
                if (!saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Change device error!");
                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show(DevicesLang.logErrorExChangeDevice
                                            , DevicesLang.logErrorExTitleChangeDevice +" " +device.DeviceId
                                            , MessageBoxButton.OK
                                            , MessageBoxImage.Error);
                }
            }, uiThreadScheduler);
            _processingDeviceIds.Remove(device.DeviceId);
        }
        private async Task ChangeSim()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show(DevicesLang.logChangeDevice, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var device in selectedDevices)
                    {
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
               // _processingDeviceIds.Clear();
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
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var tasks = new List<Task>();

                var result = System.Windows.MessageBox.Show(DevicesLang.logChangeDevice, Lang.LogInfomation, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var device in selectedDevices)
                    {
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
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Start change sim..");
            POCO.Models.DeviceModel deviceTemp = null;
            if (checkChange == 1)
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Random sim all");
                deviceTemp = await RandomDevicePrivate();
                while (deviceTemp == null) {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Random sim again.. ");
                    deviceTemp = await RandomDevicePrivate();
                    await Task.Delay(2000);
                    if (deviceTemp != null)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Random sim success ");
                        break;
                    }
                }
            }
            var uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var saveResult = true;
           
            await Task.Run(async () =>
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", "Enable wifi ");
                ADBService.enableWifi(false, device.DeviceId);
                await Task.Delay(2000);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "9%", "Start change sim device .. ");
                saveResult = Services.Util.SaveDeviceSIm(checkChange == 0 ? tempDeviceAll : deviceTemp, device.DeviceId, AppDomain.CurrentDomain.BaseDirectory);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "75%", "Check change sim devices success or error ");
                if (saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "76%", "Change sim success ");
                   
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "80%", "Wipe");
                    var packagesWipeAfterChanger = loadWipeListConfig();
                    wipePackagesChanger(packagesWipeAfterChanger, device.DeviceId);

                 //   ADBService.cleanGMSPackagesAndAccounts(device.DeviceId);
                  
                    if (device.DeviceId.Length >= 12)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "Reboot!");
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "Reboot!");
                        _processingDeviceIds.Remove(device.DeviceId);
                        ADBService.restartDevice(device.DeviceId);
                        Thread.Sleep(10000);
                        // FakeDevicePixelAction(device, checkBoxFakeSimInfo.Checked);
                    }
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "Succcess !");
                    await Task.Delay(1000);
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "...");
                }
            }).ContinueWith(task =>
            {
                if (!saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Error");
                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show(DevicesLang.logErrorExChangeDevice
                                            , DevicesLang.logErrorExTitleChangeDevice
                                            , MessageBoxButton.OK
                                            , MessageBoxImage.Error);
                }
            }, uiThreadScheduler);
            _processingDeviceIds.Remove(device.DeviceId);
        }
        private async Task Screenshot()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    if (device.Status == "Offline")
                    {
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
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Start Screen shot device ..");
            try
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "20%", "Start Screen shot device ..");
                ADBService.ScreenShotDevice(device.DeviceId);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "Success");
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
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var vm = new InputCoordinateDialogViewModel();
                var dialog = new DialogView
                {
                    Title = DevicesLang.TitleLocation,
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
                    if (device.Status == "Offline")
                    {
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
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var vm = new InputViewModel();
                var dialog = new InputView
                {
                    Title =DevicesLang.TitleUrl,
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
        private async Task FakeProxyAll()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    if (device.Status == "Offline")
                    {
                        continue;
                    }

                    if (_processingDeviceIds.Contains(device.DeviceId))
                        continue;

                    if (string.IsNullOrEmpty(FakeProxyData))
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "error proxy");
                        return;
                    }

                    _processingDeviceIds.Add(device.DeviceId);

                    tasks.Add(ProcessFakeProxyAllAsync(device));

                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error in ChangeDevice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {

            }
        }
        private async Task ProcessFakeProxyAllAsync(Models.DeviceModel device)
        {
            if (!string.IsNullOrEmpty(FakeProxyData))
            {
                // ok
                try
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", DevicesLang.logTitleProxy);
                    var peelProxy = FakeProxyData.Split(':');
                    var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                    await Task.Run(() =>
                    {
                        var isFakeTimeZone = FakeTimeZone(FakeProxyData, device.DeviceId);
                        if (isFakeTimeZone)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", DevicesLang.logTitleProxy);
                            Thread.Sleep(10000);
                            string ip = peelProxy[0];
                            int port = int.Parse(peelProxy[1]);
                            string user = (peelProxy.Length >= 3) ? peelProxy[2] : "";
                            string password = (peelProxy.Length >= 4) ? peelProxy[3] : "";
                            ADBService.enableWifi(false, device.DeviceId);
                            ADBService.rootAndRemount(device.DeviceId);
                            ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                            RedSocksService.stop(device.DeviceId);
                            if (ADBService.checkFileOnDevice("/data/local/tmp/redsocks.conf", device.DeviceId))
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "40%", Lang.LogError);
                                RedSocksService.stop(device.DeviceId);
                            }

                            RedSocksService.setUpRedSocksOnDevice("/data/local/tmp", device.DeviceId);
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", DevicesLang.logTitleProxy);
                            RedSocksService.start(ip, port, "/data/local/tmp", device.DeviceId, user, password);
                            ADBService.openWifiSettings(device.DeviceId);
                            while (!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId))
                            {
                                ADBService.openWifiSettings(device.DeviceId);
                                Thread.Sleep(3000);
                            }
                            Thread.Sleep(5000);
                            ADBService.OpenBrowserWithUrl("https://browserleaks.com/ip", device.DeviceId);
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", DevicesLang.logCheckProxy);
                        }
                        else
                        {
                            return;
                        }
                        _processingDeviceIds.Remove(device.DeviceId);
                    }).ContinueWith(task =>
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", DevicesLang.logTitleProxySuccess);
                    }, currentTask);
                    _processingDeviceIds.Remove(device.DeviceId);
                }
                catch (Exception ex)
                {
                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show(ex.Message, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async Task ProcessFakeLocationAsync(Models.DeviceModel device, string x, string y)
        {
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", $"Start fake location for location {x} - {y}");
            if (x != "" && x != "")
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", $"Start fake location for location {x} - {y}");
                ADBService.FakeLocation(x, y, device.DeviceId);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", $"Success fake location for location {x} - {y}");
            }
            else { UpdateDeviceStatus(device.DeviceId, "0%", "Value latitude and longitude is null !"); }

        }
        private async Task ProcessOpenUrlAsync(Models.DeviceModel device, string url)
        {
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", $"Start open url for {url}");
            if (url != null || url != "")
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "20%", $"Start open url for {url}");
                ADBService.OpenUrl(url, device.DeviceId);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", $"Success open url for {url}");
            }
            else { UpdateDeviceStatus(device.DeviceId, "0%", "Url null"); }

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
            string result = ADBService.ExecuteADBCommandDetail(deviceID, "shell settings get global mi_mac_address");
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