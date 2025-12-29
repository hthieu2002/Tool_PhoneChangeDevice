using System.Windows;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Setiing.xaml
    /// </summary>
    public partial class Setiing : Window
    {
        SettingViewModel settingViewModel;
        public Setiing()
        {
            InitializeComponent();
            settingViewModel = new SettingViewModel();
            DataContext = settingViewModel;
        }
    }
}
