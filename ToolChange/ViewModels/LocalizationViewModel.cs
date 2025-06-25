using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class LocalizationViewModel : INotifyPropertyChanged
    {
        private string _device;
        private string _automation;
        private string _screen;
        private string _checkBoxAutomation;
        private string _controlClickXY;
        private string _controlSwipe;
        private string _controlRandomClick;
        private string _controlWait;
        private string _titleClickControl;
        private string _titleClick2Control;
        private string _controlSearchAndClick;
        private string _controlSearchWaitClick;
        private string _controlSearchAndContinue;
        private string _titleClick3Control;
        private string _controlFindAndClick;
        private string _controlFindWaitClick;
        private string _controlFindAndContinue;
        private string _titleClick4Control;
        private string _titleTextControl;
        private string _controlSendText;
        private string _controlSendTextFromFileDel;
        private string _controlSendTextRandomFromFile;
        private string _controlRandomTextAndSend;
        private string _controlDelTextChar;
        private string _controlDelAllText;
        private string _controlSendKey;
        private string _controlCtrlA;
        private string _controlCheckKeyBoard;
        private string _controlURLKEYSendText;
        private string _titileDataChangeInfo;
        private string _controlDataChangeInfo1;
        private string _controlDataChangeInfo2;
        private string _controlDataChangeInfo3;
        private string _controlDataChangeInfo4;
        private string _controlDataChangeInfo5;
        private string _controlDataChangeInfo6;
        private string _controlDataChangeInfo7;
        private string _controlWaitReboot;
        private string _controlWaitInternet;
        private string _controlPushFile;
        private string _controlPullFile;
        private string _titleGeneral;
        private string _controlWiFiON;
        private string _controlWiFiOFF;
        private string _controlOpenURL;
        private string _controlRunCommandShell;
        private string _controlOpenApp;
        private string _controlCloseApp;
        private string _controlEnableApp;
        private string _controlDisbledApp;
        private string _controlInstallApp;
        private string _controlUninstallApp;
        private string _controlClearDataApp;
        private string _controlSwipeCloseApp;
        private string _controlLoadApp;
        private string _controlDeviceMenuDropBox1;
        private string _controlDeviceMenuDropBox2;
        private string _controlDeviceMenuDropBox3;
        private string _controlDeviceMenuDropBox4;
        private string _controlDeviceMenuDropBox5;
        private string _lableViewDevice1;
        private string _lableViewDevice2;
        private string _controlCheckScreen;
        private string _labelNumberDevice;
        private string _setting;
        private string _document;

        public string Device
        {
            get => _device;
            private set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged(nameof(Device));
                }
            }
        }

        public string Automation
        {
            get => _automation;
            private set
            {
                if (_automation != value)
                {
                    _automation = value;
                    OnPropertyChanged(nameof(Automation));
                }
            }
        }

        public string Screen
        {
            get => _screen;
            private set
            {
                if (_screen != value)
                {
                    _screen = value;
                    OnPropertyChanged(nameof(Screen));
                }
            }
        }

        public string CheckBoxAutomation
        {
            get => _checkBoxAutomation;
            private set
            {
                if (_checkBoxAutomation != value)
                {
                    _checkBoxAutomation = value;
                    OnPropertyChanged(nameof(CheckBoxAutomation));
                }
            }
        }

        public string ControlClickXY
        {
            get => _controlClickXY;
            private set
            {
                if (_controlClickXY != value)
                {
                    _controlClickXY = value;
                    OnPropertyChanged(nameof(ControlClickXY));
                }
            }
        }

        public string ControlSwipe
        {
            get => _controlSwipe;
            private set
            {
                if (_controlSwipe != value)
                {
                    _controlSwipe = value;
                    OnPropertyChanged(nameof(ControlSwipe));
                }
            }
        }

        public string ControlRandomClick
        {
            get => _controlRandomClick;
            private set
            {
                if (_controlRandomClick != value)
                {
                    _controlRandomClick = value;
                    OnPropertyChanged(nameof(ControlRandomClick));
                }
            }
        }

        public string ControlWait
        {
            get => _controlWait;
            private set
            {
                if (_controlWait != value)
                {
                    _controlWait = value;
                    OnPropertyChanged(nameof(ControlWait));
                }
            }
        }

        public string TitleClickControl
        {
            get => _titleClickControl;
            private set
            {
                if (_titleClickControl != value)
                {
                    _titleClickControl = value;
                    OnPropertyChanged(nameof(TitleClickControl));
                }
            }
        }

        public string TitleClick2Control
        {
            get => _titleClick2Control;
            private set
            {
                if (_titleClick2Control != value)
                {
                    _titleClick2Control = value;
                    OnPropertyChanged(nameof(TitleClick2Control));
                }
            }
        }

        public string ControlSearchAndClick
        {
            get => _controlSearchAndClick;
            private set
            {
                if (_controlSearchAndClick != value)
                {
                    _controlSearchAndClick = value;
                    OnPropertyChanged(nameof(ControlSearchAndClick));
                }
            }
        }

        public string ControlSearchWaitClick
        {
            get => _controlSearchWaitClick;
            private set
            {
                if (_controlSearchWaitClick != value)
                {
                    _controlSearchWaitClick = value;
                    OnPropertyChanged(nameof(ControlSearchWaitClick));
                }
            }
        }

        public string ControlSearchAndContinue
        {
            get => _controlSearchAndContinue;
            private set
            {
                if (_controlSearchAndContinue != value)
                {
                    _controlSearchAndContinue = value;
                    OnPropertyChanged(nameof(ControlSearchAndContinue));
                }
            }
        }

        public string TitleClick3Control
        {
            get => _titleClick3Control;
            private set
            {
                if (_titleClick3Control != value)
                {
                    _titleClick3Control = value;
                    OnPropertyChanged(nameof(TitleClick3Control));
                }
            }
        }

        public string ControlFindAndClick
        {
            get => _controlFindAndClick;
            private set
            {
                if (_controlFindAndClick != value)
                {
                    _controlFindAndClick = value;
                    OnPropertyChanged(nameof(ControlFindAndClick));
                }
            }
        }

        public string ControlFindWaitClick
        {
            get => _controlFindWaitClick;
            private set
            {
                if (_controlFindWaitClick != value)
                {
                    _controlFindWaitClick = value;
                    OnPropertyChanged(nameof(ControlFindWaitClick));
                }
            }
        }

        public string ControlFindAndContinue
        {
            get => _controlFindAndContinue;
            private set
            {
                if (_controlFindAndContinue != value)
                {
                    _controlFindAndContinue = value;
                    OnPropertyChanged(nameof(ControlFindAndContinue));
                }
            }
        }

        public string TitleClick4Control
        {
            get => _titleClick4Control;
            private set
            {
                if (_titleClick4Control != value)
                {
                    _titleClick4Control = value;
                    OnPropertyChanged(nameof(TitleClick4Control));
                }
            }
        }

        public string TitleTextControl
        {
            get => _titleTextControl;
            private set
            {
                if (_titleTextControl != value)
                {
                    _titleTextControl = value;
                    OnPropertyChanged(nameof(TitleTextControl));
                }
            }
        }

        public string ControlSendText
        {
            get => _controlSendText;
            private set
            {
                if (_controlSendText != value)
                {
                    _controlSendText = value;
                    OnPropertyChanged(nameof(ControlSendText));
                }
            }
        }

        public string ControlSendTextFromFileDel
        {
            get => _controlSendTextFromFileDel;
            private set
            {
                if (_controlSendTextFromFileDel != value)
                {
                    _controlSendTextFromFileDel = value;
                    OnPropertyChanged(nameof(ControlSendTextFromFileDel));
                }
            }
        }

        public string ControlSendTextRandomFromFile
        {
            get => _controlSendTextRandomFromFile;
            private set
            {
                if (_controlSendTextRandomFromFile != value)
                {
                    _controlSendTextRandomFromFile = value;
                    OnPropertyChanged(nameof(ControlSendTextRandomFromFile));
                }
            }
        }

        public string ControlRandomTextAndSend
        {
            get => _controlRandomTextAndSend;
            private set
            {
                if (_controlRandomTextAndSend != value)
                {
                    _controlRandomTextAndSend = value;
                    OnPropertyChanged(nameof(ControlRandomTextAndSend));
                }
            }
        }

        public string ControlDelTextChar
        {
            get => _controlDelTextChar;
            private set
            {
                if (_controlDelTextChar != value)
                {
                    _controlDelTextChar = value;
                    OnPropertyChanged(nameof(ControlDelTextChar));
                }
            }
        }

        public string ControlDelAllText
        {
            get => _controlDelAllText;
            private set
            {
                if (_controlDelAllText != value)
                {
                    _controlDelAllText = value;
                    OnPropertyChanged(nameof(ControlDelAllText));
                }
            }
        }

        public string ControlSendKey
        {
            get => _controlSendKey;
            private set
            {
                if (_controlSendKey != value)
                {
                    _controlSendKey = value;
                    OnPropertyChanged(nameof(ControlSendKey));
                }
            }
        }

        public string ControlCtrlA
        {
            get => _controlCtrlA;
            private set
            {
                if (_controlCtrlA != value)
                {
                    _controlCtrlA = value;
                    OnPropertyChanged(nameof(ControlCtrlA));
                }
            }
        }

        public string ControlCheckKeyBoard
        {
            get => _controlCheckKeyBoard;
            private set
            {
                if (_controlCheckKeyBoard != value)
                {
                    _controlCheckKeyBoard = value;
                    OnPropertyChanged(nameof(ControlCheckKeyBoard));
                }
            }
        }

        public string ControlURLKEYSendText
        {
            get => _controlURLKEYSendText;
            private set
            {
                if (_controlURLKEYSendText != value)
                {
                    _controlURLKEYSendText = value;
                    OnPropertyChanged(nameof(ControlURLKEYSendText));
                }
            }
        }

        public string TitileDataChangeInfo
        {
            get => _titileDataChangeInfo;
            private set
            {
                if (_titileDataChangeInfo != value)
                {
                    _titileDataChangeInfo = value;
                    OnPropertyChanged(nameof(TitileDataChangeInfo));
                }
            }
        }

        public string ControlDataChangeInfo1
        {
            get => _controlDataChangeInfo1;
            private set
            {
                if (_controlDataChangeInfo1 != value)
                {
                    _controlDataChangeInfo1 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo1));
                }
            }
        }

        public string ControlDataChangeInfo2
        {
            get => _controlDataChangeInfo2;
            private set
            {
                if (_controlDataChangeInfo2 != value)
                {
                    _controlDataChangeInfo2 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo2));
                }
            }
        }

        public string ControlDataChangeInfo3
        {
            get => _controlDataChangeInfo3;
            private set
            {
                if (_controlDataChangeInfo3 != value)
                {
                    _controlDataChangeInfo3 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo3));
                }
            }
        }

        public string ControlDataChangeInfo4
        {
            get => _controlDataChangeInfo4;
            private set
            {
                if (_controlDataChangeInfo4 != value)
                {
                    _controlDataChangeInfo4 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo4));
                }
            }
        }

        public string ControlDataChangeInfo5
        {
            get => _controlDataChangeInfo5;
            private set
            {
                if (_controlDataChangeInfo5 != value)
                {
                    _controlDataChangeInfo5 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo5));
                }
            }
        }

        public string ControlDataChangeInfo6
        {
            get => _controlDataChangeInfo6;
            private set
            {
                if (_controlDataChangeInfo6 != value)
                {
                    _controlDataChangeInfo6 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo6));
                }
            }
        }

        public string ControlDataChangeInfo7
        {
            get => _controlDataChangeInfo7;
            private set
            {
                if (_controlDataChangeInfo7 != value)
                {
                    _controlDataChangeInfo7 = value;
                    OnPropertyChanged(nameof(ControlDataChangeInfo7));
                }
            }
        }

        public string ControlWaitReboot
        {
            get => _controlWaitReboot;
            private set
            {
                if (_controlWaitReboot != value)
                {
                    _controlWaitReboot = value;
                    OnPropertyChanged(nameof(ControlWaitReboot));
                }
            }
        }

        public string ControlWaitInternet
        {
            get => _controlWaitInternet;
            private set
            {
                if (_controlWaitInternet != value)
                {
                    _controlWaitInternet = value;
                    OnPropertyChanged(nameof(ControlWaitInternet));
                }
            }
        }

        public string ControlPushFile
        {
            get => _controlPushFile;
            private set
            {
                if (_controlPushFile != value)
                {
                    _controlPushFile = value;
                    OnPropertyChanged(nameof(ControlPushFile));
                }
            }
        }

        public string ControlPullFile
        {
            get => _controlPullFile;
            private set
            {
                if (_controlPullFile != value)
                {
                    _controlPullFile = value;
                    OnPropertyChanged(nameof(ControlPullFile));
                }
            }
        }

        public string TitleGeneral
        {
            get => _titleGeneral;
            private set
            {
                if (_titleGeneral != value)
                {
                    _titleGeneral = value;
                    OnPropertyChanged(nameof(TitleGeneral));
                }
            }
        }

        public string ControlWiFiON
        {
            get => _controlWiFiON;
            private set
            {
                if (_controlWiFiON != value)
                {
                    _controlWiFiON = value;
                    OnPropertyChanged(nameof(ControlWiFiON));
                }
            }
        }

        public string ControlWiFiOFF
        {
            get => _controlWiFiOFF;
            private set
            {
                if (_controlWiFiOFF != value)
                {
                    _controlWiFiOFF = value;
                    OnPropertyChanged(nameof(ControlWiFiOFF));
                }
            }
        }

        public string ControlOpenURL
        {
            get => _controlOpenURL;
            private set
            {
                if (_controlOpenURL != value)
                {
                    _controlOpenURL = value;
                    OnPropertyChanged(nameof(ControlOpenURL));
                }
            }
        }

        public string ControlRunCommandShell
        {
            get => _controlRunCommandShell;
            private set
            {
                if (_controlRunCommandShell != value)
                {
                    _controlRunCommandShell = value;
                    OnPropertyChanged(nameof(ControlRunCommandShell));
                }
            }
        }

        public string ControlOpenApp
        {
            get => _controlOpenApp;
            private set
            {
                if (_controlOpenApp != value)
                {
                    _controlOpenApp = value;
                    OnPropertyChanged(nameof(ControlOpenApp));
                }
            }
        }

        public string ControlCloseApp
        {
            get => _controlCloseApp;
            private set
            {
                if (_controlCloseApp != value)
                {
                    _controlCloseApp = value;
                    OnPropertyChanged(nameof(ControlCloseApp));
                }
            }
        }

        public string ControlEnableApp
        {
            get => _controlEnableApp;
            private set
            {
                if (_controlEnableApp != value)
                {
                    _controlEnableApp = value;
                    OnPropertyChanged(nameof(ControlEnableApp));
                }
            }
        }

        public string ControlDisbledApp
        {
            get => _controlDisbledApp;
            private set
            {
                if (_controlDisbledApp != value)
                {
                    _controlDisbledApp = value;
                    OnPropertyChanged(nameof(ControlDisbledApp));
                }
            }
        }

        public string ControlInstallApp
        {
            get => _controlInstallApp;
            private set
            {
                if (_controlInstallApp != value)
                {
                    _controlInstallApp = value;
                    OnPropertyChanged(nameof(ControlInstallApp));
                }
            }
        }

        public string ControlUninstallApp
        {
            get => _controlUninstallApp;
            private set
            {
                if (_controlUninstallApp != value)
                {
                    _controlUninstallApp = value;
                    OnPropertyChanged(nameof(ControlUninstallApp));
                }
            }
        }

        public string ControlClearDataApp
        {
            get => _controlClearDataApp;
            private set
            {
                if (_controlClearDataApp != value)
                {
                    _controlClearDataApp = value;
                    OnPropertyChanged(nameof(ControlClearDataApp));
                }
            }
        }

        public string ControlSwipeCloseApp
        {
            get => _controlSwipeCloseApp;
            private set
            {
                if (_controlSwipeCloseApp != value)
                {
                    _controlSwipeCloseApp = value;
                    OnPropertyChanged(nameof(ControlSwipeCloseApp));
                }
            }
        }

        public string ControlLoadApp
        {
            get => _controlLoadApp;
            private set
            {
                if (_controlLoadApp != value)
                {
                    _controlLoadApp = value;
                    OnPropertyChanged(nameof(ControlLoadApp));
                }
            }
        }

        public string ControlDeviceMenuDropBox1
        {
            get => _controlDeviceMenuDropBox1;
            private set
            {
                if (_controlDeviceMenuDropBox1 != value)
                {
                    _controlDeviceMenuDropBox1 = value;
                    OnPropertyChanged(nameof(ControlDeviceMenuDropBox1));
                }
            }
        }

        public string ControlDeviceMenuDropBox2
        {
            get => _controlDeviceMenuDropBox2;
            private set
            {
                if (_controlDeviceMenuDropBox2 != value)
                {
                    _controlDeviceMenuDropBox2 = value;
                    OnPropertyChanged(nameof(ControlDeviceMenuDropBox2));
                }
            }
        }

        public string ControlDeviceMenuDropBox3
        {
            get => _controlDeviceMenuDropBox3;
            private set
            {
                if (_controlDeviceMenuDropBox3 != value)
                {
                    _controlDeviceMenuDropBox3 = value;
                    OnPropertyChanged(nameof(ControlDeviceMenuDropBox3));
                }
            }
        }

        public string ControlDeviceMenuDropBox4
        {
            get => _controlDeviceMenuDropBox4;
            private set
            {
                if (_controlDeviceMenuDropBox4 != value)
                {
                    _controlDeviceMenuDropBox4 = value;
                    OnPropertyChanged(nameof(ControlDeviceMenuDropBox4));
                }
            }
        }

        public string ControlDeviceMenuDropBox5
        {
            get => _controlDeviceMenuDropBox5;
            private set
            {
                if (_controlDeviceMenuDropBox5 != value)
                {
                    _controlDeviceMenuDropBox5 = value;
                    OnPropertyChanged(nameof(ControlDeviceMenuDropBox5));
                }
            }
        }

        public string LableViewDevice1
        {
            get => _lableViewDevice1;
            private set
            {
                if (_lableViewDevice1 != value)
                {
                    _lableViewDevice1 = value;
                    OnPropertyChanged(nameof(LableViewDevice1));
                }
            }
        }

        public string LableViewDevice2
        {
            get => _lableViewDevice2;
            private set
            {
                if (_lableViewDevice2 != value)
                {
                    _lableViewDevice2 = value;
                    OnPropertyChanged(nameof(LableViewDevice2));
                }
            }
        }

        public string ControlCheckScreen
        {
            get => _controlCheckScreen;
            private set
            {
                if (_controlCheckScreen != value)
                {
                    _controlCheckScreen = value;
                    OnPropertyChanged(nameof(ControlCheckScreen));
                }
            }
        }

        public string LabelNumberDevice
        {
            get => _labelNumberDevice;
            private set
            {
                if (_labelNumberDevice != value)
                {
                    _labelNumberDevice = value;
                    OnPropertyChanged(nameof(LabelNumberDevice));
                }
            }
        }

        public string Setting
        {
            get => _setting;
            private set
            {
                if (_setting != value)
                {
                    _setting = value;
                    OnPropertyChanged(nameof(Setting));
                }
            }
        }

        public string Document
        {
            get => _document;
            private set
            {
                if (_document != value)
                {
                    _document = value;
                    OnPropertyChanged(nameof(Document));
                }
            }
        }

        public LocalizationViewModel()
        {
            Refresh(); // Khởi tạo backing field trong constructor
        }

        public void Refresh()
        {
            LocalizationService.SetLanguage(Properties.Settings.Default.lang);

            Device = LocalizationService.Get("Device");
            Automation = LocalizationService.Get("Automation");
            Screen = LocalizationService.Get("Screen");
            CheckBoxAutomation = LocalizationService.Get("langCheckBoxAutomation");
            ControlClickXY = LocalizationService.Get("ControlClickXY");
            ControlSwipe = LocalizationService.Get("ControlSwipe");
            ControlRandomClick = LocalizationService.Get("ControlRandomClick");
            ControlWait = LocalizationService.Get("ControlWait");
            TitleClickControl = LocalizationService.Get("TitleClickControl");
            TitleClick2Control = LocalizationService.Get("TitleClick2Control");
            ControlSearchAndClick = LocalizationService.Get("ControlSearchAndClick");
            ControlSearchWaitClick = LocalizationService.Get("ControlSearchWaitClick");
            ControlSearchAndContinue = LocalizationService.Get("ControlSearchAndContinue");
            TitleClick3Control = LocalizationService.Get("TitleClick3Control");
            ControlFindAndClick = LocalizationService.Get("ControlFindAndClick");
            ControlFindWaitClick = LocalizationService.Get("ControlFindWaitClick");
            ControlFindAndContinue = LocalizationService.Get("ControlFindAndContinue");
            TitleClick4Control = LocalizationService.Get("TitleClick4Control");
            TitleTextControl = LocalizationService.Get("TitleTextControl");
            ControlSendText = LocalizationService.Get("ControlSendText");
            ControlSendTextFromFileDel = LocalizationService.Get("ControlSendTextFromFileDel");
            ControlSendTextRandomFromFile = LocalizationService.Get("ControlSendTextRandomFromFile");
            ControlRandomTextAndSend = LocalizationService.Get("ControlRandomTextAndSend");
            ControlDelTextChar = LocalizationService.Get("ControlDelTextChar");
            ControlDelAllText = LocalizationService.Get("ControlDelAllText");
            ControlSendKey = LocalizationService.Get("ControlSendKey");
            ControlCtrlA = LocalizationService.Get("ControlCtrlA");
            ControlCheckKeyBoard = LocalizationService.Get("ControlCheckKeyBoard");
            ControlURLKEYSendText = LocalizationService.Get("ControlURLKEYSendText");
            TitileDataChangeInfo = LocalizationService.Get("TitileDataChangeInfo");
            ControlDataChangeInfo1 = LocalizationService.Get("ControlDataChangeInfo1");
            ControlDataChangeInfo2 = LocalizationService.Get("ControlDataChangeInfo2");
            ControlDataChangeInfo3 = LocalizationService.Get("ControlDataChangeInfo3");
            ControlDataChangeInfo4 = LocalizationService.Get("ControlDataChangeInfo4");
            ControlDataChangeInfo5 = LocalizationService.Get("ControlDataChangeInfo5");
            ControlDataChangeInfo6 = LocalizationService.Get("ControlDataChangeInfo6");
            ControlDataChangeInfo7 = LocalizationService.Get("ControlDataChangeInfo7");
            ControlWaitReboot = LocalizationService.Get("ControlWaitReboot");
            ControlWaitInternet = LocalizationService.Get("ControlWaitInternet");
            ControlPushFile = LocalizationService.Get("ControlPushFile");
            ControlPullFile = LocalizationService.Get("ControlPullFile");
            TitleGeneral = LocalizationService.Get("TitleGeneral");
            ControlWiFiON = LocalizationService.Get("ControlWiFiON");
            ControlWiFiOFF = LocalizationService.Get("ControlWiFiOFF");
            ControlOpenURL = LocalizationService.Get("ControlOpenURL");
            ControlRunCommandShell = LocalizationService.Get("ControlRunCommandShell");
            ControlOpenApp = LocalizationService.Get("ControlOpenApp");
            ControlCloseApp = LocalizationService.Get("ControlCloseApp");
            ControlEnableApp = LocalizationService.Get("ControlEnableApp");
            ControlDisbledApp = LocalizationService.Get("ControlDisbledApp");
            ControlInstallApp = LocalizationService.Get("ControlInstallApp");
            ControlUninstallApp = LocalizationService.Get("ControlUninstallApp");
            ControlClearDataApp = LocalizationService.Get("ControlClearDataApp");
            ControlSwipeCloseApp = LocalizationService.Get("ControlSwipeCloseApp");
            ControlLoadApp = LocalizationService.Get("ControlLoadApp");
            ControlDeviceMenuDropBox1 = LocalizationService.Get("ControlDeviceMenuDropBox1");
            ControlDeviceMenuDropBox2 = LocalizationService.Get("ControlDeviceMenuDropBox2");
            ControlDeviceMenuDropBox3 = LocalizationService.Get("ControlDeviceMenuDropBox3");
            ControlDeviceMenuDropBox4 = LocalizationService.Get("ControlDeviceMenuDropBox4");
            ControlDeviceMenuDropBox5 = LocalizationService.Get("ControlDeviceMenuDropBox5");
            LableViewDevice1 = LocalizationService.Get("LableViewDevice1");
            LableViewDevice2 = LocalizationService.Get("LableViewDevice2");
            ControlCheckScreen = LocalizationService.Get("ControlCheckScreen");
            LabelNumberDevice = LocalizationService.Get("LabelNumberDevice");
            Setting = LocalizationService.Get("Setting");
            Document = LocalizationService.Get("Document");
            // device
            ToolChange.Language.DevicesLang.logTitleProxy = LocalizationService.Get("logTitleProxy");
            ToolChange.Language.DevicesLang.logTitleProxySuccess = LocalizationService.Get("logTitleProxySuccess");
            ToolChange.Language.DevicesLang.logCheckProxy = LocalizationService.Get("logCheckProxy");
            ToolChange.Language.DevicesLang.logDeviceRandomEx = LocalizationService.Get("logDeviceRandomEx");
            ToolChange.Language.DevicesLang.logSelectDeviceChange = LocalizationService.Get("logSelectDeviceChange");
            ToolChange.Language.DevicesLang.logChangeDevice = LocalizationService.Get("logChangeDevice");
            ToolChange.Language.DevicesLang.logChangeDeviceKeyBox = LocalizationService.Get("logChangeDeviceKeyBox");
            ToolChange.Language.DevicesLang.logChangeDevicePif = LocalizationService.Get("logChangeDevicePif");
            ToolChange.Language.DevicesLang.logErrorExChangeDevice = LocalizationService.Get("logErrorExChangeDevice");
            ToolChange.Language.DevicesLang.logErrorExTitleChangeDevice = LocalizationService.Get("logErrorExTitleChangeDevice");
            ToolChange.Language.DevicesLang.TitleDetailDevice = LocalizationService.Get("TitleDetailDevice");
            ToolChange.Language.DevicesLang.TitleProxy = LocalizationService.Get("TitleProxy");
            ToolChange.Language.DevicesLang.TitleProxyId = LocalizationService.Get("TitleProxyId");
            ToolChange.Language.DevicesLang.TitleLocation = LocalizationService.Get("TitleLocation");
            ToolChange.Language.DevicesLang.TitleUrl = LocalizationService.Get("TitleUrl");

            //lang
            ToolChange.Language.Lang.LogInfomation = LocalizationService.Get("LogInfomation");
            ToolChange.Language.Lang.LogWarning = LocalizationService.Get("LogError");
            ToolChange.Language.Lang.LogError = LocalizationService.Get("LogWarning");
        }
      
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}