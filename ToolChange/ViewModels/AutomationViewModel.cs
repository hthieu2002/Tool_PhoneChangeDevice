using AuthenticationService;
using MiHttpClient;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using POCO.Models;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ToolChange.Language;
using ToolChange.Models;
using ToolChange.Services;
using ToolChange.Views;
using ToolChange.Views.ControlScriptPage;
using WindowsFormsApp.Script.RoslynScript;
using Xamarin.Forms;

namespace ToolChange.ViewModels
{
    public class AutomationViewModel : INotifyPropertyChanged
    {
        private MiChangerGraphQLClient miChangerGraphQLClient;
        private CognitoService cognitoService;
        private string token;
        private string endpoint = DeepDroid.Properties.Settings.Default.endpoint;
        private string authenticationType = "authorization";
        public LocalizationViewModel LanguageVM { get; set; }
        public AutomationViewModel AutomationListVM { get; set; }
        public ObservableCollection<Models.DeviceModel> Devices { get; private set; } = new ObservableCollection<Models.DeviceModel>();
        private CancellationTokenSource _cancellationTokenSource;
        private static CancellationTokenSource? _cts;

        private readonly string jsonFilePath = Path.Combine("Resources", "Devices", "devices.json");
        private readonly string scriptDirectory = Path.Combine("Resources", "Script");
        private readonly HashSet<string> _processingDeviceIds = new();
        private string _user = DeepDroid.Properties.Settings.Default.user;
        private ObservableCollection<string> _scriptFiles = new();
        private string[] _loadFileScript = Array.Empty<string>();
        private string _selectedFileScript;
        private bool _isCheckedRunFile = false;
        private bool _isDisableRunFile = true;
        private int _runscript = 0;
        private string _btnRun = "Run Script";

        private static List<string> packages = new List<string>();

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
        public int CountRunScript
        {
            get => _runscript;
            set
            {
                if (_runscript != value)
                {
                    _runscript = value;
                    OnPropertyChanged(nameof(CountRunScript));
                }
            }
        }
        public string BtnRun
        {
            get => _btnRun;
            set
            {
                if (_btnRun != value)
                {
                    _btnRun = value;
                    OnPropertyChanged(nameof(BtnRun));
                }
            }
        }

        public ObservableCollection<string> ScriptFiles
        {
            get => _scriptFiles;
            set
            {
                if (_scriptFiles != value)
                {
                    _scriptFiles = value;
                    OnPropertyChanged(nameof(ScriptFiles));
                }
            }
        }

        public string[] LoadFileScript
        {
            get => _loadFileScript;
            set
            {
                if (!AreArraysEqual(_loadFileScript, value))
                {
                    _loadFileScript = value ?? Array.Empty<string>();
                    OnPropertyChanged(nameof(LoadFileScript));

                    if (_loadFileScript.Length > 0 && string.IsNullOrEmpty(_selectedFileScript))
                    {
                        SelectedFileScript = _loadFileScript[0];
                    }
                }
            }
        }

        public string SelectedFileScript
        {
            get => _selectedFileScript;
            set
            {
                if (_selectedFileScript != value)
                {
                    _selectedFileScript = value;
                    OnPropertyChanged(nameof(SelectedFileScript));
                }
            }
        }
        public bool IsCheckedRunFile
        {
            get => _isCheckedRunFile;
            set
            {
                if (_isCheckedRunFile != value)
                {
                    _isCheckedRunFile = value;
                    if (value == true)
                    {
                        IsDisableRunFile = false;
                    }
                    else
                    {
                        IsDisableRunFile = true;
                    }
                    OnPropertyChanged(nameof(IsCheckedRunFile));
                }
            }
        }
        public bool IsDisableRunFile
        {
            get => _isDisableRunFile;
            set
            {
                if (_isDisableRunFile != value)
                {
                    _isDisableRunFile = value;
                    OnPropertyChanged(nameof(IsDisableRunFile));
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

                    //  _isUpdatingCheckAll = true; // ⚠️ bắt đầu chặn trigger
                    foreach (var device in Devices)
                    {
                        device.IsChecked = value;
                    }
                    //   _isUpdatingCheckAll = false; // ✅ cho phép lại
                }
            }
        }
        private string _zipUrl = "";
        public string ZipUrl
        {
            get => _zipUrl;
            set
            {
                if (_zipUrl != value)
                {
                    _zipUrl = value;
                    OnPropertyChanged(nameof(ZipUrl));

                }
            }
        }
        private string _zipPassword = "";
        public string ZipPassword
        {
            get => _zipPassword;
            set
            {
                if (_zipPassword != value)
                {
                    _zipPassword = value;
                    OnPropertyChanged(nameof(ZipPassword));

                }
            }
        }
        private bool _isBackupAccount = false;
        public bool IsBackupAccount
        {
            get => _isBackupAccount;
            set
            {
                if (_isBackupAccount != value)
                {
                    _isBackupAccount = value;

                    if (_isBackupAccount)
                    {
                        AddPackages(new[]
                        {
                    "com.android.vending",
                    "com.google.android.gsf",
                    "com.google.android.gms"
                });
                    }
                    else
                    {
                        RemovePackages(new[]
                        {
                    "com.android.vending",
                    "com.google.android.gsf",
                    "com.google.android.gms"
                });
                    }

                    OnPropertyChanged(nameof(IsBackupAccount));
                }
            }
        }

