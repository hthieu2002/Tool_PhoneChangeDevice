using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;
using ToolChange.Services;
using ToolChange.Views;
using ToolChange.Views.ControlScriptPage;

namespace ToolChange.ViewModels
{
    public class ScriptAutomationViewModel : INotifyPropertyChanged
    {
        public LocalizationViewModel LanguageVM { get; set; } // language
        public ScriptAutomationViewModel ScriptAutomationVM { get; set; } // language
        private readonly string scriptDirectory = Path.Combine("Resources", "Script");
        private string _textBoxContent;
        private string _textSendContext;
        private bool _isEditScriptBoxContent = true;
        private string _saveFilePath = @"./Resources/Script/";
        private string[] _loadFileScript;
        private string _selectedFileScript;
        private string _selectedDevice;
        private int clickCount = 0;
        private double x1, y1, x2, y2 = 0;
        private double _clickedX, _clickedY;
        private string _clickedPosition;
        private string _titleTest = "Test";
        private List<UiElement> _uiElements;
        private ObservableCollection<string> _adbDevices = new ObservableCollection<string>();
        private Process scrcpyProcess;
        public ObservableCollection<CustomKeyValuePair> ElementDetailPairs { get; set; } = new();

        private string _mousePosition;
        public string MousePosition
        {
            get => _mousePosition;
            set
            {
                if (_mousePosition != value)
                {
                    _mousePosition = value;
                   
                    OnPropertyChanged(nameof(MousePosition));
                }
            }
        }
        public double ClickedX
        {
            get => _clickedX;
            set
            {
                _clickedX = value;
                OnPropertyChanged(nameof(ClickedX));
            }
        }

        public double ClickedY
        {
            get => _clickedY;
            set
            {
                _clickedY = value;
                OnPropertyChanged(nameof(ClickedY));
               
            }
        }
        public string TitleTest
        {
            get => _titleTest;
            set
            {
                _titleTest = value;
                OnPropertyChanged(nameof(TitleTest));
               
            }
        }
        public void UpdateTextSendContext()
        {
          //  ClickedPosition = $"[ CLICK: {ClickedX:0} : {ClickedY:0} ]";

            if (string.IsNullOrWhiteSpace(TextSendContext))
                return;

            string command = TextSendContext.Split('(')[0].Trim();

            if (command == "ClickXY")
            {
                TextSendContext = "";
                TextSendContext = $"ClickXY({ClickedX:0} {ClickedY:0})";
            }
            else if (command == "Swipe")
            {
                clickCount++;
              //  System.Windows.MessageBox.Show(clickCount+"");
                if (clickCount % 2 == 0)
                {
                    x2 = ClickedX;
                    y2 = ClickedY;

                 //   System.Windows.MessageBox.Show("x2 :" + x2 +" \n y2:"+y2);
                }
                else
                {
                    x1 = ClickedX;
                    y1 = ClickedY;

                 //   System.Windows.MessageBox.Show("x1 :" + x1 + " \n y1:" + y1);
                }

                if (x2 == 0 && y2 == 0)
                {
                    TextSendContext = "";
                    TextSendContext = $"Swipe({x1:0} {y1:0} null null 500)";
                }
                else
                {
                    TextSendContext = "";
                    TextSendContext = $"Swipe({x1:0} {y1:0} {x2:0} {y2:0} 500)";
                }
            }

            OnPropertyChanged(nameof(TextSendContext));
        }

        public string ClickedPosition
        {
            get => _clickedPosition;
            private set
            {
                _clickedPosition = value;
                OnPropertyChanged(nameof(ClickedPosition));
            }
        }

        public void UpdateClickedPosition(System.Windows.Point p)
        {
            ClickedPosition = $"[ CLICK: {p.X:0} : {p.Y:0} ]";
        }
        public void UpdateMousePosition(System.Windows.Point p)
        {
            MousePosition = $"[ {p.X:0} : {p.Y:0} ]";
        }

