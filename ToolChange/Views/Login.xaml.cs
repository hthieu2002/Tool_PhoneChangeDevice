using AuthenticationService;
using Microsoft.Win32;
using POCO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly CognitoService cognitoService;
        private DeviceViewModel _viewModel;
        public Login()
        {
            InitializeComponent();
            _viewModel = new DeviceViewModel();
            DataContext = _viewModel;
            txtUsername.Text = Properties.Settings.Default.user;
            txtPassword.Password = Properties.Settings.Default.password;
            var poolId = AppConfigService.ReadSetting("poolId");
            var clientId = AppConfigService.ReadSetting("clientId");
            cognitoService = new CognitoService(poolId, clientId);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false; // Vô hiệu hóa nút
            try
            {
                login(); // Gọi hàm login async
            }
            finally
            {
                btnLogin.IsEnabled = true; // Bật lại dù login thành công hay lỗi
            }
        }
       
        private async void login()
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed;

                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
                    throw new Exception("Blank field(s)!");

                // Validate email format
                var eMailValidator = new System.Net.Mail.MailAddress(txtUsername.Text);

                string username = txtUsername.Text;
                string password = txtPassword.Password;
                string token = "";

                // ✅ Chạy getIdToken ở background thread và cho timeout 6 giây
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
                token = await Task.Run(() =>
                {
                    return cognitoService.getIdToken(username, password);
                }, cts.Token);

                if (!string.IsNullOrEmpty(token))
                {
                    Properties.Settings.Default.user = username;
                    Properties.Settings.Default.password = password;
                    var home = new Home();
                    home.Show();
                    this.Close();
                }
                else
                {
                    lblError.Text = "Invalid username or password!";
                    lblError.Visibility = Visibility.Visible;
                }
            }
            catch (OperationCanceledException)
            {
                System.Windows.MessageBox.Show("⏰ Request Timeout", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FormatException)
            {
                lblError.Text = "Invalid email format!";
                lblError.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, new RoutedEventArgs()); 
                e.Handled = true; 
            }
        }
    }
}
