using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolChange.ViewModels;

namespace ToolChange.Services
{
    public static class ViewModelLocator
    {
        public static LocalizationViewModel Localization { get; } = new LocalizationViewModel();
        public static DeviceViewModel DeviceListVM { get; } = new DeviceViewModel();
        public static SettingViewModel SettingVM { get; } = new SettingViewModel();
        public static MainViewModel MainViewModel { get; } = new MainViewModel();
        public static AutomationViewModel AutomationListVM { get; } = new AutomationViewModel();
        public static ScriptAutomationViewModel ScriptAutomationVM { get; } = new ScriptAutomationViewModel();
        public static viewDevicesViewModel ViewDeviceVM { get; } = new viewDevicesViewModel();

        public static DeviceViewModel DeviceVM => new DeviceViewModel
        {
            LanguageVM = Localization,
            DeviceListVM = DeviceListVM
        };
        public static AutomationViewModel AutomationVM = new AutomationViewModel
        {
            LanguageVM = Localization,
            AutomationListVM = AutomationListVM
        };
        public static ScriptAutomationViewModel ScriptVM = new ScriptAutomationViewModel
        {
            LanguageVM = Localization,
            ScriptAutomationVM = ScriptAutomationVM
        };
        public static viewDevicesViewModel ViewDevicesViewModel => new viewDevicesViewModel
        {
            LanguageVM = Localization
        };
    }

}
