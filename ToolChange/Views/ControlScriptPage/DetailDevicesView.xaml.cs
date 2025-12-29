using System.Windows;
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for DetailDevicesView.xaml
    /// </summary>
    public partial class DetailDevicesView : Window
    {
        private DetailDeviceViewModel viewModel;
        public DetailDevicesView()
        {
            InitializeComponent();
            viewModel = new DetailDeviceViewModel();
            DataContext = viewModel;
        }
    }
}
