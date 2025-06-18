namespace ToolChange.ViewModels.Constants
{
    class ApiAuthenticationType
    {
        public const string API_KEY = "x-api-key";
        public const string TOKEN = "authorization";
    }
    class BuildKey_SYSTEM_S9
    {
        public static readonly string HARDWARE = "ro.hardware";
        public static readonly string TYPE = "ro.build.type";
        public static readonly string TYPE_SYSTEM = "ro.system.build.type";
        public static readonly string BRAND = "ro.product.brand";
        public static readonly string BRAND_SYSTEM = "ro.product.system.brand";
        //public static readonly string DEVICE_ID = "";
        public static readonly string BOARD = "ro.product.board";
        public static readonly string LINEAGE_BOARD = "ro.android.board";
        public static readonly string USER = "ro.build.user";
        public static readonly string MODEL = "ro.product.model";
        public static readonly string MODEL_SYSTEM = "ro.product.system.model";
        public static readonly string BUILD_PRODUCT = "ro.build.product";

        public static readonly string MODEL_LINEAGE = "ro.lineage.device";
        public static readonly string HARDWARE_SERIAL = "ro.boot.serialno";
        public static readonly string VENDOR = "ro.product.manufacturer";
        public static readonly string VENDOR_SYSTEM = "ro.product.system.manufacturer";
        public static readonly string SIM_SERIAL = "";

        public static readonly string SIM_OPERATOR_CODE = "gsm.sim.operator.numeric";
        public static readonly string SIM_OPERATOR_CODE_COUNTRY_ISO = "gsm.sim.operator.iso-country";
        public static readonly string SIM_OPERATOR_ALPHA = "gsm.sim.operator.alpha";
        public static readonly string NETWORK_OPERATOR_CODE = "gsm.operator.numeric";
        //code name
        public static readonly string DEVICE = "ro.product.device";
        public static readonly string DEVICE_SYSTEM = "ro.product.system.device";
        //fingerprint
        public static readonly string FINGERPRINT = "ro.build.fingerprint";
        public static readonly string FINGERPRINT_SYSTEM = "ro.system.build.fingerprint";

        //build ID
        public static readonly string BUILD_ID = "ro.build.id";
        public static readonly string BUILD_ID_SYSTEM = "ro.system.build.id";
        //version release
        public static readonly string VERSION_RELEASE = "ro.build.version.release";
        public static readonly string VERSION_RELEASE_SYSTEM = "ro.system.build.version.release";
        //sdk
        public static readonly string VERSION_SDK = "ro.build.version.sdk";
        //security_path
        public static readonly string SECURITY_PATH = "ro.build.version.security_patch";
        //Builder
        public static readonly string BUILD_HOST = "ro.build.host";
        //Bootloader
        public static readonly string BOOT_LOADER = "ro.bootloader";
        public static readonly string BOOT_BOOT_LOADER = "ro.boot.bootloader";
        public static readonly string BOOT_LOADER_LINEAGE = "ro.android.bootloader";

        public static readonly string GSM_BASEBAND = "gsm.version.baseband";
        public static readonly string BASEBAND = "ro.android.gsm.version.baseband";
        // LINEAGE version OS
        public static readonly string LINEAGE_VERSION = "ro.android.version";
        public static readonly string LINEAGE_DISPLAY_VERSION = "ro.lineage.display.version";
        public static readonly string LINEAGE_BUILD_VERSION = "ro.lineage.build.version";
        public static readonly string LINEAGE_BUILD_VERSION_PLAT_SDK = "ro.lineage.build.version.plat.sdk";
        public static readonly string LINEAGE_HARDWARE = "ro.android.hardware";

        //Wifi mac address
        public static readonly string WIFI_MAC_ADDRESS = "ro.android.wifi";
        //BlueTooth mac address
        public static readonly string BLUETOOTH_MAC_ADDRESS = "ro.android.bluetooth";

        public static readonly string PRODUCT = "ro.product.name";
        public static readonly string PRODUCT_SYSTEM = "ro.product.system.name";
        public static readonly string PLATFORM = "ro.board.platform";
        public static readonly string LINEAGE_PLATFORM = "ro.android.platform";
        public static readonly string BUILD_DISPLAY_ID = "ro.build.display.id";
        public static readonly string BUILD_TAGS = "ro.build.tags";
        public static readonly string BUILD_TAGS_SYSTEM = "ro.system.build.tags";
        public static readonly string BUILD_INCREMENTAL = "ro.build.version.incremental";
        public static readonly string BUILD_INCREMENTAL_SYSTEM = "ro.system.build.version.incremental";

        public static readonly string BUILD_DESCRIPTION = "ro.build.description";
        public static readonly string BUILD_DATE = "ro.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.build.date.utc";
        public static readonly string BUILD_FLAVOR = "ro.build.flavor";

        public static readonly string SSID = "ro.android.SSID";
        public static readonly string BSSID = "ro.android.BSSID";

        public static readonly string BUILD_DATE_SYSTEM_EXT = "ro.system_ext.build.date";
        public static readonly string BUILD_DATE_UTC_SYSTEM_EXT = "ro.system_ext.build.date.utc";
        public static readonly string FINGERPRINT_SYSTEM_EXT = "ro.system_ext.build.fingerprint";
        public static readonly string BUILD_ID_SYSTEM_EXT = "ro.system_ext.build.id";
        public static readonly string BUILD_TAGS_SYSTEM_EXT = "ro.system_ext.build.tags";
        public static readonly string TYPE_SYSTEM_EXT = "ro.system_ext.build.type";
        public static readonly string BUILD_INCREMENTAL_SYSTEM_EXT = "ro.system_ext.build.version.incremental";
        public static readonly string VERSION_RELEASE_SYSTEM_EXT = "ro.system_ext.build.version.release";
        public static readonly string VERSION_RELEASE2_SYSTEM_EXT = "ro.system_ext.build.version.release_or_codename";
        public static readonly string VERSION_SDK_SYSTEM_EXT = "ro.system_ext.build.version.sdk";
        public static readonly string BRAND_SYSTEM_EXT = "ro.product.system_ext.brand";
        public static readonly string DEVICE_SYSTEM_EXT = "ro.product.system_ext.device";
        public static readonly string MANUFACTURER_SYSTEM_EXT = "ro.product.system_ext.manufacturer";
        public static readonly string MODEL_SYSTEM_EXT = "ro.product.system_ext.model";
        public static readonly string PRODUCT_SYSTEM_EXT = "ro.product.system_ext.name";

        public static readonly string BUILD_DATE_PRODUCT = "ro.product.build.date";
        public static readonly string BUILD_DATE_UTC_PRODUCT = "ro.product.build.date.utc";
        public static readonly string FINGERPRINT_PRODUCT = "ro.product.build.fingerprint";
        public static readonly string BUILD_ID_PRODUCT = "ro.product.build.id";
        public static readonly string BUILD_TAGS_PRODUCT = "ro.product.build.tags";
        public static readonly string TYPE_PRODUCT = "ro.product.build.type";
        public static readonly string BUILD_INCREMENTAL_PRODUCT = "ro.product.build.version.incremental";
        public static readonly string VERSION_RELEASE_PRODUCT = "ro.product.build.version.release";
        public static readonly string VERSION_RELEASE2_PRODUCT = "ro.product.build.version.release_or_codename";
        public static readonly string VERSION_SDK_PRODUCT = "ro.product.build.version.sdk";
        public static readonly string BRAND_PRODUCT = "ro.product.product.brand";
        public static readonly string DEVICE_PRODUCT = "ro.product.product.device";
        public static readonly string MANUFACTURER_PRODUCT = "ro.product.product.manufacturer";
        public static readonly string MODEL_PRODUCT = "ro.product.product.model";
        public static readonly string PRODUCT_PRODUCT = "ro.product.product.name";

        public static readonly string BUILD_PDA = "ro.build.PDA";

        public static readonly string BUILD_DATE_ODM = "ro.odm.build.date";
        public static readonly string BUILD_DATE_UTC_ODM = "ro.odm.build.date.utc";
        public static readonly string FINGERPRINT_ODM = "ro.odm.build.fingerprint";
        public static readonly string BUILD_ID_ODM = "ro.odm.build.id";
        public static readonly string BUILD_TAGS_ODM = "ro.odm.build.tags";
        public static readonly string TYPE_ODM = "ro.odm.build.type";
        public static readonly string BUILD_INCREMENTAL_ODM = "ro.odm.build.version.incremental";
        public static readonly string VERSION_RELEASE_ODM = "ro.odm.build.version.release";
        public static readonly string VERSION_SDK_ODM = "ro.odm.build.version.sdk";
        public static readonly string BRAND_ODM = "ro.product.odm.brand";
        public static readonly string MANUFACTURER_ODM = "ro.product.odm.manufacturer";
        public static readonly string MODEL_ODM = "ro.product.odm.model";
        public static readonly string PRODUCT_ODM = "ro.product.odm.name";
        public static readonly string DEVICE_ODM = "ro.product.odm.device";

    }
    class BuildKey_SYSTEM_SARGO
    {
        public static readonly string SYSTEM_BUILD_DATE = "ro.system.build.date";
        public static readonly string SYSTEM_BUILD_DATE_UTC = "ro.system.build.date.utc";
        public static readonly string SYSTEM_BUILD_FINGERPRINT = "ro.system.build.fingerprint";
        public static readonly string SYSTEM_BUILD_ID = "ro.system.build.id";
        public static readonly string SYSTEM_BUILD_TAGS = "ro.system.build.tags";
        public static readonly string SYSTEM_BUILD_TYPE = "ro.system.build.type";
        public static readonly string SYSTEM_BUILD_VERSION_INCREMENTAL = "ro.system.build.version.incremental";

        public static readonly string PRODUCT_SYSTEM_BRAND = "ro.product.system.brand";
        public static readonly string PRODUCT_SYSTEM_DEVICE = "ro.product.system.device";
        public static readonly string PRODUCT_SYSTEM_MANUFACTURER = "ro.product.system.manufacturer";
        public static readonly string PRODUCT_SYSTEM_MODEL = "ro.product.system.model";
        public static readonly string PRODUCT_SYSTEM_NAME = "ro.product.system.name";

        public static readonly string PRODUCT_NAME = "ro.product.name";
        public static readonly string PRODUCT_BRAND = "ro.product.brand";
        public static readonly string PRODUCT_DEVICE = "ro.product.device";
        public static readonly string PRODUCT_MODEL = "ro.product.model";
        public static readonly string PRODUCT_MANUFACTURER = "ro.product.manufacturer";
        public static readonly string CUSTOM_HARDWARE = "xspoof.hardware";
        public static readonly string BOOT_HARDWARE = "ro.boot.hardware";
        public static readonly string CUSTOM_SERIALNO = "xspoof.serialno";
        public static readonly string BOOT_SERIALNO = "ro.boot.serialno";
        public static readonly string CUSTOM_BASEBAND = "xspoof.baseband";

        public static readonly string BUILD_DATE = "ro.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.build.date.utc";
        public static readonly string BUILD_FINGERPRINT = "ro.build.fingerprint";
        public static readonly string BUILD_ID = "ro.build.id";
        public static readonly string BUILD_TAGS = "ro.build.tags";
        public static readonly string BUILD_TYPE = "ro.build.type";
        public static readonly string BUILD_VERSION_INCREMENTAL = "ro.build.version.incremental";

        public static readonly string BUILD_DISPLAY_ID = "ro.build.display.id";
        public static readonly string BUILD_VERSION_SECURITY_PATH = "ro.build.version.security_patch";
        public static readonly string BUILD_USER = "ro.build.user";
        public static readonly string BUILD_HOST = "ro.build.host";
        public static readonly string BUILD_FLAVOR = "ro.build.flavor";
        public static readonly string BUILD_PRODUCT = "ro.build.product";
        public static readonly string BUILD_DESCRIPTION = "ro.build.description";

        public static readonly string LINEAGE_DEVICE = "ro.lineage.device";
        public static readonly string LINEAGE_VERSION = "ro.lineage.version";
        public static readonly string LINEAGE_DISPLAY_VERSION = "ro.lineage.display.version";
        public static readonly string MOD_VERSION = "ro.modversion";
        //Wifi mac address
        public static readonly string WIFI_MAC_ADDRESS = "ro.lineage.wifi";
        public static readonly string WIFI_MAC_ADDRESS_ANDROID = "ro.android.wifi";
        //BlueTooth mac address
        public static readonly string BLUETOOTH_MAC_ADDRESS = "ro.lineage.bluetooth";
        public static readonly string BLUETOOTH_MAC_ADDRESS_ANDROID = "ro.android.bluetooth"; 
        public static readonly string BSSID = "ro.lineage.BSSID";
        public static readonly string SSID_ANDROID = "ro.android.SSID";
        public static readonly string BSSID_ANDROID = "ro.android.BSSID";

    }

    class BuildKey_VENDOR_SARGO
    {
        public static readonly string BUILD_DATE = "ro.vendor.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.vendor.build.date.utc";
        public static readonly string BUILD_FINGERPRINT = "ro.vendor.build.fingerprint";
        public static readonly string BUILD_ID = "ro.vendor.build.id";
        public static readonly string BUILD_TAGS = "ro.vendor.build.tags";
        public static readonly string BUILD_TYPE = "ro.vendor.build.type";
        public static readonly string BUILD_VERSION_INCREMENTAL = "ro.vendor.build.version.incremental";

        public static readonly string PRODUCT_BRAND = "ro.product.vendor.brand";
        public static readonly string PRODUCT_DEVICE = "ro.product.vendor.device";
        public static readonly string PRODUCT_MANUFACTURER = "ro.product.vendor.manufacturer";
        public static readonly string PRODUCT_MODEL = "ro.product.vendor.model";
        public static readonly string PRODUCT_NAME = "ro.product.vendor.name";

        public static readonly string BUILD_SECURITY_PATH = "ro.vendor.build.security_patch";
        public static readonly string PRODUCT_BOARD = "ro.product.board";
        public static readonly string BOARD_PLATFORM = "ro.board.platform";
        public static readonly string BOOT_BUILD_DATE = "ro.bootimage.build.date";
        public static readonly string BOOT_BUILD_DATE_UTC = "ro.bootimage.build.date.utc";
        public static readonly string BOOT_BUILD_FINGERPRINT = "ro.bootimage.build.fingerprint";
    }

    class BuildKey_SYSTEMEXT_SARGO
    {
        public static readonly string BUILD_DATE = "ro.system_ext.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.system_ext.build.date.utc";
        public static readonly string BUILD_FINGERPRINT = "ro.system_ext.build.fingerprint";
        public static readonly string BUILD_ID = "ro.system_ext.build.id";
        public static readonly string BUILD_TAGS = "ro.system_ext.build.tags";
        public static readonly string BUILD_TYPE = "ro.system_ext.build.type";
        public static readonly string BUILD_VERSION_INCREMENTAL = "ro.system_ext.build.version.incremental";

        public static readonly string PRODUCT_BRAND = "ro.product.system_ext.brand";
        public static readonly string PRODUCT_DEVICE = "ro.product.system_ext.device";
        public static readonly string PRODUCT_MANUFACTURER = "ro.product.system_ext.manufacturer";
        public static readonly string PRODUCT_MODEL = "ro.product.system_ext.model";
        public static readonly string PRODUCT_NAME = "ro.product.system_ext.name";
    }

    class BuildKey_ODM_SARGO
    {
        public static readonly string BUILD_DATE = "ro.odm.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.odm.build.date.utc";
        public static readonly string BUILD_FINGERPRINT = "ro.odm.build.fingerprint";
        public static readonly string BUILD_ID = "ro.odm.build.id";
        public static readonly string BUILD_TAGS = "ro.odm.build.tags";
        public static readonly string BUILD_TYPE = "ro.odm.build.type";
        public static readonly string BUILD_VERSION_INCREMENTAL = "ro.odm.build.version.incremental";

        public static readonly string PRODUCT_BRAND = "ro.product.odm.brand";
        public static readonly string PRODUCT_DEVICE = "ro.product.odm.device";
        public static readonly string PRODUCT_MANUFACTURER = "ro.product.odm.manufacturer";
        public static readonly string PRODUCT_MODEL = "ro.product.odm.model";
        public static readonly string PRODUCT_NAME = "ro.product.odm.name";
    }

    class BuildKey_PRODUCT_SARGO
    {
        public static readonly string BUILD_DATE = "ro.product.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.product.build.date.utc";
        public static readonly string BUILD_FINGERPRINT = "ro.product.build.fingerprint";
        public static readonly string BUILD_ID = "ro.product.build.id";
        public static readonly string BUILD_TAGS = "ro.product.build.tags";
        public static readonly string BUILD_TYPE = "ro.product.build.type";
        public static readonly string BUILD_VERSION_INCREMENTAL = "ro.product.build.version.incremental";

        public static readonly string PRODUCT_BRAND = "ro.product.product.brand";
        public static readonly string PRODUCT_DEVICE = "ro.product.product.device";
        public static readonly string PRODUCT_MANUFACTURER = "ro.product.product.manufacturer";
        public static readonly string PRODUCT_MODEL = "ro.product.product.model";
        public static readonly string PRODUCT_NAME = "ro.product.product.name";
    }
    class BuildKey_SYSTEM
    {
        public static readonly string HARDWARE = "ro.lineage.hardware";
        public static readonly string TYPE = "ro.build.type";
        public static readonly string BRAND = "ro.product.brand";
        //public static readonly string DEVICE_ID = "";
        public static readonly string BOARD = "ro.product.board";
        public static readonly string USER = "ro.build.user";
        public static readonly string MODEL = "ro.product.model";
        public static readonly string BUILD_PRODUCT = "ro.build.product";

        public static readonly string MODEL_LINEAGE = "ro.lineage.device";
        public static readonly string HARDWARE_SERIAL = "ro.boot.serialno";
        public static readonly string VENDOR = "ro.product.manufacturer";
        public static readonly string SIM_SERIAL = "";

        public static readonly string SIM_OPERATOR_CODE = "gsm.sim.operator.numeric";
        public static readonly string SIM_OPERATOR_CODE_COUNTRY_ISO = "gsm.sim.operator.iso-country";
        public static readonly string SIM_OPERATOR_ALPHA = "gsm.sim.operator.alpha";
        public static readonly string NETWORK_OPERATOR_CODE = "gsm.operator.numeric";
        //code name
        public static readonly string DEVICE = "ro.product.device";
        //fingerprint
        public static readonly string FINGERPRINT = "ro.build.fingerprint";

        //build ID
        public static readonly string BUILD_ID = "ro.build.id";
        //version release
        public static readonly string VERSION_RELEASE = "ro.build.version.release";
        //sdk
        public static readonly string VERSION_SDK = "ro.build.version.sdk";
        //security_path
        public static readonly string SECURITY_PATH = "ro.build.version.security_patch";
        //Builder
        public static readonly string BUILD_HOST = "ro.build.host";
        //Bootloader
        public static readonly string BOOT_LOADER = "ro.bootloader";
        public static readonly string BOOT_BOOT_LOADER = "ro.boot.bootloader";
        public static readonly string BOOT_LOADER_LINEAGE = "ro.lineage.bootloader";

        public static readonly string GSM_BASEBAND = "gsm.version.baseband";
        public static readonly string BASEBAND = "ro.lineage.gsm.version.baseband";
        // LINEAGE version OS
        public static readonly string LINEAGE_VERSION = "ro.lineage.version";
        public static readonly string LINEAGE_DISPLAY_VERSION = "ro.lineage.display.version";

        public static readonly string LINEAGE_BUILD_VERSION = "ro.lineage.build.version";
        public static readonly string LINEAGE_BUILD_VERSION_PLAT_SDK = "ro.lineage.build.version.plat.sdk";


        //Wifi mac address
        public static readonly string WIFI_MAC_ADDRESS = "ro.lineage.wifi";
        //BlueTooth mac address
        public static readonly string BLUETOOTH_MAC_ADDRESS = "ro.lineage.bluetooth";

        public static readonly string PRODUCT = "ro.product.name";
        public static readonly string PLATFORM = "ro.board.platform";
        public static readonly string BUILD_DISPLAY_ID = "ro.build.display.id";
        public static readonly string BUILD_TAGS = "ro.build.tags";
        public static readonly string BUILD_INCREMENTAL = "ro.build.version.incremental";
        public static readonly string BUILD_DESCRIPTION = "ro.build.description";
        public static readonly string BUILD_DATE = "ro.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.build.date.utc";
        public static readonly string BUILD_FLAVOR = "ro.build.flavor";

        public static readonly string SSID = "ro.lineage.SSID";
        public static readonly string BSSID = "ro.lineage.BSSID";
    }

    class BuildKey_VENDOR
    {
        public static readonly string TYPE = "ro.vendor.build.type";
        public static readonly string BRAND = "ro.product.vendor.brand";
        public static readonly string BOARD = "ro.product.board";
        public static readonly string PLATFORM = "ro.board.platform";
        public static readonly string MODEL = "ro.product.vendor.model";
        public static readonly string VENDOR = "ro.product.vendor.manufacturer";
        //code name
        public static readonly string DEVICE = "ro.product.vendor.device";
        public static readonly string NAME = "ro.product.vendor.name";


        public static readonly string FINGERPRINT = "ro.vendor.build.fingerprint";
        public static readonly string FINGERPRINT_BOOT_IMAGE = "ro.bootimage.build.fingerprint";
        public static readonly string SECURITY_PATH = "ro.vendor.build.security_patch";//ro.vendor.build.security_patch
        public static readonly string BUILD_ID = "ro.vendor.build.id";
        public static readonly string BUILD_TAGS = "ro.vendor.build.tags";
        public static readonly string BUILD_INCREMENTAL = "ro.vendor.build.version.incremental";
        public static readonly string BUILD_DATE = "ro.vendor.build.date";
        public static readonly string BUILD_DATE_UTC = "ro.vendor.build.date.utc";

        public static readonly string BUILD_VERSION_RELEASE = "ro.vendor.build.version.release";
        public static readonly string BUILD_VERSION_SDK = "ro.vendor.build.version.sdk";
    }

    class SystemSetting
    {
        public static readonly string UPDATE_URI_LOCAL = @"C:\Users\Tran Mai\source\repos\Jackychans\MiChanger\Releases"; // for debug environment

        public static readonly string UPDATE_URI_ONEDRIVE = "https://onedrive.live.com/?cid=663db2e04f9a5a5d&id=663DB2E04F9A5A5D%21231708&authkey=!ANdhX7ja2JDSpUg";

        public static readonly string UPDATE_URI_AWS = "https://michanger.s3-ap-southeast-1.amazonaws.com/";
    }

    class DebuggableMode
    {
        public static readonly string PRODUCTION = "0";
        public static readonly string DEBUG = "1";
    }


}
