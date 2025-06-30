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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolChange.Services;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Automation.xaml
    /// </summary>
    public partial class Automation : Page
    {
        private int currentNumber = 0;
        private const int MAX_VALUE = 99;
        private const int MIN_VALUE = 0;
        public Automation()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.AutomationVM;
            this.Unloaded += AutomationPage_Unloaded;
            this.IsVisibleChanged += AutomationPage_IsVisibleChanged;
        }
        private void AutomationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AutomationViewModel.StopLoop();
        }
        private void AutomationPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!Equals(e.OldValue, true) && Equals(e.NewValue, true))
            {
                DeviceViewModel.StopLoop(); 
                Task.Delay(2000);
                ViewModelLocator.AutomationVM.AutomationListVM.AsyncTask(); 
            }

        }
        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (currentNumber < MAX_VALUE)
            {
                currentNumber++;
                NumberTextBox.Text = currentNumber.ToString();

                dynamic automationVM = DataContext;
                var vm = automationVM?.AutomationListVM;


                if (vm != null)
                {
                    vm.CountRunScript = int.Parse(NumberTextBox.Text);
                }
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (currentNumber > MIN_VALUE)
            {
                currentNumber--;
                NumberTextBox.Text = currentNumber.ToString();

                dynamic automationVM = DataContext;
                var vm = automationVM?.AutomationListVM;


                if (vm != null)
                {
                    vm.CountRunScript = int.Parse(NumberTextBox.Text);
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void codeScript_Click(object sender, RoutedEventArgs e)
        {
            ScriptAutomation sc = new ScriptAutomation();
            sc.Show();
        }
    }
}
