using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolChange.Services;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Automation.xaml
    /// </summary>
    public partial class Automation : Page
    {
        private int currentNumber = 0;
        private const int MAX_VALUE = 99;
        private const int MIN_VALUE = 0;
        public Automation()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.AutomationVM;
            this.Unloaded += AutomationPage_Unloaded;
            this.IsVisibleChanged += AutomationPage_IsVisibleChanged;
        }
        private void AutomationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AutomationViewModel.StopLoop();
        }
        private void backupComboBox_DropDownOpened(object sender, EventArgs e)
        {
            string backupFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Backup");

            if (Directory.Exists(backupFolder))
            {
                var files = Directory.GetFiles(backupFolder, "*.zip")
                                     .Select(f => new FileInfo(f))
                                     .OrderByDescending(f => f.LastWriteTime) // sắp xếp theo thời gian sửa đổi gần nhất
                                     .Select(f => f.Name)
                                     .ToList();

                backupComboBox.ItemsSource = files;
            }
            else
            {
                backupComboBox.ItemsSource = new List<string> { "Không có thư mục Backup" };
            }
        }


        private void AutomationPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!Equals(e.OldValue, true) && Equals(e.NewValue, true))
            {
                DeviceViewModel.StopLoop();
                Task.Delay(2000);
                ViewModelLocator.AutomationVM.AutomationListVM.AsyncTask();
            }
        }
        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (currentNumber < MAX_VALUE)
            {
                currentNumber++;
                NumberTextBox.Text = currentNumber.ToString();

                dynamic automationVM = DataContext;
                var vm = automationVM?.AutomationListVM;


                if (vm != null)
                {
                    vm.CountRunScript = int.Parse(NumberTextBox.Text);
                }
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (currentNumber > MIN_VALUE)
            {
                currentNumber--;
                NumberTextBox.Text = currentNumber.ToString();

                dynamic automationVM = DataContext;
                var vm = automationVM?.AutomationListVM;


                if (vm != null)
                {
                    vm.CountRunScript = int.Parse(NumberTextBox.Text);
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void codeScript_Click(object sender, RoutedEventArgs e)
        {
            ScriptAutomation sc = new ScriptAutomation();
            sc.Show();
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            Setiing setting = new Setiing();
            setting.ShowDialog();
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            DeepDroid.Properties.Settings.Default.user = string.Empty;
            DeepDroid.Properties.Settings.Default.password = string.Empty;
            DeepDroid.Properties.Settings.Default.token = string.Empty;
            DeepDroid.Properties.Settings.Default.Save();

            var app = System.Windows.Application.Current;

            var login = new Login();
            app.MainWindow = login;

            if (Window.GetWindow(this) is Home home)
            {
                home.SuppressClosePrompt = true;
                home.Close();
            }
            foreach (Window w in app.Windows)
                if (!ReferenceEquals(w, login)) w.Close();

            login.Show();
        }
    }
}
