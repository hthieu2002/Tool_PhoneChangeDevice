
using POCO.Models;
using Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ToolChange.ViewModels;

namespace ToolChange.Services
{
    public class Util
    {
        public static bool checkSim;

        public static bool SaveDeviceInfo(DeviceViewModel model, ObservableCollection<ToolChange.Models.DeviceModel> deviceS , POCO.Models.DeviceModel tempDevice, string deviceId, string applicationPath, bool isFakeSim = false, bool keepBrand = false, bool isFakeSdk = true)
        {
            try
            {
                //tempDevice.Brand = "google";
                //tempDevice.Manufacturer = "Google";
                //tempDevice.Name = "comet";
                //tempDevice.Fingerprint = "google/comet_beta/comet:16/BP41.250916.010.A1/14281945:user/release-keys";
                //tempDevice.Manufacturer = "Google";
                //tempDevice.Model = "Pixel 9 Pro Fold";
                //tempDevice.Code = "comet";
                //tempDevice.Release = "16";
                //tempDevice.BuildId = "BP41.250916.010.A1";
                //tempDevice.BuildIncremental = "14281945";
                //tempDevice.Product = "comet_beta";
                //tempDevice.SecurityPath = "2025-10-05";

                //tempDevice.Manufacturer = "Google";
                //tempDevice.Brand = "google";
                //tempDevice.Model = "Pixel 3a";
                //tempDevice.Fingerprint = "google/sargo/sargo:12/SP2A.220505.008/8782922:user/release-keys";
                //tempDevice.Product = "sargo";
                //tempDevice.Code = "sargo";
                //tempDevice.Release = "12";
                //tempDevice.BuildId = "SP2A.220505.008";
                //tempDevice.BuildDisplayId = "SP2A.220505.008";
                //tempDevice.BuildIncremental = "8782922";
                //tempDevice.BuildDescription = "sargo-user 12 SP2A.220505.008 8782922 release-keys";
                //tempDevice.BuildFlavor = "sargo-user";
                //tempDevice.BuildHost = "abfarm801";
                //tempDevice.BuildDate = "Wed Jun 29 18:10:44 UTC 2022";
                //tempDevice.BuildDateUtc = "1656526244";
                //tempDevice.SecurityPath = "2022-05-05";
                //tempDevice.Platform = "sdm710";
                //tempDevice.SDK = "32";
                //tempDevice.Hardware = "sargo";
                //tempDevice.Board = "sargo";
                //tempDevice.Bootloader = "b4s4-0.4-8048689";

                ADBService.runCMDRoot($"shell setprop persist.sys.deepdroid.sdk {isFakeSdk.ToString().ToLower()}", deviceId);

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

                string securityPatchProp = ADBService.getProp("persist.sys.deepdroid.pihooks_SECURITY_PATCH", deviceId);
                string roBuildDate = RandomService.generateBuildDate(securityPatchProp);
                string roBuildDateUtc = RandomService.generateBuildDateUTC(securityPatchProp);

                tempDevice.BuildDate = roBuildDate;
                tempDevice.BuildDateUtc = roBuildDateUtc;

                string bluetoothName = RandomService.generateName();
                string deviceName = RandomService.generateName();

                /**
                 * Keep OUI of wifi mac address
                 */

                //string prefixMac = ADBService.runCMDRoot($"shell cat /sys/class/net/wlan0/address", deviceId);

                //if (!string.IsNullOrEmpty(prefixMac))
                //{
                //    prefixMac = prefixMac.Trim();
                //    prefixMac = prefixMac.Substring(0, 8);

                //    tempDevice.WifiMacAddress = RandomService.generateWifiMacAddress(
                //        tempDevice.Manufacturer.ToLower(),
                //        prefixMac
                //    );

                //    tempDevice.BlueToothMacAddress = RandomService.generateWifiMacAddress(
                //        tempDevice.Manufacturer.ToLower(),
                //        prefixMac
                //    );
                //}

                if (ADBService.getDeviceStatus(deviceId) == DeviceStatus.ReadyToChange)
                {
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "15%", "Change device ...");
                    ADBService.rootAndRemount(deviceId);

                    ADBService.shellRemoveIfContainSpecificText("/system/build.prop", "product is obsolete", deviceId);
                    var changedSystemInfo = new Dictionary<string, string>();
                    var tempBaseband = string.IsNullOrEmpty(tempDevice.Baseband) ? tempDevice.BuildIncremental : tempDevice.Baseband;
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "25%", "Change information ");
                    var lineageVersion = RandomService.generateLineageOsVersion(tempDevice.Release) + "-" + tempDevice.Code;
                    var randomUser = RandomService.generateUser();
                    changedSystemInfo.Add("ro.build.type", "user");
                    changedSystemInfo.Add("ro.build.tags", "release-keys");
                    changedSystemInfo.Add("ro.build.use", randomUser);
                    changedSystemInfo.Add("ro.build.product", tempDevice.Code);
                    changedSystemInfo.Add("ro.build.fingerprint", tempDevice.Fingerprint);
                    changedSystemInfo.Add("ro.build.display.id", tempDevice.BuildDisplayId);
                    changedSystemInfo.Add("ro.build.host", randomUser);
                    //changedSystemInfo.Add("ro.build.version.incremental", tempDevice.BuildIncremental);
                    changedSystemInfo.Add("ro.build.description", tempDevice.BuildDescription);
                    //changedSystemInfo.Add("ro.build.date", tempDevice.BuildDate);
                    //changedSystemInfo.Add("ro.build.date.utc", tempDevice.BuildDateUtc);
                    changedSystemInfo.Add("ro.build.flavor", tempDevice.BuildFlavor);
                    changedSystemInfo.Add("ro.build.id", tempDevice.BuildId);

                    changedSystemInfo.Add("ro.product.name", tempDevice.Product);
                    changedSystemInfo.Add("ro.product.brand", tempDevice.Brand);
                    changedSystemInfo.Add("ro.product.manufacturer", tempDevice.Manufacturer);
                    changedSystemInfo.Add("ro.product.model", tempDevice.Model);
                    changedSystemInfo.Add("ro.product.device", tempDevice.Code);

                    changedSystemInfo.Add("ro.android.device.mac", tempDevice.WifiMacAddress);
                    changedSystemInfo.Add("ro.android.bssid", RandomService.generateWifiMacAddress(tempDevice.Manufacturer.ToLower()));
                    changedSystemInfo.Add("ro.android.ssid", RandomService.generateSSID());
                    //changedSystemInfo.Add("ro.android.bluetooth.mac", tempDevice.BlueToothMacAddress);
                    //changedSystemInfo.Add("ro.android.bluetooth.name", bluetoothName);
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
                    changedSystemInfo.Add("ro.android.build.version.sdk", tempDevice.SDK);

                    changedSystemInfo.Add("ro.android.gsm.version.baseband", tempBaseband);
                    changedSystemInfo.Add("gsm.version.baseband", tempBaseband);
                    changedSystemInfo.Add("ro.com.google.clientidbase", $"android-{tempDevice.Brand}");
                    changedSystemInfo.Add("ro.debuggable", "0");

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

                    string isRomPixelExperience = ADBService.getProp("org.pixelexperience.device", deviceId);
                    if(!string.IsNullOrEmpty(isRomPixelExperience))
                    {
                        changedSystemInfo.Add("org.pixelexperience.device", tempDevice.Code);
                        changedSystemInfo.Add("org.pixelexperience.version.display", "unknown");
                        changedSystemInfo.Add("org.pixelexperience.build_date", "unknown");
                        changedSystemInfo.Add("org.pixelexperience.build_date_utc", "unknown");
                        changedSystemInfo.Add("org.pixelexperience.build_type", "unknown");
                        changedSystemInfo.Add("org.pixelexperience.build_security_patch", "unknown");
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
                    //ADBService.replaceBuildProp("mnt/scratch/overlay/vendor/upper/build.prop", changedVendorInfo, deviceId);

                    Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
                    partitionList.Add("system", new List<string> { "/system/build.prop"});
                    partitionList.Add("bootimage", new List<string> { "/system/build.prop"});
                    partitionList.Add("vendor", new List<string> { "/vendor/build.prop"});
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
            catch
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
        public static void FakeSimInfo(DeviceModel tempDevice, string deviceId, bool isFakeSim)
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
        private static void RepleacePropertiesForPartition(DeviceModel tempDevice, Dictionary<string, List<string>> partitions, string deviceId)
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
                    //[$"ro.{partition.Key}.build.date"] = tempDevice.BuildDate,
                    //[$"ro.{partition.Key}.build.date.utc"] = tempDevice.BuildDateUtc,
                    [$"ro.{partition.Key}.build.fingerprint"] = tempDevice.Fingerprint,
                    [$"ro.{partition.Key}.build.id"] = tempDevice.BuildId,
                    [$"ro.{partition.Key}.build.tags"] = "release-keys",
                    [$"ro.{partition.Key}.build.type"] = "user",
                    //[$"ro.{partition.Key}.build.version.incremental"] = tempDevice.BuildIncremental
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

    }
}
