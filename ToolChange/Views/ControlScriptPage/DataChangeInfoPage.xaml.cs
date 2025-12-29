using System.Windows.Controls;
using ToolChange.Services;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for DataChangeInfoPage.xaml
    /// </summary>
    public partial class DataChangeInfoPage : Page
    {
        public DataChangeInfoPage()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ScriptVM;
        }
    }
}
