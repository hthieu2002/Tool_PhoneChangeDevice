
using DeepDroid.Models;
using Newtonsoft.Json;
using Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToolChange.ViewModels;
using ToolChange.ViewModels.Constants;

namespace ToolChange.Services
{
    public class Util
    {
        public static bool checkSim;

        public static bool SaveDeviceInfo(DeviceViewModel model, ObservableCollection<Models.DeviceModel> deviceS, POCO.Models.DeviceModel tempDevice, string deviceId, string applicationPath, bool isFakeSim = false, bool isAutoUpdatePif = false, bool keepBrand = false, bool isFakeSdk = true)
        {
            try
            {
                if (keepBrand)
                {
                    var value = CheckManuAndBrand(deviceId);

                    tempDevice.Manufacturer = value.manufacturer;
                    tempDevice.Brand = value.brand;
                }

                if (tempDevice.Brand == null || tempDevice.Brand == "")
                {
                    tempDevice.Brand = tempDevice.Manufacturer.ToLower();
                }

                Regex IsoDateRegex =
                    new Regex(@"^(?<y>\d{4})-(?<m>0[1-9]|1[0-2])-(?<d>0[1-9]|[12]\d|3[01])$",
                    RegexOptions.Compiled);

                if (!IsoDateRegex.IsMatch(tempDevice.SecurityPath))
                {
                    tempDevice.SecurityPath = DateTime.Now.ToString("yyyy-MM-dd");
                }

                Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "10%", "Change device ...");
                if (tempDevice == null)
                {
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "10%", "Null error");
                    return false;
                }

                string isRomPixelExperience = ADBService.getProp("org.pixelexperience.device", deviceId);

                string securityPatchProp = ADBService.getProp("persist.sys.pihooks_SECURITY_PATCH", deviceId);

                if (!string.IsNullOrEmpty(isRomPixelExperience))
                {
                    securityPatchProp = ADBService.getProp("persist.sys.deepdroid.pihooks_SECURITY_PATCH", deviceId);
                }

                string roBuildDate = RandomService.generateBuildDate(securityPatchProp);
                string roBuildDateUtc = RandomService.generateBuildDateUTC(securityPatchProp);
                tempDevice.BuildDate = roBuildDate;
                tempDevice.BuildDateUtc = roBuildDateUtc;

                string bluetoothName = RandomService.generateName();
                string deviceName = RandomService.generateName();

                if (ADBService.getDeviceStatus(deviceId) == DeviceStatus.ReadyToChange)
                {
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "15%", "Change device ...");
                    ADBService.rootAndRemount(deviceId);

                    if (isAutoUpdatePif)
                    {
                        if (!string.IsNullOrEmpty(isRomPixelExperience))
                        {
                            autoUpdatePif(deviceId, "deepdroid");
                        }
                        else
                        {
                            autoUpdatePif(deviceId, "evolution");
                        }
                    }

                    ADBService.shellRemoveIfContainSpecificText("/system/build.prop", "product is obsolete", deviceId);
                    var changedSystemInfo = new Dictionary<string, string>();
                    var tempBaseband = string.IsNullOrEmpty(tempDevice.Baseband) ? tempDevice.BuildIncremental : tempDevice.Baseband;
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "25%", "Change information ");
                    var lineageVersion = RandomService.generateLineageOsVersion(tempDevice.Release) + "-" + tempDevice.Code;
                    var randomUser = RandomService.generateUser();
                    changedSystemInfo.Add("ro.build.type", "user");
                    changedSystemInfo.Add("ro.build.tags", "release-keys");
                    changedSystemInfo.Add("ro.build.use", randomUser);
                    changedSystemInfo.Add("ro.build.product", tempDevice.Product);
                    changedSystemInfo.Add("ro.build.fingerprint", tempDevice.Fingerprint);
                    changedSystemInfo.Add("ro.build.display.id", tempDevice.BuildDisplayId);
                    changedSystemInfo.Add("ro.build.host", randomUser);
                    changedSystemInfo.Add("ro.build.version.incremental", tempDevice.BuildIncremental);
                    changedSystemInfo.Add("ro.build.description", tempDevice.BuildDescription);
                    changedSystemInfo.Add("ro.build.date", tempDevice.BuildDate);
                    changedSystemInfo.Add("ro.build.date.utc", tempDevice.BuildDateUtc);
                    changedSystemInfo.Add("ro.build.flavor", tempDevice.BuildFlavor);
                    changedSystemInfo.Add("ro.build.id", tempDevice.BuildId);

