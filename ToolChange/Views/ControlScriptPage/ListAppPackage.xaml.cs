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
