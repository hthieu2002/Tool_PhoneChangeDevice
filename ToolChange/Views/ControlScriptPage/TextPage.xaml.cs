using System.Windows.Controls;
using ToolChange.Services;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for TextPage.xaml
    /// </summary>
    public partial class TextPage : Page
    {
        public TextPage()
        {
            InitializeComponent();

            DataContext = ViewModelLocator.ScriptVM;
        }


    }
}
