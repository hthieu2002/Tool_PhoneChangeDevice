using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using ToolChange.Services;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for KeyButtonPage.xaml
    /// </summary>
    public partial class KeyButtonPage : Page
    {
        public KeyButtonPage()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ScriptVM;
        }
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                string url = "https://gist.github.com/arjunv/2bbcca9a1a1c127749f8dcb6d36fb0bc";
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Không thể mở URL: {ex.Message}");
                }
            }
        }
    }
}
