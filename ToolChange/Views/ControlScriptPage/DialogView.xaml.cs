using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
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
