using AuthenticationService;
using DeepDroid.Models;
using MiHttpClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POCO.Models;
using Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ToolChange.Language;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.ViewModels.Constants;
using ToolChange.Views.ControlScriptPage;

namespace ToolChange.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private CognitoService cognitoService;
        private string token;
        private string endpoint = DeepDroid.Properties.Settings.Default.endpoint;
        private string authenticationType = "authorization";

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        public LocalizationViewModel LanguageVM { get; set; }
        public DeviceViewModel DeviceListVM { get; set; }
        private static CancellationTokenSource? _ctsDV;
        private MiChangerGraphQLClient miChangerGraphQLClient;
        private POCO.Models.DeviceModel tempDeviceAll;
        private readonly HashSet<string> _processingDeviceIds = new();
        private string selectedFilePath;
        private string selectedFilePathJson;
        private string refreshToken;
        public ObservableCollection<SimCarrier> Countries { get; set; } = new();
        public ObservableCollection<POCO.Models.ComboBoxItem> SimOptions { get; set; } = new();
        private string _fakeProxyData;

        private static readonly string[] OsFull = { "Android 13", "Android 14", "Android 15" };
        private static readonly string[] OsOppo = {"Android 14" };
        private static readonly string[] OsVivo = { "Android 14" };
        private static readonly string[] OsOnePlus = { "Android 13" };

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
        private bool _fakeSdk = true;
        public bool IsFakeSdk
        {
            get => _fakeSdk;
            set
            {
                _fakeSdk = value;
                OnPropertyChanged(nameof(IsFakeSdk));
            }
        }
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
    "29",
    "30",
    "31",
    "32",
    "33",
    "34",
    "35"
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
        public ObservableCollection<string> DeviceTypes { get; } =
      new ObservableCollection<string>(
          new[]
          {
            "Samsung",
            "Xiaomi",
            "Oppo",
            "Vivo",
            "Google",
            "OnePlus",
          }
          .OrderBy(x => x)               // sắp xếp A-Z
          .Prepend("Random")             // đưa Random lên đầu
      );

        public ObservableCollection<string> DeviceTypesOs { get; } = new ObservableCollection<string>

