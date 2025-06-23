using POCO.Models;
using Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolChange.ViewModels.Constants;

namespace ToolChange.Services
{
    public class Util
    {
        public static bool checkSim;

        public static bool SaveDeviceInfo(DeviceModel tempDevice, string deviceId, string applicationPath, bool isFakeSim = false)
        {
            try
            {
                if (tempDevice == null)
                {
                    return false;
                }

                if (ADBService.getDeviceStatus(deviceId) == DeviceStatus.ReadyToChange)
                {
                    ADBService.rootAndRemount(deviceId);

                    ADBService.shellRemoveIfContainSpecificText("/system/build.prop", "product is obsolete", deviceId);
                    var changedSystemInfo = new Dictionary<string, string>();
                    var changedDefaultInfo = new Dictionary<string, string>();
                    var tempBaseband = string.IsNullOrEmpty(tempDevice.Baseband) ? tempDevice.BuildIncremental : tempDevice.Baseband;
                   
                    var lineageVersion = RandomService.generateLineageOsVersion(tempDevice.Release) + "-" + tempDevice.Code;
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.TYPE, "user");
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.USER, RandomService.generateUser());
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BRAND, tempDevice.Manufacturer);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.VENDOR, tempDevice.Manufacturer);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.MODEL, tempDevice.Model);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.MODEL_LINEAGE, tempDevice.Model);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.DEVICE, tempDevice.Code);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_PRODUCT, tempDevice.Code);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOARD, tempDevice.Board);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_BOARD, tempDevice.Board);
                    // changedSystemInfo.Add(BuildKey_SYSTEM_S9.EVO_DEVICE, tempDevice.Board);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_HOST, tempDevice.BuildHost);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.WIFI_MAC_ADDRESS, tempDevice.WifiMacAddress);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BLUETOOTH_MAC_ADDRESS, tempDevice.BlueToothMacAddress);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOT_LOADER, tempDevice.Bootloader);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOT_BOOT_LOADER, tempDevice.Bootloader);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOT_LOADER_LINEAGE, tempDevice.Bootloader);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.HARDWARE, tempDevice.Hardware);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_HARDWARE, tempDevice.Hardware);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.PRODUCT, tempDevice.Product);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.PLATFORM, tempDevice.Platform);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_PLATFORM, tempDevice.Platform);
                    ///
                    // changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOTLOADER_UNLOCKED, "green");
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DISPLAY_ID, tempDevice.BuildDisplayId);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_TAGS, tempDevice.Tags);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_INCREMENTAL, tempDevice.BuildIncremental);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DESCRIPTION, tempDevice.BuildDescription);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DATE, tempDevice.BuildDate);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DATE_UTC, tempDevice.BuildDateUtc);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_FLAVOR, tempDevice.BuildFlavor);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_DISPLAY_VERSION, lineageVersion);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_VERSION, lineageVersion);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_BUILD_VERSION, RandomService.getLineageNumberVersion(tempDevice.Release));
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_BUILD_VERSION_PLAT_SDK, tempDevice.Release);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BASEBAND, tempBaseband);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.GSM_BASEBAND, tempBaseband);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.SSID, RandomService.generateSSID());
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BSSID, RandomService.generateMacAddress());
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.FINGERPRINT, tempDevice.Fingerprint);
                    //changedSystemInfo.Add(BuildKey_SYSTEM_S9.SECURITY_PATH, tempDevice.SecurityPath);
                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_ID, tempDevice.BuildId);
                    //changedSystemInfo.Add(BuildKey_SYSTEM_S9.VERSION_RELEASE, tempDevice.Release);

                    changedSystemInfo.Add(BuildKey_SYSTEM_S9.FINGERPRINT_SYSTEM, tempDevice.Fingerprint);
                    //changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_PDA, tempBaseband);

                    ADBService.replaceBuildProp("/system/build.prop", changedSystemInfo, deviceId);
                    Dictionary<string, string> partitionList = new Dictionary<string, string>();
                    partitionList.Add("bootimage", "/system/build.prop");
                    partitionList.Add("vendor", "/vendor/build.prop");
                    partitionList.Add("product", "/product/etc/build.prop");
                    partitionList.Add("system", "/system/build.prop");
                    partitionList.Add("odm", "/odm/etc/build.prop");
                    partitionList.Add("odm_dlkm", "/vendor/odm_dlkm/etc/build.prop");
                    partitionList.Add("vendor_dlkm", "/vendor_dlkm/etc/build.prop");
                    partitionList.Add("system_dlkm", "/system/system_dlkm/etc/build.prop");
                    partitionList.Add("system_ext", "/system/system_ext/etc/build.prop");
                    RepleacePropertiesForPartition(tempDevice, partitionList, deviceId);


                    //BuildProp(tempDevice,"system.build.prop","/system/build.prop", deviceId);

                    // start to put setting
                    // setting device
                    /* if (ADBService.getDeviceStatus(deviceId) == DeviceStatus.ReadyToChange)
                     {
                         ADBService.rootAndRemount(deviceId);

                         ADBService.shellRemoveIfContainSpecificText("/system/build.prop", "product is obsolete", deviceId);
                         var changedSystemInfo = new Dictionary<string, string>();
                         var changedDefaultInfo = new Dictionary<string, string>();
                         var tempBaseband = string.IsNullOrEmpty(tempDevice.Baseband) ? tempDevice.BuildIncremental : tempDevice.Baseband;

                         var lineageVersion = RandomService.generateLineageOsVersion(tempDevice.Release) + "-" + tempDevice.Code;
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.TYPE, "user");
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.USER, RandomService.generateUser());
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BRAND, tempDevice.Manufacturer);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.VENDOR, tempDevice.Manufacturer);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.MODEL, tempDevice.Model);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.MODEL_LINEAGE, tempDevice.Model);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.DEVICE, tempDevice.Code);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_PRODUCT, tempDevice.Code);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOARD, tempDevice.Board);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_BOARD, tempDevice.Board);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_HOST, tempDevice.BuildHost);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.WIFI_MAC_ADDRESS, tempDevice.WifiMacAddress);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BLUETOOTH_MAC_ADDRESS, tempDevice.BlueToothMacAddress);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOT_LOADER, tempDevice.Bootloader);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOT_BOOT_LOADER, tempDevice.Bootloader);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BOOT_LOADER_LINEAGE, tempDevice.Bootloader);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.HARDWARE, tempDevice.Hardware);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_HARDWARE, tempDevice.Hardware);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.PRODUCT, tempDevice.Product);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.PLATFORM, tempDevice.Platform);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_PLATFORM, tempDevice.Platform);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DISPLAY_ID, tempDevice.BuildDisplayId);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_TAGS, tempDevice.Tags);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_INCREMENTAL, tempDevice.BuildIncremental);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DESCRIPTION, tempDevice.BuildDescription);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DATE, tempDevice.BuildDate);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_DATE_UTC, tempDevice.BuildDateUtc);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_FLAVOR, tempDevice.BuildFlavor);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_DISPLAY_VERSION, lineageVersion);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_VERSION, lineageVersion);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_BUILD_VERSION, RandomService.getLineageNumberVersion(tempDevice.Release));
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.LINEAGE_BUILD_VERSION_PLAT_SDK, tempDevice.Release);

                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BASEBAND, tempBaseband);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.GSM_BASEBAND, tempBaseband);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.SSID, RandomService.generateSSID());
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BSSID, RandomService.generateMacAddress());
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.FINGERPRINT, tempDevice.Fingerprint);
                         //changedSystemInfo.Add(BuildKey_SYSTEM_S9.SECURITY_PATH, tempDevice.SecurityPath);
                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_ID, tempDevice.BuildId);
                         //changedSystemInfo.Add(BuildKey_SYSTEM_S9.VERSION_RELEASE, tempDevice.Release);

                         changedSystemInfo.Add(BuildKey_SYSTEM_S9.FINGERPRINT_SYSTEM, tempDevice.Fingerprint);
                         //changedSystemInfo.Add(BuildKey_SYSTEM_S9.BUILD_PDA, tempBaseband);

                         ADBService.replaceBuildProp("/system/build.prop", changedSystemInfo, deviceId);

                         Dictionary<string, string> partitionList = new Dictionary<string, string>();
                         partitionList.Add("bootimage", "/system/build.prop");
                         partitionList.Add("vendor", "/vendor/build.prop");
                         partitionList.Add("system", "/system/build.prop");
                         partitionList.Add("odm", "/odm/etc/build.prop");
                         partitionList.Add("odm_dlkm", "/vendor/odm_dlkm/etc/build.prop");
                         partitionList.Add("vendor_dlkm", "/vendor_dlkm/etc/build.prop");
                         partitionList.Add("system_dlkm", "/system/system_dlkm/etc/build.prop");
                         partitionList.Add("system_ext", "/system/system_ext/etc/build.prop");
                         RepleacePropertiesForPartition(tempDevice, partitionList, deviceId);*/
                    ADBService.putSetting("bluetooth_address", tempDevice.BlueToothMacAddress, deviceId, "secure");
                    ADBService.putSetting("bluetooth_name", RandomService.generateName(), deviceId, "secure");
                    ADBService.putSetting("device_name", RandomService.generateName(), deviceId);

                    ADBService.putSetting(GlobalAndroidSettings.IMEI0, tempDevice.Imei, deviceId);
                    ADBService.putSetting(GlobalAndroidSettings.IMEI1, tempDevice.Imei1, deviceId);
                    ADBService.putSetting("spoof_status", "true", deviceId);
                    ADBService.putSetting("spoof_imei", tempDevice.Imei, deviceId);
                    // generate 48 bit random number for hardware serial no
                    ADBService.putSetting(GlobalAndroidSettings.HARDWARE_SERIALNO, tempDevice.SerialNo, deviceId);
                    //// generate android ID
                    /* ADBService.putSetting(GlobalAndroidSettings.ANDROID_ID, tempDevice.AndroidId, deviceId);
                     ADBService.putSetting("android_id", tempDevice.AndroidId, deviceId, "secure");*/
                    ADBService.updateInitRc(tempDevice.Imei, tempDevice.Imei1, tempDevice.SerialNo, tempDevice.Bootloader, tempDevice.Baseband, tempDevice.Model, deviceId, tempDevice.Hardware, tempDevice.Platform);
                    ADBService.fakeLocalHostNameV6(deviceId);

                    // fake wifi mac address
                    ADBService.fakeWifiMacAddress(tempDevice.WifiMacAddress, deviceId);
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
                    }
                    else
                    {
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


                    // update new GSF number
                    //string tempGsfInLong = long.Parse(tempDevice.Gsf, System.Globalization.NumberStyles.HexNumber).ToString();
                    //string currentGSF = ADBService.getGSFNumber(deviceId);
                    //ADBService.stringReplace(currentGSF, tempGsfInLong, "data/data/com.google.android.gms/files/checkin_id_token", deviceId);
                    //ADBService.stringReplace(currentGSF, tempGsfInLong, "data/data/com.google.android.gms/shared_prefs/Checkin.xml", deviceId);
                    Console.WriteLine("3.DONE put setting");
                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool SaveDeviceSIm(DeviceModel tempDevice, string deviceId, string applicationPath)
        {
            try
            {
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
        private static void RepleacePropertiesForPartition(DeviceModel tempDevice, Dictionary<string, string> partitions, string deviceId)
        {
            // key = partition name
            // value = path of partition
            foreach (var partition in partitions)
            {
                Console.WriteLine($"*******START Partition {partition.Key}*******");
                var changedSystemInfo = new Dictionary<string, string>();
                changedSystemInfo.Add($"ro.{partition.Key}.build.date", tempDevice.BuildDate);
                changedSystemInfo.Add($"ro.{partition.Key}.build.date.utc", tempDevice.BuildDateUtc);
                changedSystemInfo.Add($"ro.{partition.Key}.build.fingerprint", tempDevice.Fingerprint);
                changedSystemInfo.Add($"ro.{partition.Key}.build.id", tempDevice.BuildId);
                changedSystemInfo.Add($"ro.{partition.Key}.build.tags", tempDevice.Tags);
                changedSystemInfo.Add($"ro.{partition.Key}.build.type", "user");
                changedSystemInfo.Add($"ro.{partition.Key}.build.version.incremental", tempDevice.BuildIncremental);
                changedSystemInfo.Add($"ro.{partition.Key}.build.version.release", tempDevice.Release);
                changedSystemInfo.Add($"ro.{partition.Key}.build.version.release_or_codename", tempDevice.Release);
                //changedSystemInfo.Add($"ro.{partition.Key}.build.version.sdk", tempDevice.BuildDate);
                changedSystemInfo.Add($"ro.product.{partition.Key}.brand", tempDevice.Brand);
                changedSystemInfo.Add($"ro.product.{partition.Key}.device", tempDevice.Code);
                changedSystemInfo.Add($"ro.product.{partition.Key}.manufacturer", tempDevice.Manufacturer);
                changedSystemInfo.Add($"ro.product.{partition.Key}.model", tempDevice.Model);
                changedSystemInfo.Add($"ro.product.{partition.Key}.name", tempDevice.Code);
                ADBService.replaceBuildProp(partition.Value, changedSystemInfo, deviceId);
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
