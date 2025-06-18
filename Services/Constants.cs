using POCO.Models;
using System.Collections.Generic;
using System.Reflection;
//Services
namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public class Settings_Type
    {
        public const string BASE_PATH = "/data/system/users/0/";
        public const string SECURE = "/data/system/users/0/settings_secure.xml";
        public const string GLOBAL = "/data/system/users/0/settings_global.xml";
        public const string SYSTEM = "/data/system/users/0/settings_system.xml";
        public const string SSAID = "//data/system/users/0/settings_ssaid.xml";
        public const string SSAID_LOCAL_NAME = "/settings_ssaid.xml";
        public const string WIFI_CONFIG = "/data/misc/wifi/WifiConfigStore.xml";
    }
    [ObfuscationAttribute(Exclude = false)]
    public class GlobalAndroidSettings
    {
        public static readonly string MI_PREFIX = "mi_";
        public static readonly string IMEI0 = string.Concat(MI_PREFIX, "imei_number");
        public static readonly string IMEI1 = string.Concat(MI_PREFIX, "imei_number1");
        public static readonly string ICCID = string.Concat(MI_PREFIX, "iccid");
        public static readonly string IMSI = string.Concat(MI_PREFIX, "imsi");
        public static readonly string SIM_OPERATOR_COUNTRY = string.Concat(MI_PREFIX, "sim_operator_country");
        public static readonly string SIM_OPERATOR_NUMERIC = string.Concat(MI_PREFIX, "sim_operator_numeric");
        public static readonly string SIM_OPERATOR_NAME = string.Concat(MI_PREFIX, "sim_operator_name");
        public static readonly string SIM_STATE = string.Concat(MI_PREFIX, "sim_state");
        public static readonly string HARDWARE_SERIALNO = string.Concat(MI_PREFIX, "hardware_serialno");
        public static readonly string LONGITUDE = string.Concat(MI_PREFIX, "longitude");
        public static readonly string LATITUDE = string.Concat(MI_PREFIX, "latitude");

        public static readonly string NETWORK_OPERATOR_COUNTRY = string.Concat(MI_PREFIX, "network_operator_country");
        public static readonly string NETWORK_OPERATOR_NUMERIC = string.Concat(MI_PREFIX, "network_operator_numeric");
        public static readonly string NETWORK_OPERATOR_NAME = string.Concat(MI_PREFIX, "network_operator_name");
        public static readonly string NETWORK_TYPE = string.Concat(MI_PREFIX, "network_type");
        public static readonly string SIM_PHONE_NUMBER = string.Concat(MI_PREFIX, "line1_number");
        public static readonly string DATA_ACTIVITY = string.Concat(MI_PREFIX, "data_activity");
        public static readonly string DATA_STATE = string.Concat(MI_PREFIX, "data_state");
        public static readonly string DATA_NETWORK_TYPE = string.Concat(MI_PREFIX, "data_network_type");

        public static readonly string ANDROID_ID = string.Concat(MI_PREFIX, "android_id");
        public static readonly string SIM_STATE_READY = string.Concat(MI_PREFIX, "is_sim_ready");
        public static readonly string SIM_ICC_AVAILABLE = string.Concat(MI_PREFIX, "is_icc_available");


    }
    [ObfuscationAttribute(Exclude = false)]
    public class SecurityConfig
    {
        public const string DEFAULT_PASSWORD = "271189";
    }
    [ObfuscationAttribute(Exclude = false)]
    class BuildKey_SYSTEM
    {
        public static readonly string BASEBAND = "ro.lineage.gsm.version.baseband";


        public static readonly string BRAND = "ro.product.brand";
        //public static readonly string DEVICE_ID = "";
        public static readonly string BOARD = "ro.product.board";
        public static readonly string MODEL = "ro.product.model";
        public static readonly string HARDWARE_SERIAL = "ro.serialno";
        public static readonly string HARDWARE_SERIAL_BOOT = "ro.boot.serialno";

        public const string VENDOR = "ro.product.manufacturer";
        public const string HARDWARE = "ro.hardware";
        public const string SIM_SERIAL = "";

        public const string SIM_OPERATOR_CODE = "gsm.sim.operator.numeric";
        public const string SIM_OPERATOR_CODE_COUNTRY_ISO = "gsm.sim.operator.iso-country";
        public const string SIM_OPERATOR_ALPHA = "gsm.sim.operator.alpha";
        public const string NETWORK_OPERATOR_CODE = "gsm.operator.numeric";

        //code name
        public const string DEVICE = "ro.product.device";
        //fingerprint
        public const string FINGERPRINT = "ro.build.fingerprint";
        //build ID
        public const string BUILD_ID = "ro.build.id";
        //version release
        public const string VERSION_RELEASE = "ro.build.version.release";
        //version sdk
        public const string VERSION_SDK = "ro.build.version.sdk";
        //version security path
        public const string SECURITY_PATH = "ro.build.version.security_patch";
        //Builder
        public const string BUILD_HOST = "ro.build.host";
        //Bootloader
        public static readonly string BOOT_LOADER = "ro.bootloader";
        public static readonly string BOOT_LOADER_LINEAGE = "ro.lineage.bootloader";

        //Wifi mac address
        public const string WIFI_MAC_ADDRESS = "ro.lineage.wifi";
        //BlueTooth mac address
        public const string BLUETOOTH_MAC_ADDRESS = "ro.lineage.bluetooth";

        public const string TIMEZONE = "persist.sys.timezone";
        public const string LOCALE = "persist.sys.locale";

        public const string PRODUCT = "ro.product.name";
        public const string PLATFORM = "ro.board.platform";
        public const string BUILD_DISPLAY_ID = "ro.build.display.id";
        public const string BUILD_TAGS = "ro.build.tags";
        public const string BUILD_INCREMENTAL = "ro.build.version.incremental";
        public const string BUILD_DESCRIPTION = "ro.build.description";
        public const string BUILD_DATE = "ro.build.date";
        public const string BUILD_DATE_UTC = "ro.build.date.utc";
        public const string BUILD_FLAVOR = "ro.build.flavor";
    }
    [ObfuscationAttribute(Exclude = false)]
    class BuildKey_VENDOR
    {
        public const string BRAND = "ro.product.vendor.brand";
        public const string MODEL = "ro.product.vendor.model";
        public const string VENDOR = "ro.product.vendor.manufacturer";
        //code name
        public const string DEVICE = "ro.product.vendor.device";

        public const string FINGERPRINT = "ro.vendor.build.fingerprint";
        public const string FINGERPRINT_BOOT_IMAGE = "ro.bootimage.build.fingerprint";
        public const string SECURITY_PATH = "ro.vendor.build.security_patch";//ro.vendor.build.security_patch
        public const string BUILD_ID = "ro.vendor.build.id";
        public const string BUILD_TAGS = "ro.vendor.build.tags";
        public const string BUILD_INCREMENTAL = "ro.vendor.build.version.incremental";
        public const string BUILD_DATE = "ro.vendor.build.date";
        public const string BUILD_DATE_UTC = "ro.vendor.build.date.utc";


    }
    [ObfuscationAttribute(Exclude = false)]

    public class Package_Data
    {
        public const string ROOT_PATH = "/";
        public const string DATA_LOCAL_PATH = "/data/local/";
        public const string APPSDATA_PATH = "/data/local/tmp/AppsData/";
        public const string RRS_PATH = "/data/local/tmp/RRS/";
        public const string APPSDATA_DESCRIPTION_FILE = "/appdatainfo.json";
        public const string DEFAULT_DATA_PATH = "~/data/data/";
        public const string SDCARD_PATH = "/sdcard/";
        public const string SDCARD_ALLFILES_PATH = "~/mnt/sdcard/*";
        public const string KEYSTORE_FILES_PATH = "~/data/misc/keystore/user_0/*";
        public const string KEYSTORE_PATH = "~/data/misc/keystore/";
        public const string GMS = "~/data/data/com.google.android.gms/*";
        public const string GSF = "~/data/data/com.google.android.gsf/*";
        public const string VENDING = "~/data/data/com.android.vending/*";
        public const string CHROME = "~/data/data/com.android.chrome/*";
        public const string SYSTEM_SYNC = "~/data/system/sync/*";
        public const string SYSTEM_CE = "~/data/system_ce/0/*";
        public const string SYSTEM_DE = "~/data/system_de/0/*";

        public const string IMS = "~/data/data/com.google.android.ims/*";
        public const string GMAIL = "~/data/data/com.google.android.gm/*";
        public const string CALENDAR = "~/data/data/com.google.android.calendar/*";
        public const string PLAY_GAME = "~/data/data/com.google.android.play.games/*";
        public const string HTML_VIEWER = "~/data/data/com.android.htmlviewer/*";
        public const string WEBVIEW = "~/data/data/com.android.webview/*";
        public const string GOOGLE_BACKUPTRANSPORT = "~/data/data/com.google.android.backuptransport/*";
        public const string GOOGLE_WEBVIEW = "~/data/data/com.google.android.webview/*";
        public const string LOCATION = "~/data/data/com.android.location.fused/*";
        public const string YOUTUBE = "~/data/data/com.google.android.youtube/*";
        public const string APPS = "~/data/data/com.google.android.apps.docs/*";
        public const string JELLY = "~/data/data/org.lineageos.jelly/*";

        public const string GAID = "//data/data/com.google.android.gms/shared_prefs/adid_settings.xml";
        public const string PACKAGE_RESTRICT = "//data/system/users/0/package-restrictions.xml";
        public const string PACKAGE_SYSTEM_XML = "//data/system/packages.xml";
        public const string PACKAGE_SYSTEM_LIST = "//data/system/packages.list";
        public const string MISC_PROFILE_CUR = "//data/misc/profiles/cur/0/";
        public const string MISC_PROFILE_REF = "//data/misc/profiles/ref/";

        public const string VENDING_DB_PKG = "//data/data/com.android.vending/databases/xternal_referrer_status.db";
        public const string WINDOW_DUMP_PATH = "//sdcard/window_dump.xml";
        public const string GSERVICES_DB = "//data/data/com.google.android.gsf/databases/gservices.db";
        public const string GSERVICES_DB_WAL = "//data/data/com.google.android.gsf/databases/gservices.db-wal";
        public const string GSERVICES_DB_SHM = "//data/data/com.google.android.gsf/databases/gservices.db-shm";
        public const string GMS_CHECKIN_ID_TOKEN = "//data/data/com.google.android.gms/files/checkin_id_token";
        public const string GMS_CHECKIN_ID = "//data/data/com.google.android.gms/shared_prefs/Checkin.xml";
    }
    [ObfuscationAttribute(Exclude = false)]

    public class UserAgentChrome
    {
        public const string TEMP_PATH = "~/data/local/tmp/chrome-command-line";
        public const string REAL_PATH = "~/data/local/chrome-command-line";
        public const string RESOURCE_NAME = "chrome-user-agents.json";
        public const string FLAGS = "chrome --user-agent=";
        //public const string ANDROID_VERSION = 0;
        //public const string MODEL = 1;
        //public const string BUID_ID = 2;
    }
    [ObfuscationAttribute(Exclude = false)]
    public class URL_Data
    {
        public const string URL_CHECK_IP = "http://pro.ip-api.com/json/?key=IwWimzV1FTxXk3v";
        public const string URL_CHECK_BY_IP = "http://pro.ip-api.com/json/{0}?key=IwWimzV1FTxXk3v"; //replace IPAddress
        public const string URL_CHECK_IPv4v6 = "http://v4v6.ipv6-test.com/api/myip.php";
        public const string URL_CHECK_IPv4v6_IFCONFIG = "http://ifconfig.io/ip";
        public const string URL_CHECK_IPv4v6_API64 = "https://api64.ipify.org";
        public const string PLAY_STORE_URL = "https://play.google.com/store/apps/details?id=";
        public const string RENT_CODE_ID_REQUEST = "https://api.rentcode.net/api/v2/order/request?apiKey=mg3hFkccUBtnIjNnlTNXk2op8G7q1vcnjFInqioJJDMv&ServiceProviderId=40";
        public const string RENT_CODE_CHECK_MESSAGE = "https://api.rentcode.net/api/v2/order/{0}/check?apiKey=mg3hFkccUBtnIjNnlTNXk2op8G7q1vcnjFInqioJJDMv";
    }

    public enum KeyEventCode
    {
        KEYCODE_APP_SWITCH,
        HOME,
        BACK,
        ENTER,
        POWER,
        TAB,
        KEYCODE_MOVE_END = 123
    }

    public enum IPhoneSubInfo
    {
        IMEI = 3,
        ICCID = 11,
        SUBSCRIBER_ID = 7
    }

    public class ChangeLanguage
    {
        public static readonly Dictionary<string, string> LANGUAGE_CODE = new Dictionary<string, string>
        {
            { "KR", "ko-rKR" },
            { "US", "en-rUS" },
            { "CN", "zh-rCN" },
            { "VN", "vi-rVN" },
            { "DE", "de-rDE"},
            { "JP", "ja-rJP"},
            { "ES", "ca-rES"},
            { "CA", "en-rCA"},
            { "TW", "zh-rTW"},
            { "IN", "en-rIN"},
            { "AU", "en-rAU"},
            { "GB", "en-rGB"},
            { "HK", "zh-rHK"},
            { "ID", "id-rID"},
            { "FR", "fr-rFR"},
            { "RU", "ru-rRU"},
            { "BR", "es-rBR"},
            { "SA", "ar-rSA"},
            { "SG", "en-rSG"},
            { "BE", "de-rBE"}
        };

        public const string PERMISSION = "android.permission.CHANGE_CONFIGURATION";
    }
    public class Android_Version
    {
        public static List<BuildVersion> BUILD_VERSIONS = new List<BuildVersion> {
            //new BuildVersion{Release = @"4.4", SDK = "19" },
            //new BuildVersion{Release = @"4.4.4", SDK = "19"},
            //new BuildVersion{Release = @"5.0", SDK = "21"},
            //new BuildVersion{Release = @"5.1.1", SDK = "22"},
            //new BuildVersion{Release = @"6.0", SDK = "23"},
            //new BuildVersion{Release = @"6.0.1", SDK = "23"},
            //new BuildVersion{Release = @"7.0", SDK = "24"},
            //new BuildVersion{Release = @"7.1", SDK = "25"}
            new BuildVersion{Release = @"8.0.0", SDK = "26"},
            new BuildVersion{Release = @"8.1.0", SDK = "27"},
            new BuildVersion{Release = @"9", SDK = "28"}
        };
    }
    class SystemProperties
    {
        public static readonly string TIMEZONE = "persist.sys.timezone";
    }
    [Obfuscation(Exclude = false)]
    public class PackageName
    {
        public const string ADB_CHANGE_LANGUAGE = "net.sanapeli.adbchangelanguage";
        public const string GMS = "com.google.android.gms";
        public const string GSF = "com.google.android.gsf";
        public const string IMS = "com.google.android.ims";
        public const string VENDING = "com.android.vending";
        public const string GMAIL = "com.google.android.gm";
        public const string CALENDAR = "com.google.android.calendar";
        public const string YOUTUBE = "com.google.android.youtube";
        public const string YOUTUBE_SEARCH = "com.google.android.youtube.action.open.search";
        public const string PLAY_GAMES = "com.google.android.play.games";
        public const string PROVIDER_DOWNLOAD = "com.android.providers.downloads";
        public const string PROVIDER_DOWNLOAD_UI = "com.android.providers.downloads.ui";
        public const string SETTINGS = "com.android.settings";
        public const string DEFAULT_WEBVIEW = "com.android.webview";
    }

    public enum CacheProxyType
    {
        SSH,
        MICRO
    }

    public class ActivityComponentID
    {
        public const string VENDING_BUTTON = "button.view.ButtonView";
        public const string VENDING_DESCRIPTION = "descriptiontext.view.DescriptionTextModuleView";
        public const string ACCOUNT_MANAGER_ACTIVITY = @"com.android.settings/.Settings\$AccountDashboardActivity";
        public static readonly Dictionary<string, string> INSTALL_TEXT = new Dictionary<string, string>
        {
            { "KR", "설치" },
            { "US", "Install" },
            { "CN", "安装" },
            { "VN", "Cài đặt" },
            { "DE", "Installieren"},
            { "JP", "インストール"},
            { "ES", "Instalar"},
            { "CA", "Install"},
            { "TW", "安裝"},
            { "IN", "Install"},
            { "AU", "Install"},
            { "GB", "Install"},
            { "HK", "安裝"},
            { "ID", "Instal"},
            { "FR", "Installer"},
            { "RU", "Установить"},
            { "BR", "Instalar"},
            { "SA", "تثبيت"},
            { "SG", "Install"},
            { "BE", "Installieren"}
        };
    }
    public enum FakeDeviceInAction
    {
        LEAD,
        RRS,
        APP
    }
    public enum DeviceCodeName
    {
        TISSOT,
        HEROLTE,
        JASMINE_SPROUT,
        STARLTE,
        SARGO,
        CROSSHATCH
    }
    public class OTACerts
    {
        public const string HEROLTE = @"MIIEDTCCAvWgAwIBAgIUNkk9RIl8RYrWsCqjnGfxNd1uT58wDQYJKoZIhvcNAQEL
BQAwgZUxCzAJBgNVBAYTAlVTMRMwEQYDVQQIDApDYWxpZm9ybmlhMRYwFAYDVQQH
DA1Nb3VudGFpbiBWaWV3MRAwDgYDVQQKDAdBbmRyb2lkMRAwDgYDVQQLDAdBbmRy
b2lkMRAwDgYDVQQDDAdBbmRyb2lkMSMwIQYJKoZIhvcNAQkBFhRtYWlnaWF0cmFu
QGdtYWlsLmNvbTAeFw0yMDAxMjExNzE2MDZaFw00NzA2MDgxNzE2MDZaMIGVMQsw
CQYDVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYTEWMBQGA1UEBwwNTW91bnRh
aW4gVmlldzEQMA4GA1UECgwHQW5kcm9pZDEQMA4GA1UECwwHQW5kcm9pZDEQMA4G
A1UEAwwHQW5kcm9pZDEjMCEGCSqGSIb3DQEJARYUbWFpZ2lhdHJhbkBnbWFpbC5j
b20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDA+GHjS9T1+9eLeB0V
LvgxqrA4rTmAdE1zuh5lQbz1MXpav9P3IyqeAYXbyBGizHILHYqIeOXTYtOLYZeU
y2DuslfKJErV84LFjdYQgDTkh0lVuy2KZSWx74v0/5QM/4WTnOca9jf35Vcd+OJ+
x+jtI2nTE3vQtGclTYQyD2ibd85Q2ePsPWNQQKm1VszYNEwMwrR8wSLZ+6Wd6uuw
8HCX8MZae6ZqUX8+2QYdJYNXmshRZv0E0b2SQB7FSdISs2gg+soFbqx3hV1pmsUS
3oCilwMJjMtz3SX2dx0g5sjV3JwLkoovpk+vB3kVUrSnQpu4S/ibrOHHIugcws77
5Ci1AgMBAAGjUzBRMB0GA1UdDgQWBBQxUNrK23PtS2fTLdxmdCQuv+KjjzAfBgNV
HSMEGDAWgBQxUNrK23PtS2fTLdxmdCQuv+KjjzAPBgNVHRMBAf8EBTADAQH/MA0G
CSqGSIb3DQEBCwUAA4IBAQCXUKLQbq7jTt/N7N4nIyHmMbRvjoitHMvkKtlZ6diu
Q2daLkwf/u2MNHUH5Y3c1KQpLRA9azttRYjtAutXWIn2tWZgJdallbTteMs9LwTe
EvWnat3Cg3dgBnd5Xnma26SJwOW729ReLcpSAdmHwbBGhgCr7sYAEZvmxpG9YZTR
ggTO1xe2Vtvy+lsAg+S58Ly6fY/XDAUc97XH9sPRwPivpt/csqYUzD3EYaYxkZIf
i13hoNCX5aJpZqOhmtPnUt5+0vE+doJinwdML7TxMAkr1ySU2X6i0+nIJ8QD70d4
Z+I+HcoPgHKBJGgHhZkVeWK4EqDg2JBBlTibpMLZD93f";
    }
    public static class ScriptAction
    {
        public static readonly string[] EVENT = {
            TOUCH,
            TOUCH_RANDOM,
            KEY,
            SEND,
            WAIT,
            WAIT_RANDOM,
            OPEN_LINK,
            OPEN_LINK_IN_FILE,
            SWIPE,
            OPEN_PACKAGE,
            TOUCH_BY_TEXT,
            RANDOM_NAME,
            RANDOM_BASIC_INFO,
            GET_CODE_SMS
        };
        public const string TOUCH = "touch";
        public const string TOUCH_RANDOM = "touchrandom";
        public const string KEY = "key";
        public const string SEND = "send";
        public const string WAIT = "wait";
        public const string WAIT_RANDOM = "waitrandom";
        public const string OPEN_LINK = "openlink";
        public const string OPEN_LINK_IN_FILE = "openlinkinfile";
        public const string SWIPE = "swipe";
        public const string OPEN_PACKAGE = "openpackage";
        public const string LOOP_START = "loopstart";
        public const string LOOP_STOP = "loopstop";
        public const string TOUCH_BY_TEXT = "touchbytext";
        public const string RANDOM_NAME = "randomname";
        public const string RANDOM_BASIC_INFO = "randombasicinfo";
        public const string GET_CODE_SMS = "getcodesms";
    }
    public class BasicGmailInfo
    {
        public static readonly string[] MONTHS = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public static readonly string[] GENDERS = { "Female", "Male", "Rather", "Custom" };
        public const string MONTH_FLAG = "script.month";
        public const string DAY_FLAG = "script.day";
        public const string YEAR_FLAG = "script.year";
        public const string GENDER_FLAG = "script.gender";
        public const string FIRSTNAME_FLAG = "script.firstname";
        public const string LASTNAME_FLAG = "script.lastname";
        public const string MIDNAME_FLAG = "script.midnames";
        public const string CODE_SMS = "script.codesms";
        public static readonly string[] LIST_FLAGS = { MONTH_FLAG, DAY_FLAG, YEAR_FLAG, GENDER_FLAG, FIRSTNAME_FLAG, LASTNAME_FLAG, CODE_SMS };
    }
    public enum FindDumpNodeByType
    {
        ID,
        TEXT
    }
    public class AndroidPath
    {
        public const string DEFAULT_DUMP_XML = "/sdcard/window_dump.xml";
    }

    public enum ProxyMode
    {
        DIRECT,
        SHADOWSOCKS,
        SOCKS5,
        HTTP
    }
    public enum Orientation
    {
        LANDSCAPE,
        PORTRAIT
    }
}