                    changedSystemInfo.Add("ro.product.name", tempDevice.Name);
                    changedSystemInfo.Add("ro.product.brand", tempDevice.Brand);
                    changedSystemInfo.Add("ro.product.manufacturer", tempDevice.Manufacturer);
                    changedSystemInfo.Add("ro.product.model", tempDevice.Model);
                    changedSystemInfo.Add("ro.product.device", tempDevice.Code);

                    changedSystemInfo.Add("vendor.usb.product_string", tempDevice.Model);
                    changedSystemInfo.Add("ro.boot.hwname", tempDevice.Code);
                    changedSystemInfo.Add("ro.boot.hwdevice", tempDevice.Code);
                    changedSystemInfo.Add("ro.product.hardware.sku", tempDevice.Code);
                    changedSystemInfo.Add("ro.boot.product.hardware.sku", tempDevice.Code);

                    changedSystemInfo.Add("ro.android.device.mac", tempDevice.WifiMacAddress);
                    changedSystemInfo.Add("ro.android.bssid", RandomService.generateWifiMacAddress(tempDevice.Manufacturer.ToLower()));
                    changedSystemInfo.Add("ro.android.ssid", RandomService.generateSSID());
                    changedSystemInfo.Add("ro.android.bluetooth.mac", tempDevice.BlueToothMacAddress);
                    changedSystemInfo.Add("ro.android.bluetooth.name", bluetoothName);
                    changedSystemInfo.Add("ro.android.device.name", deviceName);
                    changedSystemInfo.Add("ro.android.id", tempDevice.AndroidId);
                    changedSystemInfo.Add("ro.android.serialno", tempDevice.SerialNo);
                    changedSystemInfo.Add("ro.android.imei", tempDevice.Imei);
                    changedSystemInfo.Add("ro.android.imei1", tempDevice.Imei1);
                    changedSystemInfo.Add("ro.android.bootloader", tempDevice.Bootloader);
                    changedSystemInfo.Add("ro.android.hardware", tempDevice.Hardware);
                    changedSystemInfo.Add("ro.android.platform", tempDevice.Platform);
                    changedSystemInfo.Add("ro.android.board", tempDevice.Board);

                    changedSystemInfo.Add("ro.android.soc.manufacturer", tempDevice.Manufacturer);
                    changedSystemInfo.Add("ro.android.soc.model", tempDevice.Hardware);
                    changedSystemInfo.Add("ro.android.build.version.security_patch", securityPatchProp);
                    changedSystemInfo.Add("ro.android.build.version.release", tempDevice.Release);
                    //changedSystemInfo.Add("ro.android.build.version.sdk", tempDevice.SDK);

                    changedSystemInfo.Add("ro.android.gsm.version.baseband", tempBaseband);
                    changedSystemInfo.Add("gsm.version.baseband", tempBaseband);
                    changedSystemInfo.Add("ro.com.google.clientidbase", $"android-{tempDevice.Brand}");
                    changedSystemInfo.Add("ro.debuggable", "0");

                    changedSystemInfo.Add("ro.boot.vbmeta.avb_version", "2.0");
                    changedSystemInfo.Add("ro.boot.vbmeta.hash_alg", "sha256");
                    changedSystemInfo.Add("ro.boot.vbmeta.size", "16384");
                    changedSystemInfo.Add("ro.boot.vbmeta.digest", RandomService.getRandomHex32Bytes());

                    changedSystemInfo.Add("keyguard.no_require_sim", "true");
                    changedSystemInfo.Add("debug.sf.enable_sdr_dimming", "1");
                    changedSystemInfo.Add("debug.sf.dim_in_gamma_in_enhanced_screenshots", "1");
                    changedSystemInfo.Add("ro.hardware.keystore_desede", "true");
                    changedSystemInfo.Add("ro.hardware.keystore", "trusty");
                    changedSystemInfo.Add("ro.hardware.gatekeeper", "trusty");
                    changedSystemInfo.Add("persist.vendor.enable.thermal.genl", "true");
                    changedSystemInfo.Add("ro.incremental.enable", "true");

