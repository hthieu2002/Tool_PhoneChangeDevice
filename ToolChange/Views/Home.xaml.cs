using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        private Dictionary<string, object> viewCache = new Dictionary<string, object>();
        public MainViewModel ViewModel { get; set; }
        public Home()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            BtnDevice.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            HomeFrame.Content = new Device();
        }

        private void BtnDevice_Click(object sender, RoutedEventArgs e)
        {
            BtnDevice.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            string viewKey = "Device";
            if (!viewCache.ContainsKey(viewKey))
            {
                viewCache[viewKey] = new Device();
            }
            HomeFrame.Content = viewCache[viewKey];
        }

        private void BtnAutomation_Click(object sender, RoutedEventArgs e)
        {
            BtnAutomation.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            string viewKey = "Automation";
            if (!viewCache.ContainsKey(viewKey))
            {
                viewCache[viewKey] = new Automation();
            }
            HomeFrame.Content = viewCache[viewKey];
        }

        private void BtnScreen_Click(object sender, RoutedEventArgs e)
        {
            BtnScreen.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            HomeFrame.Content = new ViewDevices();

        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            BtnSetting.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDocument.Style = (Style)FindResource("MaterialDesignFlatDarkButton");

            Setiing setting= new Setiing();
            setting.ShowDialog();
        }

        private void BtnDocument_Click(object sender, RoutedEventArgs e)
        {
            BtnDocument.Style = (Style)FindResource("MaterialDesignRaisedLightButton");
            BtnAutomation.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnDevice.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnSetting.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
            BtnScreen.Style = (Style)FindResource("MaterialDesignFlatDarkButton");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
