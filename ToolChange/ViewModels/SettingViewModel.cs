using System.Collections.ObjectModel;
using System.ComponentModel;
using ToolChange.Services;
using ToolChange.Views;

namespace ToolChange.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private LanguageItem _selectedLanguage;
        public LanguageItem SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {

                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged(nameof(SelectedLanguage));
                    HandleLanguageChanged(value);
                }
            }
        }

        public ObservableCollection<LanguageItem> Languages { get; }

        public SettingViewModel()
        {
            Languages = new ObservableCollection<LanguageItem>()
            {
                new LanguageItem { DisplayName = "Vietnamese", Value = "vi" },
                new LanguageItem { DisplayName = "English", Value = "en" }
            };
            var savedLang = DeepDroid.Properties.Settings.Default.lang ?? "en";
            SelectedLanguage = Languages.FirstOrDefault(l => l.Value == savedLang) ?? Languages[0];
        }
        private void HandleLanguageChanged(LanguageItem language)
        {
            if (language != null)
            {
                DeepDroid.Properties.Settings.Default.lang = language.Value;
                DeepDroid.Properties.Settings.Default.Save();
                ViewModelLocator.Localization.Refresh();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
