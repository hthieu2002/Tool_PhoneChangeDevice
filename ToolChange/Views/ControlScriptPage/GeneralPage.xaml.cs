using System.Windows;
using System.Windows.Controls;
using ToolChange.Services;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for GeneralPage.xaml
    /// </summary>
    public partial class GeneralPage : Page
    {
        public GeneralPage()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ScriptVM;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadApp load = new LoadApp();
            load.Show();
        }
    }
}
