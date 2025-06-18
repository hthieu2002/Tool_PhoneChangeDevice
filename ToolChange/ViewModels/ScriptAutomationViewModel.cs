using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using ToolChange.Services;
using ToolChange.Views;

namespace ToolChange.ViewModels
{
    public class ScriptAutomationViewModel : INotifyPropertyChanged
    {
        public LocalizationViewModel LanguageVM { get; set; } // language
        public ScriptAutomationViewModel ScriptAutomationVM { get; set; } // language

        private string _textBoxContent;
        private string _textSendContext;
        private bool _isEditScriptBoxContent = true;
        private string _saveFilePath = @"D:\my_script.txt"; // Đường dẫn file txt cố định

        public string TextBoxContent
        {
            get => _textBoxContent;
            set
            {
                _textBoxContent = value;
                OnPropertyChanged(nameof(TextBoxContent));
            }
        }
        public string TextSendContext
        {
            get => _textSendContext;
            set
            {
                _textSendContext = value;
                OnPropertyChanged(nameof(TextSendContext));
            }
        }
        public bool IsEditScriptBoxContent
        {
            get => _isEditScriptBoxContent;
            set
            {
                _isEditScriptBoxContent = value;
                OnPropertyChanged(nameof(IsEditScriptBoxContent));
                UpdateSendCommand();
            }
        }
        
        public string SaveFilePath
        {
            get => _saveFilePath;
            set
            {
                _saveFilePath = value;
                OnPropertyChanged(nameof(SaveFilePath));
            }
        }

        public ICommand AppendTextCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SendCommand { get; set; }

        public ScriptAutomationViewModel()
        {
            AppendTextCommand = new RelayCommandCD(AppendText);
            SaveCommand = new RelayCommandCD(SaveTextToFile);
           
            SendCommand = new RelayCommandCD(_ =>
            {
                // Gọi ngược lên View để xử lý Insert
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = System.Windows.Application.Current.Windows
                                    .OfType<ScriptAutomation>()
                                    .FirstOrDefault();
                    window?.InsertTextAtCaret(TextSendContext);
                });
            });
            TextBoxContent = string.Empty;
        }
        private void UpdateSendCommand()
        {
            if (IsEditScriptBoxContent)
            {
                SendCommand = new RelayCommandCD(_ =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var window = System.Windows.Application.Current.Windows
                                         .OfType<ScriptAutomation>()
                                         .FirstOrDefault();
                        window?.InsertTextAtCaret(TextSendContext);
                    });
                });
            }
            else
            {
                SendCommand = new RelayCommandCD(SendText);
            }

            OnPropertyChanged(nameof(SendCommand)); 
        }

        private void AppendText(object parameter)
        {
            string input = parameter?.ToString();
            if (!string.IsNullOrEmpty(input))
            {
                TextSendContext = input;
            }
        }
        private void SendText(object obj)
        {
            TextBoxContent += $"\n{TextSendContext}";
        }
        private void SaveTextToFile(object obj)
        {
            try
            {
                File.WriteAllText(SaveFilePath, TextBoxContent ?? string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu file: {ex.Message}");
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
