using System.Windows;
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for InputView.xaml
    /// </summary>
    public partial class InputView : Window
    {
        private InputViewModel model;
        public InputView()
        {
            InitializeComponent();
            model = new InputViewModel();
            this.DataContext = model;
        }
    }
}
