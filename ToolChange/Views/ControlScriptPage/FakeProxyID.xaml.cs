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

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for FakeProxyID.xaml
    /// </summary>
    public partial class FakeProxyID : Window
    {
        private FakeProxyIDViewModel _viewModel;

        public FakeProxyID(string _device, string _deviceName, string _proxy)
        {
            InitializeComponent();
            _viewModel = new FakeProxyIDViewModel();
            DataContext = _viewModel;

            device.Text = $"Thiết bị {_deviceName} - {_device}";
            proxy.Text = $"Proxy: {_proxy}";
            BtnFake.Text = $"Fake {_proxy}";
        }

        private void HostTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (sender is System.Windows.Controls.TextBox tb)
                {
                    var pastedText = System.Windows.Clipboard.GetText();
                    if (!string.IsNullOrEmpty(pastedText) && pastedText.Contains(":"))
                    {
                        var parts = pastedText.Split(':');
                        tb.Text = parts[0]; 

                        if (parts.Length > 1 && FindName("PortTextBox") is System.Windows.Controls.TextBox portBox)
                            portBox.Text = parts[1];
                        if (parts.Length > 2 && FindName("UsernameTextBox") is System.Windows.Controls.TextBox userBox)
                            userBox.Text = parts[2];
                        if (parts.Length > 3 && FindName("PasswordTextBox") is System.Windows.Controls.TextBox passBox)
                            passBox.Text = parts[3];

                        e.Handled = true; 
                    }
                    
                }
            }
        }

    }
}
