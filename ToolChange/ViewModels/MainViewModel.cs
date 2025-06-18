using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolChange.Services;

namespace ToolChange.ViewModels
{

    public class MainViewModel
    {
        public LocalizationViewModel LanguageVM => ViewModelLocator.Localization;
        public DeviceViewModel DeviceListVM => ViewModelLocator.DeviceListVM;
        public SettingViewModel SettingVM => ViewModelLocator.SettingVM;
        public AutomationViewModel AutomationVM => ViewModelLocator.AutomationListVM;
    }
}
