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
