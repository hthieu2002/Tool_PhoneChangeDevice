using AuthenticationService;
using System.Windows;
using System.Windows.Input;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private bool isLoggingIn = false;
        private readonly CognitoService cognitoService;
        private DeviceViewModel _viewModel;
        public Login()
        {
            InitializeComponent();
            _viewModel = new DeviceViewModel();
            DataContext = _viewModel;
            txtUsername.Text = DeepDroid.Properties.Settings.Default.user;
            txtPassword.Password = DeepDroid.Properties.Settings.Default.password;
            var poolId = AppConfigService.ReadSetting("poolId");
            var clientId = AppConfigService.ReadSetting("clientId");
            cognitoService = new CognitoService(poolId, clientId);

            Loaded += Login_Loaded;
        }
        private async void Login_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUsername.Text) &&
                !string.IsNullOrWhiteSpace(txtPassword.Password))
            {

                if (isLoggingIn)
                    return;

                isLoggingIn = true;
                btnLogin.Cursor = System.Windows.Input.Cursors.Wait;
                try
                {
                    await login(); // Gọi hàm login async
                }
                finally
                {
                    isLoggingIn = false;
                    btnLogin.Cursor = System.Windows.Input.Cursors.Hand;
                }
            }
        }


        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (isLoggingIn)
                return;

            isLoggingIn = true;
            btnLogin.Cursor = System.Windows.Input.Cursors.Wait; // Thay đổi con trỏ chuột thành đợi
            try
            {
                await login(); // Gọi hàm login async
            }
            finally
            {
                isLoggingIn = false;
                btnLogin.Cursor = System.Windows.Input.Cursors.Hand;
            }
        }

        private async Task login()
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

                //Chạy getIdToken ở background thread và cho timeout 6 giây
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
                token = await Task.Run(() =>
                {
                    return cognitoService.getIdToken(username, password);
                }, cts.Token);

                if (!string.IsNullOrEmpty(token))
                {
                    DeepDroid.Properties.Settings.Default.user = username;
                    DeepDroid.Properties.Settings.Default.password = password;
                    DeepDroid.Properties.Settings.Default.Save();
                    var home = new Home();
                    home.Show();
                    //this.Close();

                    System.Windows.Application.Current.MainWindow.Close();
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
