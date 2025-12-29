using System.Windows.Controls;
using ToolChange.Services;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for ClickPage.xaml
    /// </summary>
    public partial class ClickPage : Page
    {
        public ClickPage()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ScriptVM;
        }
    }
}