        private BitmapImage _screenshotImage;
        public BitmapImage ScreenshotImage
        {
            get => _screenshotImage;
            set
            {
                _screenshotImage = value;
                OnPropertyChanged(nameof(ScreenshotImage));
            }
        }
        private string _logText;
        public string LogText
        {
            get => _logText;
            set
            {

                _logText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }
        public ObservableCollection<string> AdbDevices
        {
            get => _adbDevices;
            set
            {
                _adbDevices = value;
                OnPropertyChanged(nameof(AdbDevices));
            }
        }

        public string SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public string TextBoxContent
        {
            get => _textBoxContent;
            set
            {
                _textBoxContent = value;
                OnPropertyChanged(nameof(TextBoxContent));
            }
        }
        public string TextSendContext
        {
            get => _textSendContext;
            set
            {
                _textSendContext = value;
                OnPropertyChanged(nameof(TextSendContext));
            }
        }
        public bool IsEditScriptBoxContent
        {
            get => _isEditScriptBoxContent;
            set
            {
                _isEditScriptBoxContent = value;
                OnPropertyChanged(nameof(IsEditScriptBoxContent));
                UpdateSendCommand();
            }
        }

        public string SaveFilePath
        {
            get => _saveFilePath;
            set
            {
                _saveFilePath = value;
                OnPropertyChanged(nameof(SaveFilePath));
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

                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Script", value ?? "");
                    TextBoxContent = File.Exists(filePath) ? File.ReadAllText(filePath) : "";
                }
            }
        }

