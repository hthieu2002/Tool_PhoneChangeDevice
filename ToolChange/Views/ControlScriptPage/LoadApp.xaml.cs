using Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ToolChange.Views.ControlScriptPage
{
    public class AppInfo
    {
        public int No { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Interaction logic for LoadApp.xaml
    /// </summary>
    public partial class LoadApp : Window
    {
        public LoadApp()
        {
            InitializeComponent();
        }

        private void BtnLoadDevice_Click(object sender, RoutedEventArgs e)
        {
            var devices = ADBService.GetDevices();
            ComboBoxDevices.ItemsSource = devices;
            ComboBoxDevices.SelectedIndex = 0;

        }

        private async void BtnLoadAllApp_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxDevices.SelectedItem is string deviceId)
            {
                var apps = await GetInstalledPackagesAsync(deviceId);
                DataGridApps.ItemsSource = apps.Select(app => new { PackageName = app }).ToList();
            }
        }

        private async void BtnLoadAppInstaller_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ComboBoxDevices.Text) || ComboBoxDevices.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn thiết bị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string deviceId = ComboBoxDevices.SelectedItem.ToString();

            var getPackagesUserApps = ADBService.ExecuteADBCommandDetail(deviceId, "shell pm list packages -3");

            if (!string.IsNullOrEmpty(getPackagesUserApps))
            {
                var listPkg = getPackagesUserApps
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !line.StartsWith("List") && !line.StartsWith("---------"))
                    .Select(line => line.Split('\t')[0].Replace("package:", "").Trim())
                    .OrderBy(x => x.ToLower())
                    .ToList();

                var appList = new List<AppInfo>();
                int index = 1;
                foreach (var pkg in listPkg)
                {
                    appList.Add(new AppInfo { No = index++, Name = pkg });
                }

                DataGridApps.ItemsSource = appList;
            }
            else
            {
                DataGridApps.ItemsSource = null;
            }
        }
        private void DataGridApps_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CopySelectedAppNames();
            }
        }
        private void CopySelectedAppNames()
        {
            if (DataGridApps.SelectedItems == null || DataGridApps.SelectedItems.Count == 0)
                return;

            int columnCount = DataGridApps.Columns.Count;

            if (columnCount == 1)
            {
                var lines = DataGridApps.SelectedItems
                    .Cast<object>()
                    .Select(item => item?.ToString()); 
                System.Windows.Clipboard.SetText(string.Join(Environment.NewLine, lines));
            }
            else
            {
                var selectedRows = DataGridApps.SelectedItems.Cast<AppInfo>();
                var lines = selectedRows.Select(row => row?.Name);
                System.Windows.Clipboard.SetText(string.Join(Environment.NewLine, lines));
            }
        }

        private async Task<List<string>> GetInstalledPackagesAsync(string deviceId)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "./Resources/adb",
                    Arguments = $"-s {deviceId} shell pm list packages",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return output.Split('\n')
                         .Where(x => x.StartsWith("package:"))
                         .Select(x => x.Replace("package:", "").Trim())
                         .ToList();
        }
    }
}
