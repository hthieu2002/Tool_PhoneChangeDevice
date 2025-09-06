using System;
using System.Configuration;

namespace ToolChange.ViewModels
{
    public class AppConfigService
    {
        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
        public static string ReadSetting(string key)
        {
            try
            {
                var property = typeof(DeepDroid.Properties.Settings).GetProperty(key);
                if (property != null)
                {
                    var value = property.GetValue(DeepDroid.Properties.Settings.Default)?.ToString();
                    return value ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading app settings: " + ex.Message);
                return string.Empty;
            }
        }

    }
}