                    string isRadioImeiAvailable = ADBService.getProp("persist.radio.imei1", deviceId);
                    if (!string.IsNullOrEmpty(isRadioImeiAvailable))
                    {
                        changedSystemInfo.Add("persist.radio.imei1", tempDevice.Imei);
                        changedSystemInfo.Add("persist.radio.imei2", tempDevice.Imei1);
                    }

                    string checkRilImeiAvailable = ADBService.getProp("ro.ril.imei0", deviceId);
                    if (!string.IsNullOrEmpty(checkRilImeiAvailable))
                    {
                        changedSystemInfo.Add("ro.ril.imei0", tempDevice.Imei);
                        changedSystemInfo.Add("ro.ril.imei1", tempDevice.Imei1);
                    }

                    string checkRilMiuiImeiAvailable = ADBService.getProp("ro.ril.miui.imei0", deviceId);
                    if (!string.IsNullOrEmpty(checkRilMiuiImeiAvailable))
                    {
                        changedSystemInfo.Add("ro.ril.miui.imei0", tempDevice.Imei);
                        changedSystemInfo.Add("ro.ril.miui.imei1", tempDevice.Imei1);
                    }

                    if (!string.IsNullOrEmpty(isRomPixelExperience))
                    {
                        changedSystemInfo.Add("org.pixelexperience.device", tempDevice.Code);
                    }

                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "45%", "save information ");
                    ADBService.replaceBuildProp("/system/build.prop", changedSystemInfo, deviceId);

                    var changedProductInfo = new Dictionary<string, string>();
                    changedProductInfo.Add("ro.com.google.clientidbase", $"android-{tempDevice.Brand}");
                    ADBService.replaceBuildProp("product/build.prop", changedProductInfo, deviceId);
                    ADBService.replaceBuildProp("product/etc/build.prop", changedProductInfo, deviceId);

                    var changedVendorInfo = new Dictionary<string, string>();
                    changedVendorInfo.Add("ro.soc.manufacturer", tempDevice.Manufacturer);
                    changedVendorInfo.Add("ro.soc.model", tempDevice.Hardware);
                    changedVendorInfo.Add("ro.product.board", tempDevice.Board);
                    changedVendorInfo.Add("bluetooth.device.default_name", bluetoothName);
                    ADBService.replaceBuildProp("vendor/build.prop", changedVendorInfo, deviceId);
                    ADBService.replaceBuildProp("mnt/scratch/overlay/vendor/upper/build.prop", changedVendorInfo, deviceId);

                    Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
                    partitionList.Add("system", new List<string> { "/system/build.prop" });
                    partitionList.Add("bootimage", new List<string> { "/system/build.prop" });
                    partitionList.Add("vendor", new List<string> { "/vendor/build.prop" });
                    partitionList.Add("product", new List<string> { "/product/build.prop", "/product/etc/build.prop" });
                    partitionList.Add("odm", new List<string> { "/odm/etc/build.prop" });
                    partitionList.Add("odm_dlkm", new List<string> { "/vendor/odm_dlkm/etc/build.prop" });
                    partitionList.Add("vendor_dlkm", new List<string> { "/vendor_dlkm/etc/build.prop" });
                    partitionList.Add("system_dlkm", new List<string> { "/system_dlkm/etc/build.prop", "/system/system_dlkm/etc/build.prop" });
                    partitionList.Add("system_ext", new List<string> { "/system_ext/etc/build.prop", "/system/system_ext/etc/build.prop" });
                    RepleacePropertiesForPartition(tempDevice, partitionList, deviceId);

                    //ADBService.deleteSetting("android_id", deviceId, "secure");
                    //ADBService.deleteSetting("bluetooth_address", deviceId, "secure");
                    //ADBService.deleteSetting("bluetooth_name", deviceId, "secure");
                    //ADBService.deleteSetting("device_name", deviceId);
                    ADBService.putSetting("android_id", tempDevice.AndroidId, deviceId, "global");
                    ADBService.putSetting("android_id", tempDevice.AndroidId, deviceId, "secure");
                    ADBService.putSetting("android_id", tempDevice.AndroidId, deviceId, "system");
                    ADBService.putSetting("bluetooth_address", tempDevice.BlueToothMacAddress, deviceId, "secure");
                    ADBService.putSetting("bluetooth_name", bluetoothName, deviceId, "secure");
                    ADBService.putSetting("device_name", deviceName, deviceId);
                    ADBService.putSetting("mi_mac_address", tempDevice.WifiMacAddress, deviceId);
                    ADBService.putSetting("non_persistent_mac_randomization_force_enabled", "1", deviceId);
                    ADBService.putSetting("screen_off_timeout", "1800000", deviceId, "system");
                    ADBService.runCMDRoot("shell locksettings set-disabled true", deviceId);

                    ADBService.putSetting(GlobalAndroidSettings.IMEI0, tempDevice.Imei, deviceId);
                    ADBService.putSetting(GlobalAndroidSettings.IMEI1, tempDevice.Imei1, deviceId);
                    ADBService.putSetting(GlobalAndroidSettings.HARDWARE_SERIALNO, tempDevice.SerialNo, deviceId);

                    ADBService.updateInitRc(tempDevice, randomUser, deviceId);
                    ADBService.fakeLocalHostNameV6(deviceId);

                    // fake wifi mac address
                    ADBService.fakeWifiMacAddress(tempDevice.WifiMacAddress, deviceId);

                    ADBService.runCMDRoot($"shell setprop persist.sys.sim.ready {isFakeSim.ToString().ToLower()}", deviceId);
                    if (isFakeSim)
                    {

                        Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "75%", "Fake sim .. ");
                        // setting sim card
                        ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_NUMERIC, tempDevice.SimOperatorNumeric, deviceId); // set sim numeric e.g. 42503
                        ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_COUNTRY, tempDevice.SimOperatorCountry, deviceId); // set country of operator code
                        ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_NAME, tempDevice.SimOperatorName, deviceId); // set carrier name of current sim operator

                        ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NUMERIC, tempDevice.SimOperatorNumeric, deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_COUNTRY, tempDevice.SimOperatorCountry, deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NAME, tempDevice.SimOperatorName, deviceId);
                        // setting phone number, ICCID, IMSI
                        ADBService.putSetting(GlobalAndroidSettings.SIM_PHONE_NUMBER, tempDevice.SimPhoneNumber, deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.ICCID, tempDevice.ICCID, deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.IMSI, tempDevice.IMSI, deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.SIM_STATE_READY, "1", deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.SIM_ICC_AVAILABLE, "1", deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.SIM_STATE, "5", deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.NETWORK_TYPE, "13", deviceId);

                        //public static readonly string DATA_ACTIVITY = string.Concat(MI_PREFIX, "data_activity");
                        //public static readonly string DATA_STATE = string.Concat(MI_PREFIX, "data_state");
                        //public static readonly string DATA_NETWORK_TYPE = string.Concat(MI_PREFIX, "data_network_type");
                        ADBService.putSetting(GlobalAndroidSettings.DATA_NETWORK_TYPE, "13", deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.DATA_STATE, "2", deviceId);
                        ADBService.putSetting(GlobalAndroidSettings.DATA_ACTIVITY, "4", deviceId);
                    }
                    else
                    {
                        Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "75%", "Change device .. ");
                        // setting sim card
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_OPERATOR_NUMERIC, deviceId); // set sim numeric e.g. 42503
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_OPERATOR_COUNTRY, deviceId); // set country of operator code
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_OPERATOR_NAME, deviceId); // set carrier name of current sim operator

                        ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NUMERIC, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_OPERATOR_COUNTRY, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NAME, deviceId);
                        // setting phone number, ICCID, IMSI
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_PHONE_NUMBER, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.ICCID, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.IMSI, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_STATE_READY, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_ICC_AVAILABLE, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.SIM_STATE, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_TYPE, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.DATA_NETWORK_TYPE, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.DATA_STATE, deviceId);
                        ADBService.deleteSetting(GlobalAndroidSettings.DATA_ACTIVITY, deviceId);
                    }
                    Console.WriteLine("3.DONE put setting");
                    return true;

                }
                else
                {
                    model.UpdateDeviceStatus(deviceId, "0%", "Error");
                    return false;
                }
            }
            catch (Exception ex)
            {
                model.UpdateDeviceStatus(deviceId, "0%", "Error change");
                return false;
            }
        }
        public static (string brand, string manufacturer) CheckManuAndBrand(string deviceId)
        {
            string brand = RunAdbCommand($"-s {deviceId} shell getprop ro.product.vendor.brand");
            string manufacturer = RunAdbCommand($"-s {deviceId} shell getprop ro.product.vendor.manufacturer");

            // Nếu các giá trị trên trả về rỗng (có thể do build.prop bị sửa), thử lấy từ các property khác
            if (string.IsNullOrWhiteSpace(brand))
                brand = RunAdbCommand($"-s {deviceId} shell getprop ro.product.system.brand");

            if (string.IsNullOrWhiteSpace(manufacturer))
                manufacturer = RunAdbCommand($"-s {deviceId} shell getprop ro.product.system.manufacturer");

            // Có thể fallback cuối cùng
            if (string.IsNullOrWhiteSpace(brand))
                brand = RunAdbCommand($"-s {deviceId} shell getprop ro.product.brand");

            if (string.IsNullOrWhiteSpace(manufacturer))
                manufacturer = RunAdbCommand($"-s {deviceId} shell getprop ro.product.manufacturer");

            return (brand.Trim(), manufacturer.Trim());
        }

        private static string RunAdbCommand(string arguments)
        {
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "adb";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        public static bool SaveDeviceSIm(POCO.Models.DeviceModel tempDevice, string deviceId, string applicationPath)
        {
            try
            {


                Debug.WriteLine($"{tempDevice.SimOperatorCountry} \n {tempDevice.SimOperatorName} \n {tempDevice.SimOperatorNumeric}" +
                    $"\n {tempDevice.SimPhoneNumber} ");
                ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_NUMERIC, tempDevice.SimOperatorNumeric, deviceId); // set sim numeric e.g. 42503

                ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_COUNTRY, tempDevice.SimOperatorCountry, deviceId); // set country of operator code

                ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_NAME, tempDevice.SimOperatorName, deviceId); // set carrier name of current sim operator

                ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NUMERIC, tempDevice.SimOperatorNumeric, deviceId);

                ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_COUNTRY, tempDevice.SimOperatorCountry, deviceId);

                ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NAME, tempDevice.SimOperatorName, deviceId);

                // setting phone number, ICCID, IMSI
                ADBService.putSetting(GlobalAndroidSettings.SIM_PHONE_NUMBER, tempDevice.SimPhoneNumber, deviceId);

                ADBService.putSetting(GlobalAndroidSettings.ICCID, tempDevice.ICCID, deviceId);

                ADBService.putSetting(GlobalAndroidSettings.IMSI, tempDevice.IMSI, deviceId);

                ADBService.putSetting(GlobalAndroidSettings.SIM_STATE_READY, "1", deviceId);

                ADBService.putSetting(GlobalAndroidSettings.SIM_ICC_AVAILABLE, "1", deviceId);

                ADBService.putSetting(GlobalAndroidSettings.SIM_STATE, "5", deviceId);

                ADBService.putSetting(GlobalAndroidSettings.NETWORK_TYPE, "13", deviceId);

                //public static readonly string DATA_ACTIVITY = string.Concat(MI_PREFIX, "data_activity");
                //public static readonly string DATA_STATE = string.Concat(MI_PREFIX, "data_state");
                //public static readonly string DATA_NETWORK_TYPE = string.Concat(MI_PREFIX, "data_network_type");
                ADBService.putSetting(GlobalAndroidSettings.DATA_NETWORK_TYPE, "13", deviceId);

                ADBService.putSetting(GlobalAndroidSettings.DATA_STATE, "2", deviceId);

                ADBService.putSetting(GlobalAndroidSettings.DATA_ACTIVITY, "4", deviceId);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }


        }
        public static void FakeSimInfo(POCO.Models.DeviceModel tempDevice, string deviceId, bool isFakeSim)
        {
            if (isFakeSim)
            {
                // setting sim card
                ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_NUMERIC, tempDevice.SimOperatorNumeric, deviceId); // set sim numeric e.g. 42503
                ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_COUNTRY, tempDevice.SimOperatorCountry, deviceId); // set country of operator code
                ADBService.putSetting(GlobalAndroidSettings.SIM_OPERATOR_NAME, tempDevice.SimOperatorName, deviceId); // set carrier name of current sim operator

                ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NUMERIC, tempDevice.SimOperatorNumeric, deviceId);
                ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_COUNTRY, tempDevice.SimOperatorCountry, deviceId);
                ADBService.putSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NAME, tempDevice.SimOperatorName, deviceId);

                // setting phone number, ICCID, IMSI
                ADBService.putSetting(GlobalAndroidSettings.SIM_PHONE_NUMBER, tempDevice.SimPhoneNumber, deviceId);
                ADBService.putSetting(GlobalAndroidSettings.ICCID, tempDevice.ICCID, deviceId);
                ADBService.putSetting(GlobalAndroidSettings.IMSI, tempDevice.IMSI, deviceId);
                //ADBService.putSetting(GlobalAndroidSettings.SIM_STATE_READY, "5", deviceId);
                ADBService.putSetting(GlobalAndroidSettings.SIM_ICC_AVAILABLE, "1", deviceId);
                ADBService.putSetting(GlobalAndroidSettings.SIM_STATE, "5", deviceId);
                ADBService.putSetting(GlobalAndroidSettings.NETWORK_TYPE, "13", deviceId);

                ADBService.putSetting(GlobalAndroidSettings.DATA_NETWORK_TYPE, "13", deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.DATA_STATE, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.DATA_ACTIVITY, deviceId);
            }
            else
            {
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_OPERATOR_NUMERIC, deviceId); // set sim numeric e.g. 42503
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_OPERATOR_COUNTRY, deviceId); // set country of operator code
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_OPERATOR_NAME, deviceId); // set carrier name of current sim operator

                ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NUMERIC, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_OPERATOR_COUNTRY, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_OPERATOR_NAME, deviceId);

                // setting phone number, ICCID, IMSI
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_PHONE_NUMBER, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.ICCID, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.IMSI, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_STATE_READY, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_ICC_AVAILABLE, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.SIM_STATE, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.NETWORK_TYPE, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.DATA_NETWORK_TYPE, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.DATA_STATE, deviceId);
                ADBService.deleteSetting(GlobalAndroidSettings.DATA_ACTIVITY, deviceId);
            }
        }
        private static void RepleacePropertiesForPartition(POCO.Models.DeviceModel tempDevice, Dictionary<string, List<string>> partitions, string deviceId)
        {
            foreach (var partition in partitions)
            {
                Console.WriteLine($"*******START Partition {partition.Key}*******");

                var changedDeviceInfo = new Dictionary<string, string>
                {
                    [$"ro.product.{partition.Key}.brand"] = tempDevice.Brand,
                    [$"ro.product.{partition.Key}.device"] = tempDevice.Code,
                    [$"ro.product.{partition.Key}.manufacturer"] = tempDevice.Manufacturer,
                    [$"ro.product.{partition.Key}.model"] = tempDevice.Model,
                    [$"ro.product.{partition.Key}.name"] = tempDevice.Code,
                    [$"ro.{partition.Key}.build.date"] = tempDevice.BuildDate,
                    [$"ro.{partition.Key}.build.date.utc"] = tempDevice.BuildDateUtc,
                    [$"ro.{partition.Key}.build.fingerprint"] = tempDevice.Fingerprint,
                    [$"ro.{partition.Key}.build.id"] = tempDevice.BuildId,
                    [$"ro.{partition.Key}.build.tags"] = "release-keys",
                    [$"ro.{partition.Key}.build.type"] = "user",
                    [$"ro.{partition.Key}.build.version.incremental"] = tempDevice.BuildIncremental
                    // [$"ro.{partition.Key}.build.version.release"] = tempDevice.Release,
                    // [$"ro.{partition.Key}.build.version.release_or_codename"] = tempDevice.Release,
                    // [$"ro.{partition.Key}.build.version.sdk"] = tempDevice.SDK

                };

                foreach (var path in partition.Value)
                {
                    Console.WriteLine($"--- Replacing properties in {path}");
                    ADBService.replaceBuildProp(path, changedDeviceInfo, deviceId);
                }

                Console.WriteLine($"*******END Partition {partition.Key}*******");
            }
        }
        //public static string generateCertSubject()
        //{
        //    var listCitiesNewYork = new string[] {"Los Angeles",
        //    "New York",
        //    "Buffalo",
        //    "Rochester",
        //    "Yonkers",
        //    "Syracuse",
        //    "Albany",
        //    "New Rochelle",
        //    "Mount Vernon",
        //    "Schenectady",
        //    "Utica",
        //    "White Plains",
        //    "Hempstead",
        //    "Troy",
        //    "Niagara Falls",
        //    "Binghamton",
        //    "Freeport",
        //    "Valley Stream" };
        //    var randomCity1 = RandomService.randomInRange(0, listCitiesNewYork.Length);
        //    var randomCity2 = RandomService.randomInRange(0, listCitiesNewYork.Length);
        //    var randomEmail = RandomService.generateRandomHostName();
        //    return $"CN=Android, OU=Android, O={listCitiesNewYork[randomCity1]} Inc., L={listCitiesNewYork[randomCity2]}, ST=New York, C=US, emailAddress={randomEmail}@yahoo.com";
        //}

        public static void autoUpdatePif(string deviceId, string deviceType = "evolution")
        {
            string pifUrl = "https://raw.githubusercontent.com/doanvtamhuynh/database/main/pif.json";
            using HttpClient client = new HttpClient();
            string pifJson = "";
            try
            {
                pifJson = client.GetStringAsync(pifUrl)
                                    .GetAwaiter()
                                    .GetResult();

                if (!string.IsNullOrWhiteSpace(pifJson))
                {
                    List<PifData> pifList = JsonConvert.DeserializeObject<List<PifData>>(pifJson);

                    if (pifList == null || pifList.Count == 0)
                        return;

                    PifData pifData = pifList[RandomService.randomInRange(0, pifList.Count)];

                    if (pifData != null)
                    {
                        var props = typeof(PifData).GetProperties()
                                                    .Where(p => p.PropertyType == typeof(string))
                                                    .Select(p => new { Name = p.Name, Value = p.GetValue(pifData) as string })
                                                    .Where(p => string.IsNullOrEmpty(p.Value) && p.Name != "RELEASE")
                                                    .ToList();

                        if (!props.Any())
                        {
                            if (!string.IsNullOrEmpty(pifData.FINGERPRINT))
                            {
                                string[] parts = pifData.FINGERPRINT.Split("/");
                                List<string> splitFingerprint = new List<string>();
                                foreach (string part in parts)
                                {
                                    string[] subParts = part.Split(':');
                                    splitFingerprint.AddRange(subParts);
                                }

                                if (splitFingerprint.Count == 8)
                                {
                                    var changePifInfo = new Dictionary<string, string>();

                                    if (deviceType.Contains("deepdroid"))
                                    {
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_TYPE}", "user");
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_TAGS}", "release-keys");
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_BRAND}", splitFingerprint[0]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_PRODUCT}", splitFingerprint[1]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_DEVICE}", splitFingerprint[2]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_BOARD}", splitFingerprint[2]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_HARDWARE}", splitFingerprint[2]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_ID}", splitFingerprint[4]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_INCREMENTAL}", splitFingerprint[5]);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_FINGERPRINT}", pifData.FINGERPRINT);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_MANUFACTURER}", pifData.MANUFACTURER);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_MODEL}", $"\"{pifData.MODEL}\"");
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_SECURITY_PATCH}", pifData.SECURITY_PATCH);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_DEVICE_INITIAL_SDK_INT}", pifData.DEVICE_INITIAL_SDK_INT);
                                        changePifInfo.Add($"{PifKey.PIF_DEEPDROID_SDK_INT}", pifData.SDK_INT);
                                        if (!string.IsNullOrEmpty(pifData.RELEASE))
                                            changePifInfo.Add($"{PifKey.PIF_DEEPDROID_RELEASE}", pifData.RELEASE);
                                        else
                                        {
                                            changePifInfo.Add($"{PifKey.PIF_DEEPDROID_RELEASE}", splitFingerprint[3]);
                                        }
                                    }
                                    else
                                    {
                                        changePifInfo.Add($"{PifKey.PIF_EVO_TYPE}", "user");
                                        changePifInfo.Add($"{PifKey.PIF_EVO_TAGS}", "release-keys");
                                        changePifInfo.Add($"{PifKey.PIF_EVO_BRAND}", splitFingerprint[0]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_PRODUCT}", splitFingerprint[1]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_DEVICE}", splitFingerprint[2]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_BOARD}", splitFingerprint[2]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_HARDWARE}", splitFingerprint[2]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_ID}", splitFingerprint[4]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_INCREMENTAL}", splitFingerprint[5]);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_FINGERPRINT}", pifData.FINGERPRINT);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_MANUFACTURER}", pifData.MANUFACTURER);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_MODEL}", $"\"{pifData.MODEL}\"");
                                        changePifInfo.Add($"{PifKey.PIF_EVO_SECURITY_PATCH}", pifData.SECURITY_PATCH);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_DEVICE_INITIAL_SDK_INT}", pifData.DEVICE_INITIAL_SDK_INT);
                                        changePifInfo.Add($"{PifKey.PIF_EVO_SDK_INT}", pifData.SDK_INT);
                                        if (!string.IsNullOrEmpty(pifData.RELEASE))
                                            changePifInfo.Add($"{PifKey.PIF_EVO_RELEASE}", pifData.RELEASE);
                                        else
                                        {
                                            changePifInfo.Add($"{PifKey.PIF_EVO_RELEASE}", splitFingerprint[3]);
                                        }
                                    }

                                    foreach (var item in changePifInfo)
                                    {
                                        ADBService.runCMDRoot(
                                            $"shell setprop {item.Key} \"{item.Value}\"",
                                            deviceId
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return;
            }
        }

        public static POCO.Models.DeviceModel reconfigImei(POCO.Models.DeviceModel model)
        {
            string imei = model.Imei;
            string imei1 = model.Imei1;

            string dataUrl = $"https://raw.githubusercontent.com/doanvtamhuynh/database/main/tac_imei/tac_imei_{model.Manufacturer.ToLower()}.json";
            using HttpClient client = new HttpClient();
            try
            {
                string imeiJson = client.GetStringAsync(dataUrl)
                                        .GetAwaiter()
                                        .GetResult();

                if (string.IsNullOrWhiteSpace(imeiJson))
                    return model;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Dictionary<string, TacImei> db = JsonConvert.DeserializeObject<Dictionary<string, TacImei>>(imeiJson);

                if (db == null || db.Count == 0)
                    return model;

                List<long> tacList = new List<long>();

                if (!string.IsNullOrWhiteSpace(model.Model))
                {
                    foreach (var kv in db)
                    {
                        if (kv.Key.StartsWith(model.Model, StringComparison.OrdinalIgnoreCase))
                        {
                            tacList.AddRange(kv.Value.Tac);
                        }
                    }

                    if (tacList == null || tacList.Count == 0)
                    {
                        foreach (var item in db.Values)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Name) &&
                                item.Name.ToUpperInvariant().EndsWith(Regex.Replace(model.Model.ToUpperInvariant(), @"\s*\([^)]*\)", "")
                                                                            .Replace("ACTIVE", "").Trim()) &&
                                item.Tac != null &&
                                item.Tac.Count > 0)
                            {
                                tacList.AddRange(item.Tac);
                            }
                        }
                    }
                }

                if (tacList == null || tacList.Count == 0)
                    return model;

                long tac = tacList[RandomService.randomInRange(0, tacList.Count)];

                model.Imei = RandomService.GenerateImeiFromTac(tac);
                do
                {
                    model.Imei1 = RandomService.GenerateImeiFromTac(tac);
                }
                while (model.Imei1 == model.Imei);

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return model;
            }
        }

    }
}
