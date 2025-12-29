using System.Windows;
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for ListAppPackage.xaml
    /// </summary>
    public partial class ListAppPackage : Window
    {
        private AppListViewModel vm;
        public ListAppPackage(Models.DeviceModel devices)
        {
            InitializeComponent();
            vm = new AppListViewModel(devices);
            this.DataContext = vm;
        }
    }
}