        public ICommand IsCheckBoxDevice { get; private set; }
        public ICommand LoadDevicesCommand { get; private set; }
        public ICommand BackupDevicesCommand { get; private set; }
        public ICommand ListAppBackupDeviceCommand { get; private set; }
        public ICommand FixBackupDeviceCommand { get; private set; }
        public ICommand RestoreDevicesCommand { get; private set; }
        public ICommand ScreenShotDevicesCommand { get; private set; }
        public ICommand FixRebootDeviceCommand { get; private set; }
        public ICommand LoadFileCommand { get; private set; }
        public ICommand RunScriptCommand { get; private set; }
        public AutomationViewModel()
        {
            _ = LoadDevices();

            cognitoService = new CognitoService(DeepDroid.Properties.Settings.Default.poolId, DeepDroid.Properties.Settings.Default.clientId);

            token = cognitoService.getIdToken(DeepDroid.Properties.Settings.Default.user, DeepDroid.Properties.Settings.Default.password);

            //string token = "eyJraWQiOiJGallIT0JuUERvNTdXMENjWHlQNEdvOGFCbEd1NEFnUDNYZEtGNTluQzF3PSIsImFsZyI6IlJTMjU2In0.eyJzdWIiOiIwMTM5ZDBiZC01MzI1LTQwZGQtODY0Yi0wZDNkOGFjNmZlZjAiLCJhdWQiOiIzZ29zNWppbWliODJqbDNmOXZjNjI4M2twciIsImNvZ25pdG86Z3JvdXBzIjpbIlN0YW5kYXJkIl0sImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJldmVudF9pZCI6ImIxMjlmZDhlLTE5MDItNGM3Zi1hYzBhLWUyOGQ3YWNhYTlmZiIsInRva2VuX3VzZSI6ImlkIiwiYXV0aF90aW1lIjoxNTk5MjA2ODU5LCJpc3MiOiJodHRwczpcL1wvY29nbml0by1pZHAuYXAtc291dGhlYXN0LTEuYW1hem9uYXdzLmNvbVwvYXAtc291dGhlYXN0LTFfaG5WWGljam9sIiwiY29nbml0bzp1c2VybmFtZSI6IjAxMzlkMGJkLTUzMjUtNDBkZC04NjRiLTBkM2Q4YWM2ZmVmMCIsImV4cCI6MTU5OTIxMDQ1OSwiaWF0IjoxNTk5MjA2ODU5LCJlbWFpbCI6ImRldkB5b3BtYWlsLmNvbSJ9.WUZ3aW97f9oHXv_WSpeM3zUCtS5End-_F9fI8mjj3XMIsvyDTERmWrK5zWxHBeSEOgItmAJrMk3OWEg7bOE-8V98M9c921_MVP58uhgbWZHeXAnRgLDzZASOVE0pdPcjxbXGY9MxeWUNNp39U9E4Fo1YIrZbmS4fVHXVrhP4dhblAmsloroLPc-cBuslHYyHrRc9dLw-1f4Dacnvcd_J2Y8Lv_EvivsMuVNx5SYgnbLC7SsJ2_JNecSq1WdWGneiwuamkkzXDcmv644z7U6WWRyi9FeE0YP0hD09JXyN5CJRIWt563XR2684mf4o_xWbwZiS0KtjSio_D4sE88yyCg";
            miChangerGraphQLClient = new MiChangerGraphQLClient(endpoint, authenticationType, token);

            LoadDevicesCommand = new RelayCommand(async () => await LoadDevicesAsync());
            BackupDevicesCommand = new RelayCommand(async () => await BackupDevicesAsync());
            FixRebootDeviceCommand = new RelayCommand(async () => await FixRebootDeviceAsync());
            ListAppBackupDeviceCommand = new RelayCommand(async () => await ListAppBackupDeviceAsync());
            FixBackupDeviceCommand = new RelayCommand(async () => await FixBackupDeviceAsync());
            RestoreDevicesCommand = new RelayCommand(async () => await RestoreDevicesAsync());
            ScreenShotDevicesCommand = new RelayCommand(async () => await Screenshot());
            RunScriptCommand = new RelayCommand(async () => await RunScript());
            LoadFileCommand = new RelayCommand(async () => await LoadFileScriptFunc());

            IsCheckBoxDevice = new RelayCommand<Models.DeviceModel>(CheckBoxDevice);

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
        private void AttachPropertyChanged(Models.DeviceModel device)
        {
            device.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Models.DeviceModel.IsChecked))
                {
                    if (_isUpdatingCheckAll) return; // ✅ Đang trong quá trình cập nhật từ IsAllChecked → bỏ qua

                    // Cập nhật lại IsAllChecked theo danh sách
                    _isAllChecked = Devices.All(d => d.IsChecked);
                    OnPropertyChanged(nameof(IsAllChecked));
                }
            };
        }
        public void AsyncTask()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            var tk = _cts.Token;

            Task.Run(async () =>
            {
                while (!tk.IsCancellationRequested)
                {
                    if (await DeviceSync.Mutex.WaitAsync(0, tk))
                    {
                        try
                        {
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
            _cts?.Cancel();
            _cts = null;
        }

        private async Task LoadDevicesAsync()
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
        private async Task FixBackupDeviceAsync()
        {
            try
            {
                var selectedDevices = Devices.Where(device => device.IsChecked).ToList();
                int selectedCount = selectedDevices.Count;

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

                    tasks.Add(ProcessFixBackupAllAsync(device.DeviceId));

                }
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {

            }
        }
        private async Task ListAppBackupDeviceAsync()
        {
            try
            {
                
                ObservableCollection<Models.AppItem> ItemDevice = new ObservableCollection<Models.AppItem>();
                var selectedDevices = Devices.Where(device => device.IsChecked && device.Status == "Online").ToList();
                int selectedCount = selectedDevices.Count;

                if (selectedCount == 0)
                {
                    System.Windows.MessageBox.Show(DevicesLang.logSelectDeviceChange, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var model = new AppListViewModel(selectedDevices[0]);
                var log = new ListAppPackage(selectedDevices[0])
                {
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
                    ItemDevice = model.ListB;

                    var importantPackages = new List<string>
{
    "com.android.vending",
    "com.google.android.gsf",
    "com.google.android.gms"
};

                    var selectedPackages = ItemDevice.Select(app => app.Package).ToList();
                    bool hasImportant = selectedPackages.Any(pkg => importantPackages.Contains(pkg));

                    if (hasImportant)
                    {
                        packages = selectedPackages
                            .Where(pkg => importantPackages.Contains(pkg))
                            .ToList();
                    }
                    else
                    {
                        packages = selectedPackages;
                    }
                }

                // OK

                Debug.WriteLine($"Selected packages for backup: {string.Join(", ", packages)}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        private async Task FixRebootDeviceAsync()
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
                    UpdateDeviceStatus(device.DeviceId, "0%", "Fix reboot");
                    tasks.Add(ProcessFixRebootAsync(device));
                }
                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error backup devices: {ex.Message}");
            }
        }
        private async Task BackupDevicesAsync()
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
                    UpdateDeviceStatus(device.DeviceId, "0%", "Start backup");
                    tasks.Add(ProcessBackupDevicesAsync(device));
                }
                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error backup devices: {ex.Message}");
            }
        }
        private async Task RestoreDevicesAsync()
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
                    UpdateDeviceStatus(device.DeviceId, "0%", "Start restore");
                    tasks.Add(ProcessRestoreDevicesAsync(device));
                }
                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error backup devices: {ex.Message}");
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
                    UpdateDeviceStatus(device.DeviceId, "0%", "Start screenshot");
                    tasks.Add(ProcessScreenShotDeviceAsync(device));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private async Task RunScript()
        {
            if (BtnRun == "Run Script")
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

                    if (SelectedFileScript == "" || SelectedFileScript == null)
                    {
                        System.Windows.MessageBox.Show(AutomationLang.logRunScript, Lang.LogError, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    BtnRun = "Stop";
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

                        tasks.Add(ProcessRunScriptDeviceAsync(device));
                    }
                    await Task.WhenAll(tasks);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                _cancellationTokenSource.Cancel();
                _processingDeviceIds.Clear();
                BtnRun = "Run Script";
            }
        }
        private async Task LoadFileScriptFunc()
        {
            try
            {
                var files = LoadScriptFiles().ToArray();
                if (files.Length == 0)
                {
                    System.Windows.MessageBox.Show(AutomationLang.logLoadScript, Lang.LogInfomation, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadFileScript = files;
                    SelectedFileScript = files.Length > 0 ? files[0] : null;
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                });
            }
        }
        private IEnumerable<string> LoadScriptFiles()
        {
            try
            {
                if (!Directory.Exists(scriptDirectory))
                {
                    Directory.CreateDirectory(scriptDirectory);
                }

                return Directory.GetFiles(scriptDirectory)
                    .Select(Path.GetFileName)
                    .Where(file => file.EndsWith(".txt"));
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<string>();
            }
        }
        private async Task ProcessScreenShotDeviceAsync(Models.DeviceModel device)
        {

            ADBService.ScreenshotAdb(device.DeviceId);
            UpdateDeviceStatus(device.DeviceId, "100%", "Success screenshot");

            _processingDeviceIds.Remove(device.DeviceId);
        }

        private async Task ProcessFixRebootAsync(Models.DeviceModel device)
        {
            ADBService.runCMDRoot("reboot",device.DeviceId);
            UpdateDeviceStatus(device.DeviceId, "100%", "Fix Success");

            _processingDeviceIds.Remove(device.DeviceId);
        }

        private async Task ProcessBackupDevicesAsync(Models.DeviceModel device)
        {

            string folderPath = Path.Combine(System.Windows.Forms.Application.StartupPath, device.DeviceId);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string backupPath = Path.Combine(System.Windows.Forms.Application.StartupPath, device.DeviceId, string.Concat($"dataBackupFullInfo_{device.DeviceId}"));
            var isBackupDone = await BackUpDeviceAsync(device.DeviceId, packages);

            if (isBackupDone)
            {
                ADBService.runCMDRoot("shell service call notification 1 s16 \"com.google.android.gms\"", device.DeviceId);
                UpdateDeviceStatus(device.DeviceId, "100%", "Backup success");
                _processingDeviceIds.Remove(device.DeviceId);
            }
            else
            {
                UpdateDeviceStatus(device.DeviceId, "100%", "Backup error");
                _processingDeviceIds.Remove(device.DeviceId);
            }
            _processingDeviceIds.Remove(device.DeviceId);
        }
        public async Task<bool> BackUpDeviceAsync(string deviceId, List<string> packages)
        {
            return await Task.Run(() =>
            {
                if (packages.Count == 0) return false;

                // Gắn thêm timestamp để tránh trùng
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var destinationZipPath = $"./Resources/Backup/backup_{deviceId}_{timestamp}.zip";

                // Tạo thư mục tạm riêng cho mỗi thiết bị
                string tempLocalFolder = Path.Combine(Path.GetTempPath(), $"AndroidBackupTemp_{deviceId}_{Guid.NewGuid()}");

                try
                {
                    Directory.CreateDirectory(tempLocalFolder);

                    string backupFolder = Path.GetDirectoryName(destinationZipPath);
                    if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                    bool isFullSystemBackup = packages.Any(p => p.Contains("com.google.android.gms"));
                    var androidPaths = new HashSet<string>();

                    if (isFullSystemBackup)
                    {
                        androidPaths.UnionWith(new[]
                        {
                    "/system/build.prop.min",
                    "/system/vendor/build.prop.min",
                    "/data/system/notification_policy.xml",
                    "/data/system/package",
                    "/data/system/notification_log.xml",
                    "/data/system/locksettings.db",
                    "/data/system/users/0/app_idle_stats.xml",
                    "/data/system/users/0/runtime-permissions.xml",
                    "/data/system/users/0/appwidgets.xml",
                    "/data/system/users/0/settings_ssaid.xml",
                    "/data/system/users/0/package-restrictions.xml",
                    "/data/system/users/0/settings_system.xml",
                    "/data/system/users/0/wallpaper_info.xml",
                    "/data/system",
                    "/data/system_ce",
                    "/data/system_de",
                    "/data/system_ce/0/accounts_ce.db",
                    "/data/system/syncmanager.db",
                    "/data/system/users/0/accounts.db",
                    "/data/system/sync",
                    "/data/misc/keystore",
                    "/data/misc/user/0",
                    "/data/misc/keychain",
                    "/data/misc/profiles"
                });
                    }
                    else
                    {
                        androidPaths.UnionWith(new[]
{
    $"/data/system/users/0/runtime-permissions.xml",
    $"/data/system/users/0/package-restrictions.xml",
    $"/data/system_ce/0/accounts_ce.db",
    $"/data/system/users/0/accounts.db",
    $"/data/misc/keystore",
    $"/data/misc/keychain"
});

                    }

                    foreach (var pkg in packages.Distinct())
                        {
                            UpdateDeviceStatus(deviceId, "30%", $"copy {pkg}");

                            androidPaths.Add($"/data/data/{pkg}");
                            androidPaths.Add($"/data/data/{pkg}/lib");
                            androidPaths.Add($"/data/user_de/0/{pkg}");
                            androidPaths.Add($"/sdcard/Android/data/{pkg}");
                            androidPaths.Add($"/sdcard/Android/data/{pkg}/files");

                            if (pkg != "com.android.vending" && pkg != "com.google.android.gsf" && pkg != "com.google.android.gms")
                            {
                                androidPaths.Add($"/sdcard/Android/obb/{pkg}");
                                androidPaths.Add($"/data/user/0/{pkg}");
                            }
                        }

                    ADBService.runCMDRoot("root", deviceId);
                    ADBService.runCMDRoot("remount", deviceId);

                    foreach (var path in androidPaths)
                    {
                        string relativePath = path.TrimStart('/');
                        string localPath = Path.Combine(tempLocalFolder, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                        }
                        catch { /* Ignore directory create issues */ }

                        var lsResult = ADBService.runCMDRoot($"shell ls \"{path}\"", deviceId);
                        if (lsResult.Contains("No such file or directory"))
                        {
                            Console.WriteLine($"⚠️ Skipped missing path: {path}");
                            continue;
                        }

                        var pullCmd = $"pull \"{path}\" \"{localPath}\"";
                        var result = ADBService.runCMDRoot(pullCmd, deviceId);

                        if (result?.Contains("failed") == true)
                        {
                            Console.WriteLine($"❌ Failed to pull: {path}");
                            Console.WriteLine(result);
                        }
                        else
                        {
                            UpdateDeviceStatus(deviceId, "50%", $"✅ Pulled: {path}");
                        }
                    }

                    try
                    {
                        if (File.Exists(destinationZipPath))
                            File.Delete(destinationZipPath);

                        ZipFile.CreateFromDirectory(tempLocalFolder, destinationZipPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Error creating ZIP: " + ex.Message);
                        return false;
                    }

                    return File.Exists(destinationZipPath);
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempLocalFolder))
                            Directory.Delete(tempLocalFolder, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("⚠️ Failed to delete temp folder: " + ex.Message);
                    }
                }
            });
        }

        private async Task ProcessFixBackupAllAsync(string device)
        {
            ADBService.runCMDRoot("shell am force-stop com.android.vending", device);
            ADBService.runCMDRoot("shell pm clear com.google.android.gms\r\n", device);
            ADBService.runCMDRoot("shell pm clear com.android.vending", device);
            ADBService.runCMDRoot("shell am start -n com.android.vending/com.android.vending.AssetBrowserActivity", device);
            await Task.Delay(2000);
            ADBService.runCMDRoot("shell service call notification 1 s16 \"com.google.android.gms\"", device);
            DeviceUpdater.UpdateProgress(Devices, device, "100%", "Fix success !");
            _processingDeviceIds.Remove(device);
        }
        private async Task ProcessRestoreDevicesAsync(Models.DeviceModel device)
        {
            string folderPath = Path.Combine(System.Windows.Forms.Application.StartupPath, device.DeviceId);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (string.IsNullOrEmpty(ZipUrl))
            {
                UpdateDeviceStatus(device.DeviceId, "0%", "Error not data backup !");
                _processingDeviceIds.Remove(device.DeviceId);
                return;
            }
            string restorePath = Path.Combine(System.Windows.Forms.Application.StartupPath, device.DeviceId, $"dataBackupFullInfo_{device.DeviceId}.7z");
            string zip = $"./Resources/Backup/{ZipUrl}";
            // ⚠️ CHỖ NÀY GỌI BẰNG `await Task.Run(...)`
            bool isRestoreDone = await Task.Run(() =>
            {
                return restoreFullInfoExtra(zip, device.DeviceId);
            });

            if (isRestoreDone)
            {
                UpdateDeviceStatus(device.DeviceId, "100%", "Restore success");

            }
            else
            {
                UpdateDeviceStatus(device.DeviceId, "100%", "Restore Error");
            }

            _processingDeviceIds.Remove(device.DeviceId);
        }
        public bool restoreFullInfo(string fromDesktopFullPath, string deviceId)
        {
            if (!File.Exists(fromDesktopFullPath))
            {
                _processingDeviceIds.Remove(deviceId);
                throw new FileNotFoundException("❌ Zip file not found: " + fromDesktopFullPath);
            }

            // ⚠️ Cấp quyền root và remount để ghi vào phân vùng hệ thống
            ADBService.runCMDRoot("root", deviceId);
            ADBService.runCMDRoot("remount", deviceId);
            ADBService.runCMDRoot("shell \"mount -o rw,remount rootfs\"", deviceId);

            string tempExtractDir = Path.Combine(Path.GetTempPath(), "RestoreZip_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempExtractDir);

            try
            {
                ZipFile.ExtractToDirectory(fromDesktopFullPath, tempExtractDir);
                Console.WriteLine("✅ Extracted to temp folder: " + tempExtractDir);
                UpdateDeviceStatus(deviceId, "1%", "✅ Extracted to temp folder: " + tempExtractDir);

                var files = Directory.GetFiles(tempExtractDir, "*", SearchOption.AllDirectories);
                int count = 10;

                foreach (var file in files)
                {
                    if (count < 90) count++;

                    string relativePath = Path.GetRelativePath(tempExtractDir, file).Replace("\\", "/");
                    string remotePath = "/" + relativePath; // ⚠️ Gửi đúng path gốc

                    string remoteDir = Path.GetDirectoryName(remotePath).Replace("\\", "/");
                    ADBService.runCMDRoot($"shell \"mkdir -p \\\"{remoteDir}\\\"\"", deviceId);

                    var pushCmd = $"push \"{file}\" \"{remotePath}\"";
                    ADBService.runCMDRoot(pushCmd, deviceId);

                    Debug.WriteLine($"📤 Pushed: {relativePath} ➜ {remotePath}");
                    UpdateDeviceStatus(deviceId, $"{count}%", $"📤 Pushed: {relativePath}");
                }

                ADBService.runCMDRoot($"shell \"rm -rf /sdcard/*.min\"", deviceId);

                return true;
            }
            finally
            {
                if (Directory.Exists(tempExtractDir))
                {
                    Directory.Delete(tempExtractDir, true);
                    Console.WriteLine("🧹 Temp folder deleted.");
                }
            }
        }
        //public bool restoreFullInfoExtra(string fromDesktopFullPath, string deviceId)
        //{
        //    if (!File.Exists(fromDesktopFullPath))
        //    {
        //        _processingDeviceIds.Remove(deviceId);
        //        throw new FileNotFoundException("❌ Zip file not found: " + fromDesktopFullPath);
        //    }

        //    // Cấp quyền root và remount
        //    ADBService.runCMDRoot("root", deviceId);
        //    ADBService.runCMDRoot("remount", deviceId);
        //    ADBService.runCMDRoot("shell \"mount -o rw,remount rootfs\"", deviceId);

        //    string remoteZipPath = "/sdcard/restore_temp.zip";

        //    // 1. Push ZIP file một lần duy nhất
        //    ADBService.runCMDRoot($"push \"{fromDesktopFullPath}\" \"{remoteZipPath}\"", deviceId);
        //    UpdateDeviceStatus(deviceId, "25%", $"✅ Pushed zip to: {remoteZipPath}");

        //    // 2. Giải nén trên thiết bị bằng `unzip`
        //    ADBService.runCMDRoot($"shell \"rm -rf /data/local/tmp/restore && mkdir -p /data/local/tmp/restore && unzip -o {remoteZipPath} -d /data/local/tmp/restore\"", deviceId);
        //    UpdateDeviceStatus(deviceId, "50%", $"✅ Unzipped on device");

        //    // 3. Di chuyển từng file về đúng vị trí gốc (nếu cần)
        //    ADBService.runCMDRoot($"shell \"cp -r /data/local/tmp/restore/* /\"", deviceId);
        //    UpdateDeviceStatus(deviceId, "90%", $"✅ Restored data to rootfs");

        //    // 4. Xóa file tạm
        //    ADBService.runCMDRoot($"shell \"rm -rf /data/local/tmp/restore {remoteZipPath}\"", deviceId);
        //    UpdateDeviceStatus(deviceId, "100%", $"🧹 Cleaned up");

        //    Task.Delay(5000).Wait(); // Đợi 3 giây để đảm bảo backup hoàn tất
        //    ADBService.runCMDRoot("reboot", deviceId);
        //    return true;
        //}
        public bool restoreFullInfoExtra(string fromDesktopFullPath, string deviceId)
        {
            if (!File.Exists(fromDesktopFullPath))
            {
                _processingDeviceIds.Remove(deviceId);
                throw new FileNotFoundException("❌ Zip file not found: " + fromDesktopFullPath);
            }

            // Danh sách các từ khóa cần bỏ qua (file dễ gây lỗi nav bar)
            string[] skipKeywords = {
        "systemui", "overlays", "display-manager-state.xml",
        "settings_system.xml", "settings_secure.xml", "settings_global.xml",
        "navigation", "framework-res", "input", "device_policies.xml"
    };

            // 1. Giải nén ZIP ra thư mục tạm trên PC
            string tempExtractDir = Path.Combine(Path.GetTempPath(), "restore_temp_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempExtractDir);

            System.IO.Compression.ZipFile.ExtractToDirectory(fromDesktopFullPath, tempExtractDir);

            // 2. Lọc bỏ file có chứa từ khóa nguy hiểm
            foreach (var file in Directory.GetFiles(tempExtractDir, "*", SearchOption.AllDirectories))
            {
                string lower = file.ToLower();
                if (skipKeywords.Any(k => lower.Contains(k)))
                {
                    Console.WriteLine($"[SKIP] {file}");
                    File.Delete(file);
                }
            }

            // 3. Nén lại ZIP an toàn để đẩy lên thiết bị
            string safeZipPath = Path.Combine(Path.GetTempPath(), "restore_safe.zip");
            if (File.Exists(safeZipPath)) File.Delete(safeZipPath);
            System.IO.Compression.ZipFile.CreateFromDirectory(tempExtractDir, safeZipPath);

            // 4. Thực hiện restore trên thiết bị
            ADBService.runCMDRoot("root", deviceId);
            ADBService.runCMDRoot("remount", deviceId);
            ADBService.runCMDRoot("shell \"mount -o rw,remount rootfs\"", deviceId);

            string remoteZipPath = "/sdcard/restore_safe.zip";
            ADBService.runCMDRoot($"push \"{safeZipPath}\" \"{remoteZipPath}\"", deviceId);
            UpdateDeviceStatus(deviceId, "25%", $"✅ Pushed safe zip to: {remoteZipPath}");

            // Giải nén an toàn
            ADBService.runCMDRoot($"shell \"rm -rf /data/local/tmp/restore && mkdir -p /data/local/tmp/restore && unzip -o {remoteZipPath} -d /data/local/tmp/restore\"", deviceId);
            UpdateDeviceStatus(deviceId, "50%", $"✅ Unzipped on device");

            // Copy dữ liệu
            ADBService.runCMDRoot($"shell \"cp -r /data/local/tmp/restore/* /\"", deviceId);
            UpdateDeviceStatus(deviceId, "90%", $"✅ Restored safe data to rootfs");

            // Xóa file tạm
            ADBService.runCMDRoot($"shell \"rm -rf /data/local/tmp/restore {remoteZipPath}\"", deviceId);
            UpdateDeviceStatus(deviceId, "100%", $"🧹 Cleaned up");

            Task.Delay(3000).Wait();
            ADBService.runCMDRoot("reboot", deviceId);

            return true;
        }


        public static void AddPackage(string packageName)
        {
            if (!packages.Contains(packageName))
            {
                packages.Add(packageName);
            }
        }
        public static void AddPackages(IEnumerable<string> packageNames)
        {
            foreach (var pkg in packageNames)
            {
                AddPackage(pkg);
            }
        }
        public static void RemovePackages(IEnumerable<string> packageNames)
        {
            foreach (var pkg in packageNames)
            {
                packages.Remove(pkg);
            }
        }
        public static void RemovePackage(string packageName)
        {
            if (packages.Contains(packageName))
            {
                packages.Remove(packageName);
            }
        }

        private async Task ProcessRunScriptDeviceAsync(Models.DeviceModel device)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;
            try
            {
                if (!IsDisableRunFile)
                {
                    int count = 0;
                    await Task.Run(() =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            count++;
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateDeviceStatus(device.DeviceId, "1", $"{AutomationLang.logUntimateRunSctiptInfo} {count}");
                            });

                            RoslynScriptAutomation.Run($"./Resources/script/{SelectedFileScript}", device.DeviceId, token);
                        }
                    });
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateDeviceStatus(device.DeviceId, "100", $"{AutomationLang.logUntimateRunSctiptInfoSuccess}");
                    });

                    _processingDeviceIds.Remove(device.DeviceId);
                }
                else
                {
                    await Task.Run(() =>
                    {
                        if (CountRunScript == 0) { return; }
                        for (int i = 0; i < CountRunScript; i++)
                        {
                            if (token.IsCancellationRequested) return;
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateDeviceStatus(device.DeviceId, "1", $"{AutomationLang.logRunSctiptInfo} {i + 1}");
                            });

                            RoslynScriptAutomation.Run($"./Resources/script/{SelectedFileScript}", device.DeviceId, token);
                        }
                    });
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateDeviceStatus(device.DeviceId, "100", $"{AutomationLang.logRunSctiptInfoSuccess}");
                    });

                    _processingDeviceIds.Remove(device.DeviceId);
                }

            }
            catch (Exception e)
            {

            }

        }
        public void UpdateDeviceStatus(string deviceId, string newPercentage, string newProgress)
        {
            var device = Devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device == null) return;

            // Kiểm tra Dispatcher hiện tại
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                {
                    device.Percentage = newPercentage;
                    device.Progress = newProgress;
                });
            }
            else
            {
                device.Percentage = newPercentage;
                device.Progress = newProgress;
            }
        }
        private async Task UpdateDevicesStatus()
        {
            try
            {
                var adbDevices = await GetDevicesFromAdbAsync();
                var adbDeviceDict = adbDevices.ToDictionary(d => d.DeviceId, d => d);

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
                        : JsonSerializer.Deserialize<ObservableCollection<Models.DeviceModel>>(jsonContent) ?? new ObservableCollection<Models.DeviceModel>();
                }
                else
                {
                    Devices = new ObservableCollection<Models.DeviceModel>();
                }

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
        private bool AreArraysEqual(string[] array1, string[] array2)
        {
            if (ReferenceEquals(array1, array2)) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
