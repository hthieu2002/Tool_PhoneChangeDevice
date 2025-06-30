using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ToolChange.Language;
using ToolChange.Services;
using ToolChange.Views;
using WindowsFormsApp.Script.RoslynScript;

namespace ToolChange.ViewModels
{
    public class AutomationViewModel : INotifyPropertyChanged
    {
        public LocalizationViewModel LanguageVM { get; set; }
        public AutomationViewModel AutomationListVM { get; set; }
        public ObservableCollection<Models.DeviceModel> Devices { get; private set; } = new ObservableCollection<Models.DeviceModel>();
        private CancellationTokenSource _cancellationTokenSource;
        private static CancellationTokenSource? _cts;

        private readonly string jsonFilePath = Path.Combine("Resources", "Devices", "devices.json");
        private readonly string scriptDirectory = Path.Combine("Resources", "Script");
        private readonly HashSet<string> _processingDeviceIds = new();
        private string _user = Properties.Settings.Default.user;
        private ObservableCollection<string> _scriptFiles = new();
        private string[] _loadFileScript = Array.Empty<string>();
        private string _selectedFileScript;
        private bool _isCheckedRunFile = false;
        private bool _isDisableRunFile = true;
        private int _runscript = 0;
        private string _btnRun = "Run Script";
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

        public ICommand IsCheckBoxDevice { get; private set; }
        public ICommand LoadDevicesCommand { get; private set; }
        public ICommand ScreenShotDevicesCommand { get; private set; }
        public ICommand LoadFileCommand { get; private set; }
        public ICommand RunScriptCommand { get; private set; }
        public AutomationViewModel()
        {
            _ = LoadDevices();
          //  AsyncTask();
            //DispatcherTimer timer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromSeconds(2)
            //};
            //timer.Tick += async (s, e) =>
            //{
            //    await UpdateDevicesStatus();
            //};
            //timer.Start();
            LoadDevicesCommand = new RelayCommand(async () => await LoadDevicesAsync());
            ScreenShotDevicesCommand = new RelayCommand(async () => await Screenshot());
            RunScriptCommand = new RelayCommand(async () => await RunScript());
            LoadFileCommand = new RelayCommand(async () => await LoadFileScriptFunc());
           
            IsCheckBoxDevice = new RelayCommand<Models.DeviceModel>(CheckBoxDevice);
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

                    await Task.Delay(1000, tk);
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
                        continue;
                    if (_processingDeviceIds.Contains(device.DeviceId))
                        continue;

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
                            continue;
                        if (_processingDeviceIds.Contains(device.DeviceId))
                            continue;

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
                        if(CountRunScript == 0) { return; }
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
