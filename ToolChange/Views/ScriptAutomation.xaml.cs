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
using ToolChange.Services;
using ToolChange.ViewModels;
using ToolChange.Views.ControlScriptPage;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for ScriptAutomation.xaml
    /// </summary>
    public partial class ScriptAutomation : Window
    {
        private string _initialText = "";
        private int _selectedLine = -1;
        public ScriptAutomation()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ScriptVM;
            txbClick.FontWeight = FontWeights.Bold;
            txbClick.TextDecorations = TextDecorations.Underline;
            // null
            txbText.FontWeight = FontWeights.Normal;
            txbText.TextDecorations = null;
            txbKeyButton.FontWeight = FontWeights.Normal;
            txbKeyButton.TextDecorations = null;
            txbDataChangeInfo.FontWeight = FontWeights.Normal;
            txbDataChangeInfo.TextDecorations = null;
            txbGeneral.FontWeight = FontWeights.Normal;
            txbGeneral.TextDecorations = null;
            ScriptFrame.Navigate(new ClickPage());

            var saveGesture = new KeyGesture(Key.S, ModifierKeys.Control);
            var saveCommandBinding = new CommandBinding(ApplicationCommands.Save,
                (s, e) =>
                {
                    ViewModelLocator.ScriptAutomationVM.SaveCommand.Execute(null);
                });

            this.CommandBindings.Add(saveCommandBinding);
            this.InputBindings.Add(new InputBinding(ApplicationCommands.Save, saveGesture));

            UpdateLineNumbers();
        }
        private void EditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            if (_initialText == "")
            {
                _initialText = textBox.Text; // Lưu nội dung ban đầu
            }

            if (textBox.Text != _initialText)
            {
                editIndicator.Text = "*";
                editIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                editIndicator.Text = "";
                editIndicator.Visibility = Visibility.Collapsed;
            }

            UpdateLineNumbers();
        }

        private void EditTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                UpdateLineNumbers();
            }
        }

        private void TextScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double verticalOffset = e.VerticalOffset;
            double maxHeight = textScrollViewer.ScrollableHeight;
            if (maxHeight > 0)
            {
                double proportion = verticalOffset / maxHeight;
                int itemIndex = (int)(proportion * (lineNumbers.Items.Count - 1));
                if (itemIndex >= 0 && itemIndex < lineNumbers.Items.Count)
                {
                    lineNumbers.ScrollIntoView(lineNumbers.Items[itemIndex]);
                }
            }
        }

        private void EditTextBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            int lineIndex = textBox.GetLineIndexFromCharacterIndex(textBox.CaretIndex);
            if (lineIndex != _selectedLine && lineIndex >= 0)
            {
                _selectedLine = lineIndex;
                UpdateLineNumbers();
                lineNumbers.SelectedIndex = _selectedLine;
            }
        }
        public void InsertTextAtCaret(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            string formatted = "\n" + text;
            int caretIndex = editTextBox.CaretIndex;

            editTextBox.Text = editTextBox.Text.Insert(caretIndex, formatted);
            editTextBox.CaretIndex = caretIndex + formatted.Length;

            ViewModelLocator.ScriptVM.TextBoxContent = editTextBox.Text;
            
        }
        private void UpdateLineNumbers()
        {
            lineNumbers.Items.Clear();
            string[] lines = editTextBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.None); 
            for (int i = 0; i < lines.Length; i++)
            {
                if (i < editTextBox.LineCount) 
                {
                    string number = (i + 1).ToString();
                    if (i == _selectedLine && _selectedLine >= 0 && _selectedLine < lines.Length)
                    {
                        number = "*" + number;
                    }
                    lineNumbers.Items.Add(number);
                }
            }
            if (lineNumbers.Items.Count > 0 && _selectedLine >= 0 && _selectedLine < lineNumbers.Items.Count)
            {
                lineNumbers.SelectedIndex = _selectedLine;
            }
        }
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                string tag = textBlock.Tag.ToString();
                switch (tag)
                {
                    case "Click":
                        txbClick.FontWeight = FontWeights.Bold;
                        txbClick.TextDecorations = TextDecorations.Underline;
                        // null
                        txbText.FontWeight = FontWeights.Normal;
                        txbText.TextDecorations = null;
                        txbKeyButton.FontWeight = FontWeights.Normal;
                        txbKeyButton.TextDecorations = null;
                        txbDataChangeInfo.FontWeight = FontWeights.Normal;
                        txbDataChangeInfo.TextDecorations = null;
                        txbGeneral.FontWeight = FontWeights.Normal;
                        txbGeneral.TextDecorations = null;
                        ScriptFrame.Navigate(new ClickPage());
                        break;
                    case "Text":
                        txbText.FontWeight = FontWeights.Bold;
                        txbText.TextDecorations = TextDecorations.Underline;
                        // null
                        txbClick.FontWeight = FontWeights.Normal;
                        txbClick.TextDecorations = null;
                        txbKeyButton.FontWeight = FontWeights.Normal;
                        txbKeyButton.TextDecorations = null;
                        txbDataChangeInfo.FontWeight = FontWeights.Normal;
                        txbDataChangeInfo.TextDecorations = null;
                        txbGeneral.FontWeight = FontWeights.Normal;
                        txbGeneral.TextDecorations = null;
                        ScriptFrame.Navigate(new TextPage());
                        // ContentFrame.Navigate(new TextPage());
                        break;
                    case "KeyButton":
                        txbKeyButton.FontWeight = FontWeights.Bold;
                        txbKeyButton.TextDecorations = TextDecorations.Underline;
                        // null
                        txbClick.FontWeight = FontWeights.Normal;
                        txbClick.TextDecorations = null;
                        txbText.FontWeight = FontWeights.Normal;
                        txbText.TextDecorations = null;
                        txbDataChangeInfo.FontWeight = FontWeights.Normal;
                        txbDataChangeInfo.TextDecorations = null;
                        txbGeneral.FontWeight = FontWeights.Normal;
                        txbGeneral.TextDecorations = null;
                        ScriptFrame.Navigate(new KeyButtonPage());
                        //  ContentFrame.Navigate(new KeyButtonPage());
                        break;
                    case "DataChange":
                        txbDataChangeInfo.FontWeight = FontWeights.Bold;
                        txbDataChangeInfo.TextDecorations = TextDecorations.Underline;
                        //
                        txbClick.FontWeight = FontWeights.Normal;
                        txbClick.TextDecorations = null;
                        txbText.FontWeight = FontWeights.Normal;
                        txbText.TextDecorations = null;
                        txbKeyButton.FontWeight = FontWeights.Normal;
                        txbKeyButton.TextDecorations = null;
                        txbGeneral.FontWeight = FontWeights.Normal;
                        txbGeneral.TextDecorations = null;
                        ScriptFrame.Navigate(new DataChangeInfoPage());
                        //ContentFrame.Navigate(new DataChangePage());
                        break;
                    case "General":
                        txbGeneral.FontWeight = FontWeights.Bold;
                        txbGeneral.TextDecorations = TextDecorations.Underline;
                        //
                        txbClick.FontWeight = FontWeights.Normal;
                        txbClick.TextDecorations = null;
                        txbText.FontWeight = FontWeights.Normal;
                        txbText.TextDecorations = null;
                        txbKeyButton.FontWeight = FontWeights.Normal;
                        txbKeyButton.TextDecorations = null;
                        txbDataChangeInfo.FontWeight = FontWeights.Normal;
                        txbDataChangeInfo.TextDecorations = null;
                        ScriptFrame.Navigate(new GeneralPage());
                        //ContentFrame.Navigate(new GeneralPage());
                        break;
                }
            }
        }
    }
}
