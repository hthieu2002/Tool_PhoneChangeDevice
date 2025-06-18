using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for DialogView.xaml
    /// </summary>
    public partial class DialogView : Window
    {
        private InputCoordinateDialogViewModel vmodel;
        public DialogView()
        {
            InitializeComponent();
            vmodel = new InputCoordinateDialogViewModel();
            DataContext = vmodel;
        }
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string currentText = textBox.Text;
            int selectionStart = textBox.SelectionStart;
            string previewText = currentText.Insert(selectionStart, e.Text);

            e.Handled = !IsTextAllowed(previewText);
        }

        private void NumberOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
                string currentText = textBox.Text;
                int selectionStart = textBox.SelectionStart;
                string previewText = currentText.Insert(selectionStart, text);

                if (!IsTextAllowed(previewText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsTextAllowed(string text)
        {
            // Chỉ cho phép số thực, có dấu âm, dấu chấm
            return Regex.IsMatch(text, @"^-?\d*(\.\d*)?$");
        }
    }
}
