using DeepDroid.Models;
using Microsoft.VisualBasic.ApplicationServices;
using POCO.Models;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToolChange.ViewModels;
using ToolChange.ViewModels.Constants;
using ToolChange.Views;

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

                ADBService.runCMDRoot($"shell setprop persist.sys.pixelexperience.sdk {isFakeSdk.ToString().ToLower()}", deviceId);

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

                VbMeta vbMeta = new VbMeta(deviceId);

                if (ADBService.getDeviceStatus(deviceId) == DeviceStatus.ReadyToChange)
                {
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "15%", "Change device ...");
                    ADBService.rootAndRemount(deviceId);

                    ADBService.shellRemoveIfContainSpecificText("/system/build.prop", "product is obsolete", deviceId);
                    var changedSystemInfo = new Dictionary<string, string>();
                    var changedDefaultInfo = new Dictionary<string, string>();
                    var tempBaseband = string.IsNullOrEmpty(tempDevice.Baseband) ? tempDevice.BuildIncremental : tempDevice.Baseband;
                    Models.DeviceUpdater.UpdateProgress(deviceS, deviceId, "25%", "Change information ");
                    var lineageVersion = RandomService.generateLineageOsVersion(tempDevice.Release) + "-" + tempDevice.Code;
                    var randomUser = RandomService.generateUser();
                    changedSystemInfo.Add("ro.build.type", "user");
                    changedSystemInfo.Add("ro.build.tags", tempDevice.Tags);
                    changedSystemInfo.Add("ro.build.use", randomUser);
                    changedSystemInfo.Add("ro.build.product", tempDevice.Code);
                    changedSystemInfo.Add("ro.build.fingerprint", tempDevice.Fingerprint);
                    changedSystemInfo.Add("ro.build.display.id", tempDevice.BuildDisplayId);
                    changedSystemInfo.Add("ro.build.host", tempDevice.BuildHost);
                    changedSystemInfo.Add("ro.build.version.incremental", tempDevice.BuildIncremental);
                    changedSystemInfo.Add("ro.build.description", tempDevice.BuildDescription);
                    changedSystemInfo.Add("ro.build.date", tempDevice.BuildDate);
                    changedSystemInfo.Add("ro.build.date.utc", tempDevice.BuildDateUtc);
                    changedSystemInfo.Add("ro.build.flavor", tempDevice.BuildFlavor);
                    changedSystemInfo.Add("ro.build.id", tempDevice.BuildId);

                    changedSystemInfo.Add("ro.product.name", tempDevice.Product);
                    changedSystemInfo.Add("ro.product.brand", tempDevice.Brand);
                    changedSystemInfo.Add("ro.product.manufacturer", tempDevice.Manufacturer);
                    changedSystemInfo.Add("ro.product.model", tempDevice.Model);
                    changedSystemInfo.Add("ro.product.device", tempDevice.Code);

                    changedSystemInfo.Add("ro.android.wifi", tempDevice.WifiMacAddress);
                    changedSystemInfo.Add("ro.android.bluetooth", tempDevice.BlueToothMacAddress);
                    changedSystemInfo.Add("ro.android.bootloader", tempDevice.Bootloader);
                    changedSystemInfo.Add("ro.android.hardware", tempDevice.Hardware);
                    changedSystemInfo.Add("ro.android.platform", tempDevice.Platform);
                    changedSystemInfo.Add("ro.android.board", tempDevice.Board);

                    changedSystemInfo.Add("ro.android.SSID", RandomService.generateSSID());
                    changedSystemInfo.Add("ro.android.BSSID", RandomService.generateMacAddress());
                    changedSystemInfo.Add("ro.android.soc.manufacturer", tempDevice.Manufacturer);
                    changedSystemInfo.Add("ro.android.soc.model", tempDevice.Hardware);
                    changedSystemInfo.Add("ro.android.build.version.security_patch", tempDevice.SecurityPath);
                    changedSystemInfo.Add("ro.android.build.version.release", tempDevice.Release);
                    changedSystemInfo.Add("ro.android.build.version.sdk", tempDevice.SDK);

                    changedSystemInfo.Add("ro.android.gsm.version.baseband", tempBaseband);
                    changedSystemInfo.Add("gsm.version.baseband", tempBaseband);
                    changedSystemInfo.Add("ro.com.google.clientidbase", $"android-{tempDevice.Brand}");
                    changedSystemInfo.Add("ro.debuggable", "0");

                    changedSystemInfo.Add("ro.boot.vbmeta.avb_version", vbMeta.VbmetaVersion);
                    changedSystemInfo.Add("ro.boot.vbmeta.hash_alg", vbMeta.VbmetaAlgorithm);
                    changedSystemInfo.Add("ro.boot.vbmeta.size", vbMeta.VbmetaSize);
                    changedSystemInfo.Add("ro.boot.vbmeta.digest", vbMeta.VbmetaDigest);

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
                    ADBService.replaceBuildProp("/system/etc/build.prop", changedSystemInfo, deviceId);

                    var changedProductInfo = new Dictionary<string, string>();
                    changedProductInfo.Add("ro.com.google.clientidbase", $"android-{tempDevice.Brand}");
                    ADBService.replaceBuildProp("product/build.prop", changedProductInfo, deviceId);
                    ADBService.replaceBuildProp("product/etc/build.prop", changedProductInfo, deviceId);

                    var changedVendorInfo = new Dictionary<string, string>();
                    changedVendorInfo.Add("ro.soc.manufacturer", tempDevice.Manufacturer);
                    changedVendorInfo.Add("ro.soc.model", tempDevice.Hardware);
                    changedVendorInfo.Add("ro.product.board", tempDevice.Board);
                    changedVendorInfo.Add("bluetooth.device.default_name", tempDevice.Manufacturer);
                    ADBService.replaceBuildProp("vendor/build.prop", changedVendorInfo, deviceId);
                    ADBService.replaceBuildProp("vendor/etc/build.prop", changedVendorInfo, deviceId);
                    ADBService.replaceBuildProp("mnt/scratch/overlay/vendor/upper/build.prop", changedVendorInfo, deviceId);

                    Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
                    partitionList.Add("system", new List<string> { "/system/build.prop", "/system/etc/build.prop" });
                    partitionList.Add("bootimage", new List<string> { "/system/build.prop", "/system/etc/build.prop" });
                    partitionList.Add("vendor", new List<string> { "/vendor/build.prop", "/vendor/etc/build.prop" });
                    partitionList.Add("product", new List<string> { "/product/build.prop", "/product/etc/build.prop" });
                    partitionList.Add("odm", new List<string> { "/odm/build.prop", "/odm/etc/build.prop" });
                    partitionList.Add("odm_dlkm", new List<string> { "/vendor/odm_dlkm/build.prop", "/vendor/odm_dlkm/etc/build.prop" });
                    partitionList.Add("vendor_dlkm", new List<string> { "/vendor_dlkm/build.prop", "/vendor_dlkm/etc/build.prop" });
                    partitionList.Add("system_dlkm", new List<string> { "/system_dlkm/build.prop", "/system_dlkm/etc/build.prop", "/system/system_dlkm/build.prop", "/system/system_dlkm/etc/build.prop" });
                    partitionList.Add("system_ext", new List<string> { "/system_ext/build.prop", "/system_ext/etc/build.prop", "/system/system_ext/build.prop", "/system/system_ext/etc/build.prop" });
                    RepleacePropertiesForPartition(tempDevice, partitionList, deviceId);

                    ADBService.putSetting("bluetooth_address", tempDevice.BlueToothMacAddress, deviceId, "secure");
                    ADBService.putSetting("bluetooth_name", RandomService.generateName(), deviceId, "secure");
                    ADBService.putSetting("device_name", RandomService.generateName(), deviceId);

                    ADBService.putSetting(GlobalAndroidSettings.IMEI0, tempDevice.Imei, deviceId);
                    ADBService.putSetting(GlobalAndroidSettings.IMEI1, tempDevice.Imei1, deviceId);
                    // generate 48 bit random number for hardware serial no
                    ADBService.putSetting(GlobalAndroidSettings.HARDWARE_SERIALNO, tempDevice.SerialNo, deviceId);
                    //// generate android ID
                    ADBService.putSetting(GlobalAndroidSettings.ANDROID_ID, tempDevice.AndroidId, deviceId);
                    ADBService.putSetting("mi_bluetooth_mac_address", tempDevice.BlueToothMacAddress, deviceId);
                    ADBService.putSetting("mi_wifi_mac_address", tempDevice.WifiMacAddress, deviceId);
                    ADBService.putSetting("android_id", tempDevice.AndroidId, deviceId, "secure");

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
           
                //                        public static readonly string DATA_ACTIVITY = string.Concat(MI_PREFIX, "data_activity");
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
                    [$"ro.{partition.Key}.build.date"] = tempDevice.BuildDate,
                    [$"ro.{partition.Key}.build.date.utc"] = tempDevice.BuildDateUtc,
                    [$"ro.{partition.Key}.build.fingerprint"] = tempDevice.Fingerprint,
                    [$"ro.{partition.Key}.build.id"] = tempDevice.BuildId,
                    [$"ro.{partition.Key}.build.tags"] = tempDevice.Tags,
                    [$"ro.{partition.Key}.build.type"] = "user",
                    [$"ro.{partition.Key}.build.version.incremental"] = tempDevice.BuildIncremental
                    // [$"ro.{partition.Key}.build.version.release"] = tempDevice.Release,
                    // [$"ro.{partition.Key}.build.version.release_or_codename"] = tempDevice.Release,
                    // [$"ro.{partition.Key}.build.version.sdk"] = tempDevice.BuildDate,
                    
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