        public ICommand AppendTextCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SendCommand { get; set; }
        public ICommand LoadFileCommand { get; set; }
        public ICommand CreateFileCommand { get; set; }
        public ICommand DeleteScriptCommand { get; }
        public ICommand LoadDevicesCommand { get; }
        public ICommand CaptureCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand TestCommand { get; }
        public ScriptAutomationViewModel()
        {
            _uiElements = new List<UiElement>();
            AppendTextCommand = new RelayCommandCD(AppendText);
            SaveCommand = new RelayCommandCD(SaveTextToFile);

            SendCommand = new RelayCommandCD(_ =>
            {
                // Gọi ngược lên View để xử lý Insert
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = System.Windows.Application.Current.Windows
                                    .OfType<ScriptAutomation>()
                                    .FirstOrDefault();
                    window?.InsertTextAtCaret(TextSendContext);
                });
            });
            LoadFileCommand = new RelayCommand(async () => await LoadFileScriptFunc());
            CreateFileCommand = new RelayCommand(async () => await CreateFileAsync());
            CaptureCommand = new RelayCommand(async () => await ExecuteCapture());
            LoadDevicesCommand = new RelayCommandCD(_ => LoadDevices());
            ViewCommand = new RelayCommand(async () => await ViewDevice());
            TestCommand = new RelayCommand(async () => await Test());
            DeleteScriptCommand = new RelayCommand(async () =>
            {
                string fileName = SelectedFileScript;
                await DeleteScriptFile(fileName);
            });
            TextBoxContent = string.Empty;
        }
        private async Task Test()
        {
            string deviceID = SelectedDevice;
            string contentTest = TextSendContext;

            if (string.IsNullOrEmpty(deviceID))
            {
                System.Windows.MessageBox.Show("Chọn thiết bị test");
                return;
            }
            if (string.IsNullOrEmpty(contentTest))
            {
                System.Windows.MessageBox.Show("Chọn chức năng test");
                return;
            }
            try
            {
                TitleTest = "Running";
                Script.Roslyn.ScriptAutomation scriptRolyn = new Script.Roslyn.ScriptAutomation();
                await scriptRolyn.TestFunction(contentTest, deviceID);

                await Task.Delay(2000);

                await ExecuteCapture();

                LogText = "";

                TitleTest = "Test";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            LogText = "";
        }
        private async Task ViewDevice()
        {
            if (string.IsNullOrEmpty(SelectedDevice))
            {
                System.Windows.MessageBox.Show("Chọn thiết bị cần view", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Đường dẫn scrcpy.exe từ thư mục Resources
                string scrcpyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "scrcpy.exe");

                if (!File.Exists(scrcpyPath))
                {
                    System.Windows.MessageBox.Show("Không tìm thấy scrcpy.exe trong thư mục Resources.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string deviceId = SelectedDevice;

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = scrcpyPath,
                    Arguments = $"-s {deviceId} --always-on-top --window-x 0 --window-y 30",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                scrcpyProcess = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi khi mở scrcpy: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDevices()
        {
            var devices = ADBService.GetDevices();
           
            AdbDevices.Clear();
            foreach (var device in devices)
            {
                AdbDevices.Add(device);
            }

            if (AdbDevices.Count > 0)
            {
                SelectedDevice = AdbDevices[0];
            }
            else
            {
                System.Windows.MessageBox.Show("Lỗi");
            }
        }
        private void UpdateSendCommand()
        {
            if (IsEditScriptBoxContent)
            {
                SendCommand = new RelayCommandCD(_ =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var window = System.Windows.Application.Current.Windows
                                         .OfType<ScriptAutomation>()
                                         .FirstOrDefault();
                        window?.InsertTextAtCaret(TextSendContext);
                    });
                });
            }
            else
            {
                SendCommand = new RelayCommandCD(SendText);
            }

            OnPropertyChanged(nameof(SendCommand));
        }
        private async Task CreateFileAsync()
        {
            try
            {
                string fileName = "";
                var vm = new InputViewModel();
                var dialog = new InputView
                {
                    Title = "Nhập tên script ",
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
                    fileName = vm.InputText;
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Script");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    string filePath = Path.Combine(folderPath, $"{fileName}.txt");
                    File.WriteAllText(filePath, string.Empty);
                    System.Windows.MessageBox.Show($"Đã tạo script thànnh công ");

                    await LoadFileScriptFunc();

                    SelectedFileScript = fileName + ".txt";
                    TextBoxContent = File.ReadAllText(filePath + "\\" + SelectedFileScript);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task LoadFileScriptFunc(string deletedFileName = null)
        {
            try
            {
                var files = LoadScriptFiles().ToList();

                if (files.Count == 0)
                {
                    System.Windows.MessageBox.Show("Không có script tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadFileScript = Array.Empty<string>();
                        SelectedFileScript = null;
                        TextBoxContent = "";
                    });

                    return;
                }

                // Tìm vị trí của file vừa xóa
                int index = deletedFileName != null
                    ? files.FindIndex(f => string.Equals(f, deletedFileName, StringComparison.OrdinalIgnoreCase))
                    : 0;

                if (index >= 0 && index >= files.Count)
                    index = files.Count - 1;

                if (index == -1) index = 0;

                string fileToLoad = files[index];

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadFileScript = files.ToArray();
                    SelectedFileScript = fileToLoad;

                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Script", fileToLoad);
                    TextBoxContent = File.Exists(filePath) ? File.ReadAllText(filePath) : "";
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi load lại file: {ex.Message}");
            }
        }

        private async Task DeleteScriptFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                System.Windows.MessageBox.Show("Lỗi: Chưa chọn file cần xóa");
                return;
            }

            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Script");
                string filePath = Path.Combine(folderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    System.Windows.MessageBox.Show($"Đã xóa file: {filePath}");

                    await LoadFileScriptFunc(fileName); // Gọi lại, truyền tên file vừa xóa
                }
                else
                {
                    System.Windows.MessageBox.Show($"Không tìm thấy file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi xóa file: {ex.Message}");
            }
        }


        private void AppendText(object parameter)
        {
            string input = parameter?.ToString();
            if (!string.IsNullOrEmpty(input))
            {
                TextSendContext = input;
            }
        }
        private void SendText(object obj)
        {
            TextBoxContent += $"\n{TextSendContext}";
        }
        private void SaveTextToFile(object obj)
        {
            try
            {
                if (SelectedFileScript == null || SelectedFileScript == "")
                {
                    return;
                }
                File.WriteAllText(SaveFilePath + SelectedFileScript, TextBoxContent ?? string.Empty);
                System.Windows.MessageBox.Show("Đã lưu vào script " + SelectedFileScript);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lưu file: {ex.Message}");
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
        private async Task ExecuteCapture()
        {
            string deviceId = SelectedDevice; // bind từ ComboBox nếu cần

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                System.Windows.MessageBox.Show("Vui lòng chọn thiết bị");
                return;
            }

            try
            {
                LogText = "📸 Chụp ảnh...";
                await RunAdbCommandAsync($"-s {deviceId} shell screencap -p /sdcard/screen.png");
                await Task.Delay(1000);
                DeleteOldScreenshot("screen.png");
                await Task.Delay(1000);
                await RunAdbCommandAsync($"-s {deviceId} pull /sdcard/screen.png \"screen.png\"");

                LogText = "📄 Dump UI...";
                await RunAdbCommandAsync($"-s {deviceId} shell uiautomator dump /sdcard/ui_dump.xml");
                await RunAdbCommandAsync($"-s {deviceId} pull /sdcard/ui_dump.xml \"ui_dump.xml\"");

                LoadUiElements("ui_dump.xml");
                await Task.Delay(2000);
                LoadImage("screen.png");

                LogText = "✅ Hoàn tất";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        private async Task RunAdbCommandAsync(string args)
        {
            var psi = new ProcessStartInfo("./Resources/adb", args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            await Task.Run(() => process.WaitForExit());
        }
        private void DeleteOldScreenshot(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    // Đảm bảo không còn ứng dụng nào giữ ảnh
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    File.Delete(path);
                    Debug.WriteLine("🧹 Ảnh cũ đã được xóa: " + path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠ Không thể xóa ảnh cũ: {ex.Message}");
            }
        }

        private void LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.WriteLine("❌ Đường dẫn không hợp lệ: null hoặc rỗng");
                System.Windows.MessageBox.Show("Đường dẫn không hợp lệ.", "Lỗi Load Ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(path))
            {
                Debug.WriteLine($"❌ File không tồn tại: {path}");
                System.Windows.MessageBox.Show($"File không tồn tại: {path}", "Lỗi Load Ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Chuẩn hóa đường dẫn thành URI hợp lệ
                string normalizedPath = Path.GetFullPath(path); // Đảm bảo đường dẫn tuyệt đối
                Uri uri = new Uri($"file:///{normalizedPath.Replace("\\", "/")}"); // Thêm schema file:// và thay \ thành /

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = uri;
                bitmap.EndInit();
                bitmap.Freeze();
                ScreenshotImage = bitmap;

            }
            catch (UriFormatException ex)
            {
                Debug.WriteLine($"❌ Lỗi định dạng URI: {ex.Message} - Đường dẫn: {path}");
                System.Windows.MessageBox.Show($"Lỗi định dạng URI:\n{ex.Message}\nĐường dẫn: {path}", "Lỗi Load Ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine($"❌ Định dạng không hỗ trợ: {ex.Message}");
                System.Windows.MessageBox.Show($"Định dạng không hỗ trợ:\n{ex.Message}", "Lỗi Load Ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Lỗi không xác định khi load ảnh: {ex.Message}\nStackTrace: {ex.StackTrace}");
                System.Windows.MessageBox.Show($"Lỗi không xác định khi load ảnh:\n{ex.Message}\nChi tiết: {ex.StackTrace}", "Lỗi Load Ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
      private void LoadUiElements(string xmlPath)
{
    if (!File.Exists(xmlPath))
    {
        System.Windows.MessageBox.Show($"Không tìm thấy file: {xmlPath}");
        return;
    }

    XmlDocument doc = new();
    doc.Load(xmlPath);

    XmlNodeList nodeList = doc.GetElementsByTagName("node");
    if (nodeList == null || nodeList.Count == 0)
    {
        System.Windows.MessageBox.Show("Không có node nào trong XML.");
        return;
    }

    _uiElements.Clear();
    ElementDetailPairs.Clear();

    foreach (XmlNode node in nodeList)
    {
        // Chỉ xử lý node có class
        if (node.Attributes?["class"] == null)
            continue;

        // Lấy className nếu bạn muốn lọc nâng cao sau này
        string className = node.Attributes["class"].Value;

        // Bỏ qua nếu không có bounds
        if (string.IsNullOrEmpty(node.Attributes["bounds"]?.Value))
            continue;

        var element = new UiElement
        {
            Index = node.Attributes["index"]?.Value,
            Text = node.Attributes["text"]?.Value,
            ResourceId = node.Attributes["resource-id"]?.Value,
            Class = node.Attributes["class"]?.Value,
            Package = node.Attributes["package"]?.Value,
            ContentDesc = node.Attributes["content-desc"]?.Value,
            Checkable = node.Attributes["checkable"]?.Value,
            Checked = node.Attributes["checked"]?.Value,
            Clickable = node.Attributes["clickable"]?.Value,
            Enabled = node.Attributes["enabled"]?.Value,
            Focusable = node.Attributes["focusable"]?.Value,
            Focused = node.Attributes["focused"]?.Value,
            Scrollable = node.Attributes["scrollable"]?.Value,
            LongClickable = node.Attributes["long-clickable"]?.Value,
            Password = node.Attributes["password"]?.Value,
            Selected = node.Attributes["selected"]?.Value,
            Bounds = node.Attributes["bounds"]?.Value
        };

        _uiElements.Add(element);
    }
}

        public UiElement FindElementAt(int x, int y)
        {
            foreach (var element in _uiElements)
            {
                var bounds = element.Bounds.Trim('[', ']').Split(new[] { "][" }, StringSplitOptions.None);
                var topLeft = bounds[0].Split(',');
                var bottomRight = bounds[1].Split(',');

                int left = int.Parse(topLeft[0]);
                int top = int.Parse(topLeft[1]);
                int right = int.Parse(bottomRight[0]);
                int bottom = int.Parse(bottomRight[1]);

                if (x >= left && x <= right && y >= top && y <= bottom)
                {
                    return element;
                }
            }

            return null;
        }
        public void DisplayElementInfo(UiElement element)
        {
            ElementDetailPairs.Clear();

            var bounds = element.Bounds.Trim('[', ']').Split(new[] { "][" }, StringSplitOptions.None);
            var topLeft = bounds[0].Split(',');
            var bottomRight = bounds[1].Split(',');

            int left = int.Parse(topLeft[0]);
            int top = int.Parse(topLeft[1]);
            int right = int.Parse(bottomRight[0]);
            int bottom = int.Parse(bottomRight[1]);

            int centerX = (left + right) / 2;
            int centerY = (top + bottom) / 2;
            var xY = $"[ {centerX} : {centerY} ]";

            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "index", Value = element.Index });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "[X : Y]", Value = xY });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "text", Value = element.Text });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "resource-id", Value = element.ResourceId });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "class", Value = element.Class });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "package", Value = element.Package });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "content-desc", Value = element.ContentDesc });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "checkable", Value = element.Checkable });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "checked", Value = element.Checked });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "clickable", Value = element.Clickable });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "enabled", Value = element.Enabled });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "focusable", Value = element.Focusable });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "focused", Value = element.Focused });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "scrollable", Value = element.Scrollable });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "long-clickable", Value = element.LongClickable });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "password", Value = element.Password });
            ElementDetailPairs.Add(new CustomKeyValuePair { Key = "selected", Value = element.Selected });
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
