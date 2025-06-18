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
        private DeviceViewModel _viewModel;
        public Login()
        {
            InitializeComponent();
            _viewModel = new DeviceViewModel();
            DataContext = _viewModel;
            txtUsername.Text = Properties.Settings.Default.user;
            txtPassword.Password = Properties.Settings.Default.password;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if ((username == "admin@gmail.com" && password == "1234") || (username == "user@gmail.com" && password == "1234"))
            {
                lblError.Visibility = Visibility.Collapsed;
                _viewModel.User = username;
                Home home = new Home();
                home.Show();
                this.Hide();
            }
            else
            {
                lblError.Text = "Invalid username or password!";
                lblError.Visibility = Visibility.Visible;
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