{
    "Random",
    "Android 13",
    "Android 14",
    "Android 15"
};
        private string _user = DeepDroid.Properties.Settings.Default.user.Split('@')[0];

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
        private string _gpu;
        private string _mac;
        private string _brandValue;
        private string _osValue;
        private string _osValueMax;
        private bool _isSyncing;
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
        public bool BrandRandom = true;
        public bool OsRandom = true;
        public string Brand
        {
            get => _brand;
            set
            {
                if (value == null) return;

                var normalized = ADBService.GetValueBrand(value);
                if (string.Equals(_brand, normalized, StringComparison.OrdinalIgnoreCase))
                    return;

                if (_isSyncing)
                {
                    _brand = normalized;
                    OnPropertyChanged(nameof(Brand));
                    return;
                }

                _isSyncing = true;
                try
                {
                    _brand = normalized;
                    OnPropertyChanged(nameof(Brand));
                    RefreshOsByBrand(_brand, BrandRandom);

                    if ((string.IsNullOrWhiteSpace(_os) && DeviceTypesOs != null && DeviceTypesOs.Any()) || _os == "Random")
                        Os = DeviceTypesOs.First();  

                    Debug.WriteLine(Os);
                }
                finally { _isSyncing = false; }
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
                value = ADBService.GetValueOS(value);

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
        public string Gpu
        {
            get => _gpu;
            set
            {
                _gpu = value;
                OnPropertyChanged(nameof(Gpu));
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
                    DeepDroid.Properties.Settings.Default.user = value;
                    DeepDroid.Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(User));
                }
            }
        }

        private bool _clearData = DeepDroid.Properties.Settings.Default.ClearData;
        public bool IsClearData
        {
            get => _clearData;
            set
            {
                if (_clearData != value)
                {
                    _clearData = value;
                    DeepDroid.Properties.Settings.Default.ClearData = value;
                    DeepDroid.Properties.Settings.Default.Save();

                    OnPropertyChanged(nameof(IsClearData));
                }
            }
        }
        private System.Windows.Input.Cursor _customCursorRandomDevice = System.Windows.Input.Cursors.Hand;
        public System.Windows.Input.Cursor CustomCursorRandomDevice
        {
            get => _customCursorRandomDevice;
            set
            {
                if (_customCursorRandomDevice != value)
                {
                    _customCursorRandomDevice = value;
                    OnPropertyChanged(nameof(CustomCursorRandomDevice));
                }
            }
        }
        private System.Windows.Input.Cursor _customCursorChangeDevice = System.Windows.Input.Cursors.No;
        public System.Windows.Input.Cursor CustomCursorChangeDevice
        {
            get => _customCursorChangeDevice;
            set
            {
                if (_customCursorChangeDevice != value)
                {
                    _customCursorChangeDevice = value;
                    OnPropertyChanged(nameof(CustomCursorChangeDevice));
                }
            }
        }
        private System.Windows.Input.Cursor _deviceDataGridCurson = System.Windows.Input.Cursors.Arrow;
        public System.Windows.Input.Cursor DeviceDataGridCurson
        {
            get => _deviceDataGridCurson;
            set
            {
                if (_deviceDataGridCurson != value)
                {
                    _deviceDataGridCurson = value;
                    OnPropertyChanged(nameof(DeviceDataGridCurson));
                }
            }
        }
        private bool _isAllChecked = true;
        private bool _isUpdatingCheckAll = false;

        public bool IsAllChecked
        {
            get => _isAllChecked;
            set
            {
                if (_isAllChecked != value)
                {
                    _isAllChecked = value;
                    OnPropertyChanged(nameof(IsAllChecked));

                    _isUpdatingCheckAll = true; // bắt đầu chặn trigger
                    foreach (var device in Devices)
                    {
                        device.IsChecked = value;
                    }
                    _ = SaveDevices();
                    _isUpdatingCheckAll = false; // cho phép lại
                }
            }
        }


        public ObservableCollection<Models.DeviceModel> Devices { get; set; }
        public ICommand DeleteDeviceCommand { get; private set; }
        public ICommand CopyDeviceIdCommand { get; private set; }
        public ICommand CopyDeviceIdCommandAll { get; private set; }
        public ICommand RandomDeviceCommand { get; private set; }
        public ICommand RandomSimCommand { get; private set; }
        public ICommand ChangeDeviceCommand { get; private set; }
        public ICommand IsCheckBoxDevice { get; private set; }
        public ICommand AutoChangeFullCommand { get; private set; }
        public ICommand ChangeSimCommand { get; private set; }
        public ICommand AutoChangeSimCommand { get; private set; }
        public ICommand ScreenshotCommand { get; private set; }
        public ICommand PlayIntegrityFix { get; private set; }
        public ICommand FakeLocationCommand { get; private set; }
        public ICommand FakeTimeZoneCommand { get; private set; }
        public ICommand DetailsDeviceIdCommand { get; private set; }
        public ICommand ViewDevicesCommand { get; private set; }
        public ICommand FakeProxyDeviceIdCommand { get; private set; }
        public ICommand FakeProxyDeviceIdHttpCommand { get; private set; }
        public ICommand OpenUrlCommand { get; private set; }
        public ICommand FakeProxyAllCommand { get; private set; }

        private readonly string jsonFilePath = Path.Combine("Resources", "Devices", "devices.json");

        public event PropertyChangedEventHandler PropertyChanged;
        public DeviceViewModel()
        {
            Devices = new ObservableCollection<Models.DeviceModel>();

            _ = ResetDeviceJson();
            _ = LoadDevices();

            _ = CreateService();

            LoadData();
            //   AsyncTask();
            Brand = DeviceTypes.First();
            Os = DeviceTypesOs.First();

            DeleteDeviceCommand = new RelayCommand<object>(DeleteDevice, CanDeleteDevice);
            CopyDeviceIdCommand = new RelayCommand<Models.DeviceModel>(CopyDeviceId);
            CopyDeviceIdCommandAll = new RelayCommand<Models.DeviceModel>(CopyDeviceIdAll);
            // DetailsDeviceIdCommand = new RelayCommand<Models.DeviceModel>(DetailsDevices);
            DetailsDeviceIdCommand = new RelayCommand<Models.DeviceModel>(async (device) => await DetailsDevices(device));
            ViewDevicesCommand = new RelayCommand<Models.DeviceModel>(async (device) => await ViewDevicesIC(device));

            FakeProxyDeviceIdCommand = new RelayCommand<Models.DeviceModel>(FakeProxyDeviceId);
            FakeProxyDeviceIdHttpCommand = new RelayCommand<Models.DeviceModel>(FakeProxyDeviceIdHttp);
            RandomDeviceCommand = new RelayCommand(async () => await RandomDevice());
            RandomSimCommand = new RelayCommand(async () => await RandomSim());
            ChangeDeviceCommand = new RelayCommand(async () => await ChangeDevice());
            AutoChangeFullCommand = new RelayCommand(async () => await AutoChangeFull());
            ChangeSimCommand = new RelayCommand(async () => await ChangeSim());
            AutoChangeSimCommand = new RelayCommand(async () => await AutoChangeSim());
            ScreenshotCommand = new RelayCommand(async () => await Screenshot());
            PlayIntegrityFix = new RelayCommand(async () => await PlayIntegrity());
            FakeLocationCommand = new RelayCommand(async () => await FakeLocation());
            FakeTimeZoneCommand = new RelayCommand(async () => await FakeTimeZone());
            OpenUrlCommand = new RelayCommand(async () => await OpenUrl());
            FakeProxyAllCommand = new RelayCommand(async () => await FakeProxyAll());
            IsCheckBoxDevice = new RelayCommand<Models.DeviceModel>(async (device) => await CheckBoxDevice(device));
            foreach (var device in Devices)
            {
                AttachPropertyChanged(device);
            }
            Devices.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Models.DeviceModel device in e.NewItems)
                    {
                        AttachPropertyChanged(device);
                    }
                }
            };

        }
        private void RebuildBrandList(IList<string> brands)
        {
            var current = _brand;

            DeviceTypes.Clear();
            DeviceTypes.Add("Random");
            foreach (var b in brands)
                DeviceTypes.Add(b);

            // Nếu BrandRandom → chọn "Random"
            if (BrandRandom)
            {
                if (!string.Equals(_brand, "Random", StringComparison.OrdinalIgnoreCase))
                    Brand = "Random";
                return;
            }

            // Nếu brand hiện tại vẫn còn trong list → giữ nguyên
            if (!string.IsNullOrEmpty(current) &&
                DeviceTypes.Any(x => x.Equals(current, StringComparison.OrdinalIgnoreCase)))
            {
                // giữ _brand, KHÔNG set lại để tránh re-entrancy
                return;
            }

            // Nếu không còn, chọn brand hợp lệ đầu tiên (ưu tiên brand thực, không phải "Random")
            var firstRealBrand = DeviceTypes.FirstOrDefault(x => !x.Equals("Random", StringComparison.OrdinalIgnoreCase));
            if (firstRealBrand != null)
                Brand = firstRealBrand;
            else
                Brand = "Random";
        }
        private void RefreshOsByBrand(string value, bool BrandRandom)
        {
            if (BrandOsMap.TryGetValue(value, out var osList))
            {
                SetOsOptions(osList);
            }
            else
            {
                SetOsOptions(OsFull);
            }
        }
        private void SetOsOptions(IEnumerable<string> osList)
        {
            DeviceTypesOs.Clear();
            DeviceTypesOs.Add("Random");
            foreach (var os in osList)
                DeviceTypesOs.Add(os);

            Os = DeviceTypesOs.First(); // luôn là "Random"
        }

        private static readonly Dictionary<string, string[]> BrandOsMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Samsung"] = OsFull,
            ["Xiaomi"] = OsFull,
            ["Google"] = OsFull,
            ["Oppo"] = OsOppo,
            ["Vivo"] = OsVivo,
            ["OnePlus"] = OsOnePlus,
            // fallback cho brand khác nếu cần có thể thêm ở đây
        };

        private void AttachPropertyChanged(Models.DeviceModel device)
        {
            device.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Models.DeviceModel.IsChecked))
                {
                    if (_isUpdatingCheckAll) return; //Đang trong quá trình cập nhật từ IsAllChecked → bỏ qua

                    // Cập nhật lại IsAllChecked theo danh sách
                    _isAllChecked = Devices.All(d => d.IsChecked);
                    OnPropertyChanged(nameof(IsAllChecked));
                }
            };
        }



        public void AsyncTask()
        {
            if (_ctsDV != null) return;
            _ctsDV = new CancellationTokenSource();
            var tk = _ctsDV.Token;


            Task.Run(async () =>
            {
                while (!tk.IsCancellationRequested)
                {
                    // Thử vào vùng độc quyền
                    if (await DeviceSync.Mutex.WaitAsync(0, tk))
                    {
                        try
                        {
                            await AddNewDevicesFromAdb();
                            await UpdateDevicesStatus();
                        }
                        finally
                        {
                            DeviceSync.Mutex.Release();
                        }
                    }

                    await Task.Delay(500, tk);
                }
            }, tk);
        }
        public static void StopLoop()
        {
            _ctsDV?.Cancel();
            _ctsDV = null;
        }
        private async Task ResetDeviceJson()
        {
            try
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
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading devices: {ex.Message}");
                await Task.Delay(100);
            }
        }

        private async Task LoadDevices()
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
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
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
                }

                if (adbDevices.Any(d => !existingDeviceIds.Contains(d.DeviceId)))
                {
                    await SaveDevices();
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
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    {
                        foreach (var device in Devices)
                        {
                            if (adbDeviceDict.TryGetValue(device.DeviceId, out var adbDevice))
                            {
                                string newStatus = adbDevice.Status;
                                string newActive = await ADBService.CheckDeviceActive(device.DeviceId, miChangerGraphQLClient);

                                if (device.Status != newStatus || device.Active != newActive)
                                {
                                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "...");
                                    device.Status = newStatus;
                                    device.Active = newActive;
                                }
                            }
                            else if (device.Status != "Offline" || device.Active != "NO")
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "⚠ Device offline");
                                _processingDeviceIds.Remove(device.DeviceId);
                                device.Status = "Offline";
                                device.Active = "NO";
                            }
                        }
                    });
                }
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
                            Active = status == "Online" ? await ADBService.CheckDeviceActive(deviceId, miChangerGraphQLClient) : "NO"
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
        public async Task SaveDevices()
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
                    _ = SaveDevices();
                    _ = LoadDevices();
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
            DeviceDataGridCurson = System.Windows.Input.Cursors.AppStarting;
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
                Arguments = $"-s {device.DeviceId} --window-title={device.DeviceId}_ICDeviceView",
                UseShellExecute = false,
                CreateNoWindow = true
            };


            try
            {
                var process = Process.Start(startInfo);
                if (process != null)
                {
                    var timeout = DateTime.Now.AddSeconds(5);
                    string expectedTitle = $"{device.DeviceId}_ICDeviceView";
                    IntPtr windowHandle = IntPtr.Zero;

                    while (DateTime.Now < timeout)
                    {
                        await Task.Delay(100);
                        windowHandle = FindWindow(null, expectedTitle);
                        if (windowHandle != IntPtr.Zero)
                            break;
                    }

                    if (windowHandle != IntPtr.Zero)
                    {
                        // Cửa sổ scrcpy đã hiển thị
                        DeviceDataGridCurson = System.Windows.Input.Cursors.Arrow;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi chạy scrcpy: {ex.Message}");
            }
            DeviceDataGridCurson = System.Windows.Input.Cursors.Arrow;
        }

        private async Task DetailsDevices(Models.DeviceModel device)
        {
            if (device == null) return;

            // Khởi tạo dialog & VM trước
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

            // Chạy song song các lệnh ADB trên thread nền
            var brandTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.product.brand"));
            var nameTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.android.board"));
            var modelTask = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.product.model"));
            var os1Task = Task.Run(() => GetDeviceInfoFromADB(device.DeviceId, "getprop ro.android.build.version.release"));
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

            // Gán dữ liệu (đang ở UI-thread vì await đã về Dispatcher)
            vm.Brand = brandTask.Result;
            vm.Name = nameTask.Result;
            vm.Model = modelTask.Result;
            vm.Os = "Android " + os1Task.Result;
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

        private async void FakeProxyDeviceId(Models.DeviceModel device)
        {
            string proxyHost = "";
            string proxyPort = "";
            string proxyUsername = string.Empty;
            string proxyPassword = string.Empty;

            const string typeproxy = "socks5";

            var model = new FakeProxyIDViewModel();
            var log = new FakeProxyID(device.DeviceId, device.Name, "socks5")
            {
                Title = "Fake proxy socks5",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = model,
            };
            model.CloseAction = result =>
            {
                log.DialogResult = result;
                log.Close();
            };
            if (log.ShowDialog() == true)
            {
                proxyHost = model.ProxyHost.Trim();
                proxyPort = model.ProxyPort.Trim();
                proxyUsername = model.ProxyUsername?.Trim();
                proxyPassword = model.ProxyPassword?.Trim();
            }

            if (string.IsNullOrEmpty(proxyHost) || string.IsNullOrEmpty(proxyPort))
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "---%", "ERROR");
                return;
            }

            if (device.Status == "Offline")
            {
                UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                return;
            }
            if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
            {
                UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                return;
            }
            if (_processingDeviceIds.Contains(device.DeviceId))
            {
                UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                return;
            }

            try
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", DevicesLang.logTitleProxy);
                string proxy = $"{proxyHost}:{proxyPort}:{proxyUsername}:{proxyPassword}";

                var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                await Task.Run(() =>
                {
                    var isFakeTimeZone = FakeTimeZone(proxy, device.DeviceId);
                    if (isFakeTimeZone)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", DevicesLang.logTitleProxy);
                        Thread.Sleep(10000);
                        string ip = proxyHost;
                        int port = int.Parse(proxyPort);
                        string user = proxyUsername;
                        string password = proxyPassword;
                        string authen = (!string.IsNullOrEmpty(proxyUsername) && !string.IsNullOrEmpty(proxyPassword)) ? $"{proxyUsername}:{proxyPassword}@" : "";
                        string proxyParams = $"{typeproxy}://{proxyHost}:{proxyPort}";
                        if (!string.IsNullOrEmpty(authen))
                        {
                            proxyParams = $"{typeproxy}://{authen}{proxyHost}:{proxyPort}";
                        }
                        string ipProxyV4 = Tun2socksService.getIpv4SocksProxy(proxy, device.DeviceId);

                        ADBService.enableWifi(false, device.DeviceId);
                        ADBService.rootAndRemount(device.DeviceId);
                        ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                        Tun2socksService.stop(device.DeviceId);
                        Tun2socksService.setUpTun2socksOnDevice("/data/local/tmp", device.DeviceId);
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", DevicesLang.logTitleProxy);
                        Tun2socksService.start("/data/local/tmp", proxyParams, ipProxyV4, device.DeviceId);
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "60%", "In progress connect wifi..");
                        Thread.Sleep(3000);
                        ADBService.enableWifi(true, device.DeviceId);
                        ADBService.openWifiSettings(device.DeviceId);
                        int step = 0;
                        while ((!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId)) || step++ == 40)
                        {
                            ADBService.openWifiSettings(device.DeviceId);
                            Thread.Sleep(3000);
                        }
                        if (step >= 39)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "⚠ Fake proxy success, check error - wifi error");
                            return;
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
        private async void FakeProxyDeviceIdHttp(Models.DeviceModel device)
        {
            string proxyHost = "";
            string proxyPort = "";
            string proxyUsername = string.Empty;
            string proxyPassword = string.Empty;

            const string typeproxy = "http";

            var model = new FakeProxyIDViewModel();
            var log = new FakeProxyID(device.DeviceId, device.Name, "http")
            {
                Title = "Fake proxy http",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = model,
            };
            model.CloseAction = result =>
            {
                log.DialogResult = result;
                log.Close();
            };
            if (log.ShowDialog() == true)
            {
                proxyHost = model.ProxyHost.Trim();
                proxyPort = model.ProxyPort.Trim();
                proxyUsername = model.ProxyUsername?.Trim();
                proxyPassword = model.ProxyPassword?.Trim();
            }

            if (string.IsNullOrEmpty(proxyHost) || string.IsNullOrEmpty(proxyPort))
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "---%", "ERROR");
                return;
            }


            if (device.Status == "Offline")
            {
                UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                return;
            }
            if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
            {
                UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                return;
            }
            if (_processingDeviceIds.Contains(device.DeviceId))
            {
                UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                return;
            }
            try
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", DevicesLang.logTitleProxy);
                string proxy = $"{proxyHost}:{proxyPort}:{proxyUsername}:{proxyPassword}";

                var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                await Task.Run(() =>
                {
                    var isFakeTimeZone = FakeTimeZoneHttp(proxy, device.DeviceId);
                    if (isFakeTimeZone)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", DevicesLang.logTitleProxy);
                        Thread.Sleep(10000);
                        string ip = proxyHost;
                        int port = int.Parse(proxyPort);
                        string user = proxyUsername;
                        string password = proxyPassword;
                        string authen = (!string.IsNullOrEmpty(proxyUsername) && !string.IsNullOrEmpty(proxyPassword)) ? $"{proxyUsername}:{proxyPassword}@" : "";
                        string proxyParams = $"{typeproxy}://{proxyHost}:{proxyPort}";
                        if (!string.IsNullOrEmpty(authen))
                        {
                            proxyParams = $"{typeproxy}://{authen}{proxyHost}:{proxyPort}";
                        }
                        string ipProxyV4 = Tun2socksService.getIpv4HttpProxy(proxy, device.DeviceId);

                        ADBService.enableWifi(false, device.DeviceId);
                        ADBService.rootAndRemount(device.DeviceId);
                        ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                        Tun2socksService.stop(device.DeviceId);
                        Tun2socksService.setUpTun2socksOnDevice("/data/local/tmp", device.DeviceId);
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", DevicesLang.logTitleProxy);
                        Tun2socksService.start("/data/local/tmp", proxyParams, ipProxyV4, device.DeviceId);
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "60%", "In progress connect wifi..");
                        Thread.Sleep(3000);
                        ADBService.enableWifi(true, device.DeviceId);
                        ADBService.openWifiSettings(device.DeviceId);
                        int step = 0;
                        while ((!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId)) || step++ == 40)
                        {
                            ADBService.openWifiSettings(device.DeviceId);
                            Thread.Sleep(3000);
                        }
                        if (step >= 39)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "⚠ Fake proxy success, check error - wifi error");
                            return;
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
                await CreateService();
            }

            if (IsTokenExpired(refreshToken))
            {
                await CreateService();
            }

            var currentSelectedCarrier = SelectedSim;
            var currentSelectedCountry = SelectedCountry;
            var mcc = SelectedCountry?.Attribute?.Mcc;
            var mnc = SelectedSim?.Value;
            string specialChars = "- &";

            Console.WriteLine("Country Code = {0}. MCC = {1} while carrier name = {2} MNC = {3}"
                , currentSelectedCountry.CountryCode
                , mcc
                , currentSelectedCarrier.Name
                , mnc);

            try
            {
                (string brand, string os) value;

                object brandArg = BrandRandom ? (object)true : (object)(Brand ?? "");
                object osArg = OsRandom ? (object)true : (object)(Os ?? "");

                value = ADBService.GetRandomValue(brandArg, osArg);

                tempDevice = await miChangerGraphQLClient.GetRandomDeviceV4(
                        brand: value.brand,
                        sdkMin: int.Parse(value.os),
                        sdkMax: int.Parse(value.os));
                if (tempDevice.Model == null)
                {
                    return null;
                }
                tempDevice.SDK = value.os;

                tempDevice.IMSI = RandomService.generateIMSI(mcc, mnc);
                tempDevice.ICCID = RandomService.generateICCID(currentSelectedCountry.CountryCode, mnc);
                tempDevice.SerialNo = RandomService.getRandomStringHex16Digit().Substring(0, RandomService.randomInRange(8, 13));
                tempDevice.SimPhoneNumber = string.Format("+{0}{1}", currentSelectedCountry.CountryCode, RandomService.generatePhoneNumber());
                tempDevice.SimOperatorNumeric = string.Concat(mcc, mnc);
                tempDevice.SimOperatorCountry = currentSelectedCountry.CountryIso;
                tempDevice.SimOperatorName = currentSelectedCarrier.Name.LastIndexOf('-') >= 0 
                                                                ? currentSelectedCarrier.Name.Substring(0, currentSelectedCarrier.Name.LastIndexOf('-')) 
                                                                : currentSelectedCarrier.Name;
                tempDevice.AndroidId = RandomService.getRandomStringHex16Digit();
                tempDevice.WifiMacAddress = RandomService.generateWifiMacAddress(tempDevice.Manufacturer.ToLower());
                tempDevice.BlueToothMacAddress = RandomService.generateWifiMacAddress(tempDevice.Manufacturer.ToLower());
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
            CustomCursorRandomDevice = System.Windows.Input.Cursors.Wait;
            if (miChangerGraphQLClient == null)
            {
                await CreateService();
            }
            if (IsTokenExpired(refreshToken))
            {
                await CreateService();
            }
            var currentSelectedCarrier = SelectedSim;
            var currentSelectedCountry = SelectedCountry;
            var mcc = SelectedCountry?.Attribute?.Mcc;
            var mnc = SelectedSim?.Value;

            try
            {
                (string brand, string os) value;

                object brandArg = BrandRandom ? (object)true : (object)(Brand ?? "");
                object osArg = OsRandom ? (object)true : (object)(Os ?? "");

                value = ADBService.GetRandomValue(brandArg, osArg);

                tempDeviceAll = await miChangerGraphQLClient.GetRandomDeviceV4(
                        brand: value.brand,
                        sdkMin: int.Parse(value.os),
                        sdkMax: int.Parse(value.os));
                //    tempDeviceAll = await miChangerGraphQLClient.GetRandomDeviceV4();
                if (tempDeviceAll.Model == null) throw new Exception("Không tìm thấy thiết bị phù hợp.");

                tempDeviceAll.SDK = value.os;

                Brand = tempDeviceAll.Manufacturer;
                Name = tempDeviceAll.Board;
                Model = tempDeviceAll.Model;
                Os = tempDeviceAll.Release;
                Imei = tempDeviceAll.Imei;
                Imsi = tempDeviceAll.IMSI = RandomService.generateIMSI(mcc, mnc);
                Iccid = tempDeviceAll.ICCID = RandomService.generateICCID(currentSelectedCountry.CountryCode, mnc);
                Serial = tempDeviceAll.SerialNo = RandomService.getRandomStringHex16Digit().Substring(0, RandomService.randomInRange(8, 13));
                Phone = tempDeviceAll.SimPhoneNumber = string.Format("+{0}{1}", currentSelectedCountry.CountryCode, RandomService.generatePhoneNumber());
                Code = tempDeviceAll.SimOperatorNumeric = string.Concat(mcc, mnc);
                tempDeviceAll.SimOperatorCountry = currentSelectedCountry.CountryIso;
                tempDeviceAll.SimOperatorName = currentSelectedCarrier.Name.LastIndexOf('-') >= 0
                                                                ? currentSelectedCarrier.Name.Substring(0, currentSelectedCarrier.Name.LastIndexOf('-'))
                                                                : currentSelectedCarrier.Name;
                tempDeviceAll.AndroidId = RandomService.getRandomStringHex16Digit();
                Mac = tempDeviceAll.WifiMacAddress = RandomService. generateWifiMacAddress(tempDeviceAll.Manufacturer.ToLower());
                tempDeviceAll.BlueToothMacAddress = RandomService.generateWifiMacAddress(tempDeviceAll.Manufacturer.ToLower());

                Gpu = tempDeviceAll.Gpu;
                IsButtonChangeDevice = true;
                IsButtonChangeFull = true;
                CustomCursorChangeDevice = System.Windows.Input.Cursors.Hand;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi random device:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex);
            }
            finally
            {
                IsRandomButtonEnabled = true;
                CustomCursorRandomDevice = System.Windows.Input.Cursors.Hand;
            }
        }

        private async Task RandomSim()
        {
            IsRandomButtonSimEnabled = false;
            if (miChangerGraphQLClient == null)
            {
                await CreateService();
            }
            if (IsTokenExpired(refreshToken))
            {
                await CreateService();
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
                //tempDeviceAll = await miChangerGraphQLClient.GetRandomDeviceV3(sdkMin: 30);
                //if (tempDeviceAll.Model == null)
                //{
                //    throw new Exception(DevicesLang.logDeviceRandomEx);
                //}
                if (tempDeviceAll == null)
                {
                    tempDeviceAll = new POCO.Models.DeviceModel
                    {

                    };
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
                tempDeviceAll.SimOperatorName = currentSelectedCarrier.Name.LastIndexOf('-') >= 0
                                                                ? currentSelectedCarrier.Name.Substring(0, currentSelectedCarrier.Name.LastIndexOf('-'))
                                                                : currentSelectedCarrier.Name;
                // tempDeviceAll.WifiMacAddress = RandomService.generateWifiMacAddress();
                // tempDeviceAll.BlueToothMacAddress = RandomService.generateMacAddress();

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

                    if (IsCheckedKeyBox == true)
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
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                            continue;

                        }
                        if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                            continue;

                        }
                        if (_processingDeviceIds.Contains(device.DeviceId))
                        {
                            UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                            continue;

                        }


                        _processingDeviceIds.Add(device.DeviceId);
                        if (false)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "In progress push file keybox.xml to phone");
                            await Task.Delay(1000);
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePath} /data/local/tmp/",
                                device.DeviceId
                            );

                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Push file keybox.xml to phone success");
                            await Task.Delay(1000);
                        }
                        if (false)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "In progress push file pif.json to phone");
                            await Task.Delay(1000);
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePathJson} /data/local/tmp/",
                                device.DeviceId
                            );

                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Push file pif.json to phone success");
                            await Task.Delay(1000);


                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "In progress setprop pihooks to phone");
                            string jsonContent = File.ReadAllText(selectedFilePathJson);
                            await Task.Delay(1000);
                            PifData pifData = JsonConvert.DeserializeObject<PifData>(jsonContent);

                            if (pifData == null)
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "File PIF data null");
                            } 
                            else
                            {
                                var props = typeof(PifData).GetProperties()
                                                            .Where(p => p.PropertyType == typeof(string))
                                                            .Select(p => new { Name = p.Name, Value = p.GetValue(pifData) as string })
                                                            .Where(p => string.IsNullOrEmpty(p.Value) && p.Name != "RELEASE")
                                                            .ToList();

                                if (props.Any())
                                {
                                    string missing = string.Join(", ", props.Select(p => p.Name));
                                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", $"Missing or empty fields: {missing}");
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(pifData.FINGERPRINT))
                                    {
                                        string[] parts = pifData.FINGERPRINT.Split("/");
                                        List<string> splitFingerprint = new List<string>();
                                        foreach (string part in parts)
                                        {
                                            string[] subParts = part.Split(':');
                                            splitFingerprint.AddRange(subParts);
                                        }

                                        if (splitFingerprint.Count == 8)
                                        {
                                            var changePifInfo = new Dictionary<string, string>();
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_BRAND", splitFingerprint[0]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_PRODUCT", splitFingerprint[1]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_DEVICE", splitFingerprint[2]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_BOARD", splitFingerprint[2]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_HARDWARE", splitFingerprint[2]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_ID",splitFingerprint[4]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_INCREMENTAL", splitFingerprint[5]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_FINGERPRINT", pifData.FINGERPRINT);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_MANUFACTURER", pifData.MANUFACTURER);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_MODEL", $"\"{pifData.MODEL}\"");
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_SECURITY_PATCH", pifData.SECURITY_PATCH);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_DEVICE_INITIAL_SDK_INT", pifData.DEVICE_INITIAL_SDK_INT);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_SDK_INT", pifData.SDK_INT);
                                            if(!string.IsNullOrEmpty(pifData.RELEASE))
                                                changePifInfo.Add("persist.sys.deepdroid.pihooks_RELEASE", pifData.RELEASE);
                                            else
                                            {
                                                changePifInfo.Add("persist.sys.deepdroid.pihooks_RELEASE", splitFingerprint[3]);
                                            }

                                            ADBService.replaceBuildProp("/product/etc/build.prop", changePifInfo, device.DeviceId);

                                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Setprop pihooks to phone success");
                                            await Task.Delay(1000);
                                        }
                                        else
                                        {
                                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Invalid FINGERPRINT format");
                                        }
                                    }
                                    else
                                    {
                                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "FINGERPRINT is null or empty");
                                    }
                                }
                            }
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
        private void CopyDeviceIdAll(Models.DeviceModel devices)
        {
            var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
            int selectedCount = selectedDevices.Count;

            if (selectedCount == 0)
            {
                System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var deviceIds = selectedDevices
                .Where(d => !string.IsNullOrEmpty(d.DeviceId))
                .Select(d => d.DeviceId);

            var joinedIds = string.Join("\n", deviceIds);

            if (!string.IsNullOrWhiteSpace(joinedIds))
            {
                System.Windows.Clipboard.SetText(joinedIds);
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

                    if (IsCheckedKeyBox == true)
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
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                            continue;

                        }
                        if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                            continue;

                        }
                        if (_processingDeviceIds.Contains(device.DeviceId))
                        {
                            UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                            continue;

                        }

                        _processingDeviceIds.Add(device.DeviceId);
                        if (false)
                        {
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePath} /data/local/tmp/",
                                device.DeviceId
                            );
                        }
                        if (false)
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "In progress push file pif.json to phone");
                            await Task.Delay(1000);
                            ADBService.ExecuteAdbCommand(
                                $"push {selectedFilePathJson} /data/local/tmp/",
                                device.DeviceId
                            );

                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Push file pif.json to phone success");
                            await Task.Delay(1000);


                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "In progress setprop pihooks to phone");
                            string jsonContent = File.ReadAllText(selectedFilePathJson);
                            await Task.Delay(1000);
                            PifData pifData = JsonConvert.DeserializeObject<PifData>(jsonContent);

                            if (pifData == null)
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "File PIF data null");
                            }
                            else
                            {
                                var props = typeof(PifData).GetProperties()
                                                            .Where(p => p.PropertyType == typeof(string))
                                                            .Select(p => new { Name = p.Name, Value = p.GetValue(pifData) as string })
                                                            .Where(p => string.IsNullOrEmpty(p.Value) && p.Name != "RELEASE")
                                                            .ToList();

                                if (props.Any())
                                {
                                    string missing = string.Join(", ", props.Select(p => p.Name));
                                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", $"Missing or empty fields: {missing}");
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(pifData.FINGERPRINT))
                                    {
                                        string[] parts = pifData.FINGERPRINT.Split("/");
                                        List<string> splitFingerprint = new List<string>();
                                        foreach (string part in parts)
                                        {
                                            string[] subParts = part.Split(':');
                                            splitFingerprint.AddRange(subParts);
                                        }

                                        if (splitFingerprint.Count == 8)
                                        {
                                            var changePifInfo = new Dictionary<string, string>();
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_BRAND", splitFingerprint[0]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_PRODUCT", splitFingerprint[1]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_DEVICE", splitFingerprint[2]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_BOARD", splitFingerprint[2]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_HARDWARE", splitFingerprint[2]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_ID", splitFingerprint[4]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_INCREMENTAL", splitFingerprint[5]);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_FINGERPRINT", pifData.FINGERPRINT);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_MANUFACTURER", pifData.MANUFACTURER);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_MODEL", $"\"{pifData.MODEL}\"");
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_SECURITY_PATCH", pifData.SECURITY_PATCH);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_DEVICE_INITIAL_SDK_INT", pifData.DEVICE_INITIAL_SDK_INT);
                                            changePifInfo.Add("persist.sys.deepdroid.pihooks_SDK_INT", pifData.SDK_INT);
                                            if (!string.IsNullOrEmpty(pifData.RELEASE))
                                                changePifInfo.Add("persist.sys.deepdroid.pihooks_RELEASE", pifData.RELEASE);
                                            else
                                            {
                                                changePifInfo.Add("persist.sys.deepdroid.pihooks_RELEASE", splitFingerprint[3]);
                                            }

                                            ADBService.replaceBuildProp("/product/etc/build.prop", changePifInfo, device.DeviceId);

                                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Setprop pihooks to phone success");
                                            await Task.Delay(1000);
                                        }
                                        else
                                        {
                                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Invalid FINGERPRINT format");
                                        }
                                    }
                                    else
                                    {
                                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "FINGERPRINT is null or empty");
                                    }
                                }
                            }
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
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Change device start");
            ADBService.cleanFolder(device.DeviceId);

            if (DeepDroid.Properties.Settings.Default.ClearData)
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Wipe data ON");
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Delete app");
                await Task.Run(() =>
                {
                    ADBService.UninstallAllUserApps(device.DeviceId); // Chạy trên thread phụ

                    Thread.Sleep(1000);
                });
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "Delete account");
                ADBService.RemoveAccountsDb(device.DeviceId);
            }
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
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "7%", "Enable Wifi");

                ADBService.enableWifi(false, device.DeviceId);
                await Task.Delay(2000);
                //  UpdateDeviceStatus(device.DeviceId, "15%", "Change device ....");
                Console.WriteLine(IsCheckedSim);
                await Task.Delay(1000);
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "9%", "Start change device ...");
             
                saveResult = Services.Util.SaveDeviceInfo(this, Devices, checkChange == 0 ? tempDeviceAll : deviceTemp, device.DeviceId, AppDomain.CurrentDomain.BaseDirectory, IsCheckedSim, IsCheckedpif, IsFakeSdk);

                //    UpdateDeviceStatus(device.DeviceId, "75%", "Change device ....");
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "75%", "Change device check");
                if (saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "75%", "Change device Success");


                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "85%", "Wipe device");
                    var packagesWipeAfterChanger = loadWipeListConfig();
                    wipePackagesChanger(packagesWipeAfterChanger, device.DeviceId);
                    ADBService.cleanGMSPackagesAndAccounts(device.DeviceId);
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "90%", "Wipe network");
                    ADBService.cleanNetworkInternet(device.DeviceId);
                //    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "93%", "Wipe");
              //      ADBService.cleanFolder(device.DeviceId);
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "95%", "Reboot!");
                    _processingDeviceIds.Remove(device.DeviceId);
                    ADBService.restartDevice(device.DeviceId);
                    await Task.Delay(10000);

                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "OK");
                    await Task.Delay(2000);
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "...");
                }
            }).ContinueWith(task =>
            {
                if (!saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "⚠ Change device error!");
                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show(DevicesLang.logErrorExChangeDevice
                                            , DevicesLang.logErrorExTitleChangeDevice + " " + device.DeviceId
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
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                            continue;

                        }
                        if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                            continue;

                        }
                        if (_processingDeviceIds.Contains(device.DeviceId))
                        {
                            UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                            continue;

                        }

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
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                            continue;

                        }
                        if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                        {
                            UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                            continue;

                        }
                        if (_processingDeviceIds.Contains(device.DeviceId))
                        {
                            UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                            continue;

                        }

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
                while (deviceTemp == null)
                {
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

                    //DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "80%", "Wipe");
                    //var packagesWipeAfterChanger = loadWipeListConfig();
                    //wipePackagesChanger(packagesWipeAfterChanger, device.DeviceId);

                    //  ADBService.cleanGMSPackagesAndAccounts(device.DeviceId);

                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "Reboot!");
                    _processingDeviceIds.Remove(device.DeviceId);
                    ADBService.restartDevice(device.DeviceId);
                    Thread.Sleep(10000);

                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "Succcess !");
                    await Task.Delay(1000);
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "...");
                }
            }).ContinueWith(task =>
            {
                if (!saveResult)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "⚠ Error");
                    _processingDeviceIds.Remove(device.DeviceId);
                    System.Windows.MessageBox.Show(DevicesLang.logErrorExChangeDevice
                                            , DevicesLang.logErrorExTitleChangeDevice
                                            , MessageBoxButton.OK
                                            , MessageBoxImage.Error);
                }
            }, uiThreadScheduler);
            _processingDeviceIds.Remove(device.DeviceId);
        }
        private async Task PlayIntegrity()
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
                //

                bool validFileSelectedPif = false;
                while (!validFileSelectedPif)
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
                            validFileSelectedPif = true;
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
                        validFileSelectedPif = true;
                    }
                }

                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                        continue;

                    }
                    if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                        continue;

                    }
                    if (_processingDeviceIds.Contains(device.DeviceId))
                    {
                        UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                        continue;

                    }

                    _processingDeviceIds.Add(device.DeviceId);

                    tasks.Add(ProcessPlayIntegrityAsync(device));

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
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                        continue;

                    }
                    if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                        continue;

                    }
                    if (_processingDeviceIds.Contains(device.DeviceId))
                    {
                        UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                        continue;

                    }

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
        private async Task ProcessPlayIntegrityAsync(Models.DeviceModel device)
        {
            //ADBService.runCMDRoot($"shell am force-stop com.android.vending", device.DeviceId);
            //await Task.Delay(1000);
            //ADBService.runCMDRoot($"shell am force-stop com.google.android.gms", device.DeviceId);
            //await Task.Delay(1000);
            //ADBService.runCMDRoot($"shell am force-stop com.google.android.gsf", device.DeviceId);

            

            //
            if (selectedFilePath != null)
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", "In progress push file keybox.xml to phone");
                await Task.Delay(1000);
                ADBService.ExecuteAdbCommand(
                    $"push {selectedFilePath} /data/local/tmp/",
                    device.DeviceId
                );

                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", "Push file keybox.xml to phone success");
                await Task.Delay(1000);
            }
            if (selectedFilePathJson != null)
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "20%", "In progress push file pif.json to phone");
                await Task.Delay(1000);
                ADBService.ExecuteAdbCommand(
                    $"push {selectedFilePathJson} /data/local/tmp/",
                    device.DeviceId
                );

                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "30%", "Push file pif.json to phone success");
                await Task.Delay(1000);


                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "40%", "In progress setprop pihooks to phone");
                string jsonContent = File.ReadAllText(selectedFilePathJson);
                await Task.Delay(1000);
                PifData pifData = JsonConvert.DeserializeObject<PifData>(jsonContent);

                if (pifData == null)
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "40%", "File PIF data null");
                }
                else
                {
                    var props = typeof(PifData).GetProperties()
                                                .Where(p => p.PropertyType == typeof(string))
                                                .Select(p => new { Name = p.Name, Value = p.GetValue(pifData) as string })
                                                .Where(p => string.IsNullOrEmpty(p.Value) && p.Name != "RELEASE")
                                                .ToList();

                    if (props.Any())
                    {
                        string missing = string.Join(", ", props.Select(p => p.Name));
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", $"Missing or empty fields: {missing}");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(pifData.FINGERPRINT))
                        {
                            string[] parts = pifData.FINGERPRINT.Split("/");
                            List<string> splitFingerprint = new List<string>();
                            foreach (string part in parts)
                            {
                                string[] subParts = part.Split(':');
                                splitFingerprint.AddRange(subParts);
                            }

                            if (splitFingerprint.Count == 8)
                            {
                                var changePifInfo = new Dictionary<string, string>();
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_BRAND", splitFingerprint[0]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_PRODUCT", splitFingerprint[1]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_DEVICE", splitFingerprint[2]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_BOARD", splitFingerprint[2]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_HARDWARE", splitFingerprint[2]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_ID", splitFingerprint[4]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_INCREMENTAL", splitFingerprint[5]);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_FINGERPRINT", pifData.FINGERPRINT);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_MANUFACTURER", pifData.MANUFACTURER);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_MODEL", $"\"{pifData.MODEL}\"");
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_SECURITY_PATCH", pifData.SECURITY_PATCH);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_DEVICE_INITIAL_SDK_INT", pifData.DEVICE_INITIAL_SDK_INT);
                                changePifInfo.Add("persist.sys.deepdroid.pihooks_SDK_INT", pifData.SDK_INT);
                                if (!string.IsNullOrEmpty(pifData.RELEASE))
                                    changePifInfo.Add("persist.sys.deepdroid.pihooks_RELEASE", pifData.RELEASE);
                                else
                                {
                                    changePifInfo.Add("persist.sys.deepdroid.pihooks_RELEASE", splitFingerprint[3]);
                                }

                                ADBService.replaceBuildProp("/product/etc/build.prop", changePifInfo, device.DeviceId);

                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "90%", "Setprop pihooks to phone success");
                                await Task.Delay(1000);
                            }
                            else
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "Invalid FINGERPRINT format");
                            }
                        }
                        else
                        {
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "FINGERPRINT is null or empty");
                        }
                    }
                }
            }
            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", "success");
            //ADBService.runCMDRoot($"shell am force-stop com.android.vending", device.DeviceId);
            //await Task.Delay(1000);
            //ADBService.runCMDRoot($"shell am force-stop com.google.android.gms", device.DeviceId);
            //await Task.Delay(1000);
            //ADBService.runCMDRoot($"shell am force-stop com.google.android.gsf", device.DeviceId);
            ADBService.runCMDRoot($"reboot", device.DeviceId);
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
                else
                {
                    return;
                }
                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                        continue;

                    }
                    if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                        continue;

                    }
                    if (_processingDeviceIds.Contains(device.DeviceId))
                    {
                        UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                        continue;

                    }

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
        private async Task FakeTimeZone()
        {
            try
            {
                string time = "";
                string timezone = "";
                bool deviceCheck = false;
                bool deviceautoCheck = true;
                ObservableCollection<Models.DeviceModel> devices = new ObservableCollection<Models.DeviceModel>();
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var vm = new FakeTimeZoneViewModel(selectedDevices);
                var dialog = new Views.ControlScriptPage.FakeTimeZone(selectedDevices)
                {
                    Title = "Fake time zone GTM",
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
                    time = vm.SelectedGmtOffset;
                    timezone = vm.SelectedTimeZone;
                    deviceCheck = vm.DeviceALL;
                    devices = vm.SelectedDevices;
                    deviceautoCheck = vm.DeviceAutoALL;
                }
                else
                {
                    return;
                }

                var tasks = new List<Task>();

                foreach (var device in (deviceCheck ? selectedDevices.Cast<Models.DeviceModel>() : devices))
                {
                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                        continue;

                    }
                    if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                        continue;

                    }
                    if (_processingDeviceIds.Contains(device.DeviceId))
                    {
                        UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                        continue;

                    }

                    if ((string.IsNullOrEmpty(timezone) || string.IsNullOrEmpty(timezone)) && !deviceautoCheck)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "error time zone");
                        return;
                    }
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", $"Start fake time zone {time}");
                    _processingDeviceIds.Add(device.DeviceId);

                    tasks.Add(ProcessFakeTimeZoneAllAsync(device, timezone, time, deviceautoCheck));
                    _processingDeviceIds.Remove(device.DeviceId);
                }
                await Task.WhenAll(tasks);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        private async Task ProcessFakeTimeZoneAllAsync(Models.DeviceModel device, string timezone, string time, bool autoCheck)
        {
            if (!string.IsNullOrEmpty(timezone) || autoCheck)
            {
                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "30%", $"Start fake time zone {time}");

                // Chạy nền để không bị treo
                await Task.Run(async () =>
                {
                    try
                    {
                        if (autoCheck)
                        {
                            time = FakeTimezoneByNetwork(device.DeviceId);
                        }
                        else
                        {
                            ADBService.FakeTimezone(timezone, device.DeviceId);
                        }
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", $"Fake time zone success {time}");
                    }
                    catch (Exception ex)
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", $"Failed to fake timezone: {ex.Message}");
                    }
                    finally
                    {
                        _processingDeviceIds.Remove(device.DeviceId);
                    }
                });
            }

        }
        public string FakeTimezoneByNetwork(string deviceId)
        {
            try
            {
                DeviceUpdater.UpdateProgress(Devices, deviceId, "35%", "Bắt đầu fake timezone...");

                string timezone = GetTimezoneFromDevice(deviceId);

                if (!string.IsNullOrEmpty(timezone))
                {
                    DeviceUpdater.UpdateProgress(Devices, deviceId, "40%", $"Lấy được timezone từ device: {timezone}");

                    ADBService.rootAndRemount(deviceId);
                    ADBService.runCMDRoot($"shell settings put global auto_time_zone 0", deviceId);
                    ADBService.runCMDRoot($"shell setprop persist.sys.timezone \"{timezone}\"", deviceId);
                    DeviceUpdater.UpdateProgress(Devices, deviceId, "50%", $"Set timezone = {timezone}");

                    ADBService.runCMDRoot($"shell am broadcast -a android.intent.action.TIMEZONE_CHANGED", deviceId);
                    ADBService.runCMDRoot($"shell settings put system time_12_24 24", deviceId);
                    ADBService.runCMDRoot($"shell am broadcast -a android.intent.action.TIME_SET", deviceId);

                    DeviceUpdater.UpdateProgress(Devices, deviceId, "80%", $"✅ Fake timezone success → {timezone}");
                }
                else
                {
                    DeviceUpdater.UpdateProgress(Devices, deviceId, "80%", "⚠ Không lấy được timezone từ device.");
                }

                return timezone;
            }
            catch (Exception ex)
            {
                DeviceUpdater.UpdateProgress(Devices, deviceId, "80%", $"⚠ Lỗi FakeTimezoneByNetwork: {ex.Message}");
                return "Error";
            }
        }

        public static string GetTimezoneFromDevice(string deviceId)
        {
            try
            {
                // Gọi API trực tiếp từ thiết bị Android (qua proxy/device IP)
                string cmd = "shell curl -s http://ip-api.com/json/?fields=timezone";
                string result = ADBService.runCMDRoot(cmd, deviceId);

                if (!string.IsNullOrEmpty(result))
                {
                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    return json.timezone;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetTimezoneFromDevice: " + ex.Message);
            }
            return null;
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
                var dialog = new Views.ControlScriptPage.InputView
                {
                    Title = DevicesLang.TitleUrl,
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
                else
                {
                    return;
                }


                var tasks = new List<Task>();
                foreach (var device in selectedDevices)
                {
                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                        continue;

                    }
                    if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                        continue;

                    }
                    if (_processingDeviceIds.Contains(device.DeviceId))
                    {
                        UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                        continue;

                    }

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
                string proxyHost = "";
                string proxyPort = "";
                string proxyUsername = "";
                string proxyPassword = "";
                bool deviceCheck = false;
                string typeProxy = "";
                ObservableCollection<Models.DeviceModel> devices = new ObservableCollection<Models.DeviceModel>();

                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                const string typeproxy = "http-connect";

                var model = new FakeProxyViewModel(selectedDevices);
                var log = new FakeProxy(selectedDevices)
                {
                    Title = "Fake proxy",
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = model,
                };
                model.CloseAction = result =>
                {
                    log.DialogResult = result;
                    log.Close();
                };
                if (log.ShowDialog() == true)
                {
                    proxyHost = model.ProxyHost.Trim();
                    proxyPort = model.ProxyPort.Trim();
                    proxyUsername = model.ProxyUsername?.Trim();
                    proxyPassword = model.ProxyPassword?.Trim();
                    deviceCheck = model.DeviceALL;
                    typeProxy = model.TypeProxy.Trim();
                    devices = model.SelectedDevices;
                }

                var tasks = new List<Task>();

                foreach (var device in (deviceCheck ? selectedDevices.Cast<Models.DeviceModel>() : devices))
                {

                    if (string.IsNullOrEmpty(typeProxy))
                    {
                        System.Windows.MessageBox.Show("Type proxy null", "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (device.Status == "Offline")
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device offline");
                        continue;

                    }
                    if (!await ADBService.CheckDeviceActiveBool(device.DeviceId, miChangerGraphQLClient))
                    {
                        UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Device not active");
                        continue;

                    }
                    if (_processingDeviceIds.Contains(device.DeviceId))
                    {
                        UpdateDeviceStatus(device.DeviceId, "%", "⏳ Device running...");
                        continue;

                    }

                    if (string.IsNullOrEmpty(proxyHost) || string.IsNullOrEmpty(proxyPort))
                    {
                        DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "⚠ error proxy");
                        return;
                    }

                    _processingDeviceIds.Add(device.DeviceId);

                    tasks.Add(ProcessFakeProxyAllAsync(device, proxyHost, proxyPort, proxyUsername, proxyPassword, typeProxy));

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
        private async Task ProcessFakeProxyAllAsync(Models.DeviceModel device, string proxyHost, string proxyPort, string proxyUsername, string proxyPassword, string typeProxy)
        {
            if (!string.IsNullOrEmpty(proxyHost) && !string.IsNullOrEmpty(proxyPort))
            {
                // ok
                try
                {
                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "5%", DevicesLang.logTitleProxy);
                    // var peelProxy = FakeProxyData.Split(':');
                    string proxy = $"{proxyHost}:{proxyPort}:{proxyUsername}:{proxyPassword}";
                    proxy = proxy.Replace("\n", "")
                                 .Replace("\r", "")
                                 .Replace("\t", "");

                    var currentTask = TaskScheduler.FromCurrentSynchronizationContext();
                    await Task.Run(async () =>
                    {
                        bool isFakeTimeZone;
                        string PROXYTYPE = "";
                        if (typeProxy == "HTTP")
                        {
                            isFakeTimeZone = FakeTimeZoneHttp(proxy, device.DeviceId);
                            PROXYTYPE = "http";
                        }
                        else
                        {
                            isFakeTimeZone = FakeTimeZone(proxy, device.DeviceId);
                            PROXYTYPE = "socks5";
                        }
                        if (isFakeTimeZone)
                        {
                            if (typeProxy == "HTTP")
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", DevicesLang.logTitleProxy);
                                Thread.Sleep(10000);
                                string ip = proxyHost;
                                int port = int.Parse(proxyPort);
                                string user = proxyUsername;
                                string password = proxyPassword;
                                string authen = (!string.IsNullOrEmpty(proxyUsername) && !string.IsNullOrEmpty(proxyPassword)) ? $"{proxyUsername}:{proxyPassword}@" : "";
                                string proxyParams = $"{PROXYTYPE}://{proxyHost}:{proxyPort}";
                                if (!string.IsNullOrEmpty(authen))
                                {
                                    proxyParams = $"{PROXYTYPE}://{authen}{proxyHost}:{proxyPort}";
                                }
                                string ipProxyV4 = Tun2socksService.getIpv4HttpProxy(proxy, device.DeviceId);

                                ADBService.enableWifi(false, device.DeviceId);
                                ADBService.rootAndRemount(device.DeviceId);
                                ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                                Tun2socksService.stop(device.DeviceId);
                                Tun2socksService.setUpTun2socksOnDevice("/data/local/tmp", device.DeviceId);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", DevicesLang.logTitleProxy);
                                Tun2socksService.start("/data/local/tmp", proxyParams, ipProxyV4, device.DeviceId);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "60%", "In progress connect wifi..");
                                await Task.Delay(2000);
                                ADBService.enableWifi(true, device.DeviceId);
                                ADBService.openWifiSettings(device.DeviceId);
                                int step = 0;
                                while ((!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId)) || step == 40)
                                {
                                    ADBService.openWifiSettings(device.DeviceId);
                                    Thread.Sleep(3000);
                                }
                                if (step >= 39)
                                {
                                    _processingDeviceIds.Remove(device.DeviceId);
                                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "Fake proxy success, check error - wifi error");
                                    return;
                                }
                                Thread.Sleep(5000);
                                ADBService.OpenBrowserWithUrl("https://browserleaks.com/ip", device.DeviceId);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", DevicesLang.logCheckProxy);
                                await Task.Delay(2000);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", DevicesLang.logTitleProxySuccess);
                            
                            }
                            else
                            {
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "10%", DevicesLang.logTitleProxy);
                                Thread.Sleep(10000);
                                string ip = proxyHost;
                                int port = int.Parse(proxyPort);
                                string user = proxyUsername;
                                string password = proxyPassword;
                                string authen = (!string.IsNullOrEmpty(proxyUsername) && !string.IsNullOrEmpty(proxyPassword)) ? $"{proxyUsername}:{proxyPassword}@" : "";
                                string proxyParams = $"{PROXYTYPE}://{proxyHost}:{proxyPort}";
                                if (!string.IsNullOrEmpty(authen))
                                {
                                    proxyParams = $"{PROXYTYPE}://{authen}{proxyHost}:{proxyPort}";
                                }
                                string ipProxyV4 = Tun2socksService.getIpv4SocksProxy(proxy, device.DeviceId);

                                ADBService.enableWifi(false, device.DeviceId);
                                ADBService.rootAndRemount(device.DeviceId);
                                ADBService.putSetting("http_proxy", ":0", device.DeviceId);
                                Tun2socksService.stop(device.DeviceId);
                                Tun2socksService.setUpTun2socksOnDevice("/data/local/tmp", device.DeviceId);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "50%", DevicesLang.logTitleProxy);
                                Tun2socksService.start("/data/local/tmp", proxyParams, ipProxyV4, device.DeviceId);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "60%", "In progress connect wifi..");
                                await Task.Delay(2000);
                                ADBService.enableWifi(true, device.DeviceId);
                                ADBService.openWifiSettings(device.DeviceId);
                                int step = 0;
                                while ((!ADBService.isWifiConnectedV2(device.DeviceId) && !ADBService.isWifiConnected(device.DeviceId)) || step == 40)
                                {
                                    ADBService.openWifiSettings(device.DeviceId);
                                    Thread.Sleep(3000);
                                }
                                if (step >= 39)
                                {
                                    _processingDeviceIds.Remove(device.DeviceId);
                                    DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", "Fake proxy success, check error - wifi error");
                                    return;
                                }
                                Thread.Sleep(5000);
                                ADBService.OpenBrowserWithUrl("https://browserleaks.com/ip", device.DeviceId);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "99%", DevicesLang.logCheckProxy);
                                await Task.Delay(2000);
                                DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "100%", DevicesLang.logTitleProxySuccess);
                            }
                        }
                        else
                        {
                            _processingDeviceIds.Remove(device.DeviceId);
                            DeviceUpdater.UpdateProgress(Devices, device.DeviceId, "0%", "⚠ Error ! Again fake proxy");
                            return;
                        }
                        _processingDeviceIds.Remove(device.DeviceId);
                    }).ContinueWith(task =>
                    {
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
            else { UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Value latitude and longitude is null !"); }

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
            else { UpdateDeviceStatus(device.DeviceId, "0%", "⚠ Url null"); }

        }
        private async Task CheckBoxDevice(Models.DeviceModel device)
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

            await SaveDevices();
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
        private async Task CreateService()
        {
            await Task.Run(() =>
            {
                var poolId = AppConfigService.ReadSetting("poolId");
                var clientId = AppConfigService.ReadSetting("clientId");
                var cognito = new CognitoService(poolId, clientId);
                var username = AppConfigService.ReadSetting("user");
                var password = AppConfigService.ReadSetting("password");
                var endpoint = AppConfigService.ReadSetting("endpoint");
                refreshToken = cognito.getIdToken(username, password);

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    miChangerGraphQLClient = new MiChangerGraphQLClient(endpoint, ApiAuthenticationType.TOKEN, refreshToken);
                }
                ;
            });
        }
        private bool IsTokenExpired(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var exp = jwt.Payload.Exp;

            if (exp.HasValue)
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.Value);
                return DateTimeOffset.UtcNow > expTime; // còn thời gian token
            }

            return true; // hết hạn token
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
        private bool FakeTimeZoneHttp(string proxy, string deviceId)
        {
            try
            {
                ADBService.enableWifi(false, deviceId);
                var url = "http://ip-api.com/json";
                var proxyParts = proxy.Split(':');
                ADBService.rootAndRemount(deviceId);

                string commandline;
                string str;

                if (proxyParts.Length == 4)
                {
                    // HTTP proxy có username/password
                    commandline = $"curl --proxy http://{proxyParts[2]}:{proxyParts[3]}@{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                }
                else if (proxyParts.Length == 2)
                {
                    // HTTP proxy không cần username/password
                    commandline = $"curl --proxy http://{proxyParts[0]}:{proxyParts[1]} \"{url}\"";
                }
                else
                {
                    // Proxy format không hợp lệ
                    return false;
                }

                str = CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline));

                if (!string.IsNullOrEmpty(str))
                {
                    JObject jsonObject = JObject.Parse(str);
                    ADBService.FakeTimezone(jsonObject["timezone"]?.ToString(), deviceId);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
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