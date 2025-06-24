using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Services
{
    [Obfuscation(Exclude = false)]
    public static partial class ADBService
    {
        private static List<Point> getKeysCoordinator(string text)
        {
            List<Point> result = new List<Point>();
            Dictionary<char, Point> keyDic = new Dictionary<char, Point>();
            keyDic.Add('q', new Point(100, 1300));
            keyDic.Add('w', new Point(200, 1300));
            keyDic.Add('e', new Point(300, 1300));
            keyDic.Add('r', new Point(400, 1300));
            keyDic.Add('t', new Point(500, 1300));
            keyDic.Add('y', new Point(600, 1300));
            keyDic.Add('u', new Point(700, 1300));
            keyDic.Add('i', new Point(800, 1300));
            keyDic.Add('o', new Point(900, 1300));
            keyDic.Add('p', new Point(1000, 1300));

            keyDic.Add('a', new Point(150, 1500));
            keyDic.Add('s', new Point(250, 1500));
            keyDic.Add('d', new Point(350, 1500));
            keyDic.Add('f', new Point(450, 1500));
            keyDic.Add('g', new Point(550, 1500));
            keyDic.Add('h', new Point(650, 1500));
            keyDic.Add('j', new Point(750, 1500));
            keyDic.Add('k', new Point(850, 1500));
            keyDic.Add('l', new Point(950, 1500));


            keyDic.Add('z', new Point(220, 1660));
            keyDic.Add('x', new Point(320, 1660));
            keyDic.Add('c', new Point(420, 1660));
            keyDic.Add('v', new Point(520, 1660));
            keyDic.Add('b', new Point(620, 1660));
            keyDic.Add('n', new Point(720, 1660));
            keyDic.Add('m', new Point(820, 1660));

            keyDic.Add('1', new Point(60, 1300));
            keyDic.Add('2', new Point(170, 1300));
            keyDic.Add('3', new Point(280, 1300));
            keyDic.Add('4', new Point(390, 1300));
            keyDic.Add('5', new Point(500, 1300));
            keyDic.Add('6', new Point(610, 1300));
            keyDic.Add('7', new Point(720, 1300));
            keyDic.Add('8', new Point(830, 1300));
            keyDic.Add('9', new Point(940, 1300));
            keyDic.Add('0', new Point(1050, 1300));

            keyDic.Add(' ', new Point(555, 1830));

            foreach (var character in text)
            {
                if (char.IsUpper(character))
                    result.Add(new Point(75, 1665));
                if (char.IsDigit(character))
                    result.Add(new Point(75, 1800));
                result.Add(keyDic[char.ToLower(character)]);
                if (char.IsDigit(character))
                    result.Add(new Point(75, 1800));
            }
            return result;//76,1665
        }
        public static void InputTextFromKeyboard(string text, string deviceId)
        {
            var listPoints = getKeysCoordinator(text);
            for (int i = 0; i < listPoints.Count(); i++)
            {
                var keyStrokePoint = listPoints[i];
                inputTapEvent(keyStrokePoint.X, keyStrokePoint.Y, deviceId);
                Thread.Sleep(RandomService.randomInRange(0, 123));
            }
        }
        public static void setDebuggableValue(string debuggableValue, string deviceId)
        {
            runCMD("root", deviceId);
            runCMD(string.Format("shell setprop ro.debuggable {0}", debuggableValue), deviceId);
        }
        public static bool restoreFullInfo(string fromDesktopFullPath, string deviceId, string password = SecurityConfig.DEFAULT_PASSWORD)
        {
            runCMD("root", deviceId);
            runCMD("remount", deviceId);
            runCMD("shell \"mount -o rw,remount rootfs\"", deviceId);
            var fileName = fromDesktopFullPath.Split('\\').Last().Replace(".7z", string.Empty);
            var adbPushResponse = runCMD(String.Format("push \"{0}\" /", fromDesktopFullPath), deviceId);

            // check if old version, using default password
            if (checkPassword7z(fileName, deviceId, SecurityConfig.DEFAULT_PASSWORD))
            {
                password = string.Format("-p{0}", SecurityConfig.DEFAULT_PASSWORD);

            }
            // newer version
            else if (checkPassword7z(fileName, deviceId, password))
            {
                password = string.Format("-p{0}", password);
            }
            else
            {
                runCMD("shell \"rm -rf *.7z\"", deviceId);
                throw new Exception("Corrupted");
            }
            var exclude = "excludeFiles.txt";

            var un7zAllPackagesCommand = String.Format("shell \" 7z x {0}.7z {1} -y \"", fileName, password);
            // unzip with exclude files defined in excludeFiles.txt
            var unZipAllPackagesCommand = String.Format("shell \"unzip -o {0}.zip -x `cat {1}`\"", fileName, exclude);

            var adbUn7zResponse = runCMD(un7zAllPackagesCommand, deviceId);
            // List all exclude files in old backups.zip (vending/lib, all providers, quicksearchbox)
            runCMD(string.Format("shell \"unzip {0}.zip -lq | grep -E 'vending/lib|providers|googlequicksearchbox|netstats|procstats|ar.core|syncmanager-log|suggestions.db|locksettings.db|notification_log.db|spblob' | awk '{{print $4}}' > {1}\"", fileName, exclude), deviceId);
            // Exclude all package inside data/data/* except gms, gsf, vending
            runCMD(string.Format("shell \"unzip {0}.zip -lq | grep 'data/data/' | grep -Ev 'data\\/com.google.android.gms|data\\/com.google.android.gsf|data\\/com.android.vending' | awk '{{print $4}}' >> {1}\"", fileName, exclude), deviceId);

            var adbUnzipResponse = runCMD(unZipAllPackagesCommand, deviceId);

            var systemProps = getPropsFromFile("/system/build.prop.min", deviceId);
            replaceBuildProp("/system/build.prop", systemProps, deviceId);
            var vendorProps = getPropsFromFile("/system/vendor/build.prop.min", deviceId);
            replaceBuildProp("/system/vendor/build.prop", vendorProps, deviceId);

            runCMD("shell \"rm -rf *.zip\"", deviceId);
            runCMD("shell \"rm -rf *.7z\"", deviceId);
            runCMD($"shell \"rm -rf {exclude}\"", deviceId);
            //Remove prop.min
            runCMD("shell \"rm -rf /system/build.prop.min\"", deviceId);
            runCMD("shell \"rm -rf /system/vendor/build.prop.min\"", deviceId);
            return adbPushResponse.Contains("1 file pushed")
            && adbUnzipResponse.Contains("inflating")
            && adbUn7zResponse.Contains("Everything is Ok");

        }
        public static Dictionary<string, string> getPropsFromFile(string androidPath, string deviceId)
        {
            var lines = readFromFile(androidPath, deviceId).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var props = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var indexSplitChar = line.IndexOf('=');
                if (indexSplitChar != -1)
                {
                    var key = line.Substring(0, indexSplitChar);
                    var value = line.Substring(indexSplitChar + 1).Trim();
                    props.Add(key, value);
                }
            }
            return props;
        }
        /* public static bool getDeviceStatusBool(string device)
         {
             var adbRootRunning = getRoot("init.svc.adb_root", device).Contains("running");
             if (adbRootRunning)
             {
                 return true;
             }
             else
             {
                 return false;
             }
         }*/

        /*public static DeviceStatus getDeviceStatus(string deviceId)
        {
            var adbRootRunning = getProp("init.svc.adb_root", deviceId).Contains("running");
            var adbRootRunning1 = getProp("lineage.service.adb.root", deviceId).Trim().Equals("1");
            var adbRootRunning2 = runCMD("root", deviceId).Contains("running");
            if (adbRootRunning || adbRootRunning1 || adbRootRunning2)
            {
                return DeviceStatus.ReadyToChange;
            }
            return DeviceStatus.Undefined;
        }*/
        public static bool checkPassword7z(string fromSystemPath, string deviceId, string password)
        {
            var checkPassCommand = string.Format("shell \"7z t -p{0} {1}.7z > /dev/null 2>&1 && echo correct || echo wrong \"", password, fromSystemPath);
            var response = runCMD(checkPassCommand, deviceId).Trim();
            return response.Equals("correct");
        }
        public static void savePropsToFile(string srcPath, string dstPath, List<string> propKeys, string deviceId)
        {
            runCMD("remount", deviceId);
            var grepParams = string.Join("|", propKeys);
            runCMD(string.Format("shell \"cat {0} | grep -E '{1}' > {2}\"", srcPath, grepParams, dstPath), deviceId);
        }
        public static bool backUpGMS(string destinationDesktopFullPath, string deviceId)
        {
            runCMD("root", deviceId);
            runCMD("remount", deviceId);
            var fileName = destinationDesktopFullPath.Split('\\').Last();
            var filePath = destinationDesktopFullPath.Substring(0, destinationDesktopFullPath.LastIndexOf(@"\"));

            var zipAllPackagesCommand = String.Format("shell \"zip -r ~/{0} /data/data/com.google.android.* /data/data/com.android.vending /data/data/com.android.providers.contacts /data/syste* -x '*/cache/*' /data/system/notification_policy.xml /data/system/package* /data/system/users/0/* \""
                , fileName);

            var zipPackagesAdbResponse = runCMD(zipAllPackagesCommand, deviceId);
            var pullResponse = runCMD(String.Format("pull /{0} \"{1}\" ", fileName, filePath), deviceId);
            //Console.WriteLine(pullResponse);

            runCMD("shell \"rm -rf *.zip\"", deviceId);
            //runCMD("unroot", deviceId);

            return zipPackagesAdbResponse.Contains("adding") && pullResponse.Contains("1 file pulled");
        }
        public static bool backUpFullInfo(string destinationDesktopFullPath, string deviceId, string password = "")
        {
            runCMD("root", deviceId);
            runCMD("remount", deviceId);
            runCMD("shell \"mount -o rw,remount rootfs\"  ", deviceId);
            var fileName = destinationDesktopFullPath.Split('\\').Last().Replace(".7z", string.Empty);
            var filePath = destinationDesktopFullPath.Substring(0, destinationDesktopFullPath.LastIndexOf(@"\"));
            var zipAllPackagesCommand = String.Format("shell \"zip -r ~/{0}.zip " +
                "/system/build.prop.min " +
                "/system/vendor/build.prop.min " +
                "/data/data/com.google.android.gms " +
                "/data/data/com.google.android.gsf " +
                "/data/data/com.android.vending " +
                //"/data/data/com.android.providers* " +
                //"/data/data/com.google.ar.core " +
                "/data/user_de/0/com.android.vending " +
                "/data/user_de/0/com.google.android.gms " +
                "/data/user_de/0/com.google.android.gsf " +
                "/sdcard/Android/data/com.google.android.gms " +
                "/sdcard/Android/data/com.android.vending " +
                "/data/syste* " +
                "-x '*/cache/*' " +
                "*/dropbox/* " +
                "*/netstats/* " +
                "*/procstats/* " +
                "*/graphicsstats/* " +
                "*/spblob/* " +
                "*/syncmanager-log/* " +
                "/data/system/locksettings* " +
                "/data/system/notification_log* " +
                "`find /sdcard/Android/data/com.android.vending/files/ -type f` " +
                "`find /sdcard/Android/data/com.google.android.gms/files/ -type f` " +
                "/data/data/com.android.vending/lib/ " +
                "/data/data/com.android.vending/lib/* " +
                "/data/data/com.android.providers* " +
                "/data/system/notification_policy.xml " +
                "/data/system/package* " +
                "/data/system/users/0/app_idle_stats.xml " +
                "/data/system/users/0/runtime-permissions.xml " +
                "/data/system/users/0/appwidgets.xml " +
                "/data/system/users/0/settings_ssaid.xml " +
                "/data/system/users/0/package-restrictions.xml " +
                "/data/system/users/0/settings_system.xml " +
                "/data/system/users/0/wallpaper_info.xml \""
                , fileName);

            if (!string.IsNullOrEmpty(password))
                password = string.Format("-p{0}", password);

            var sevenZPackageCommand = String.Format("shell \"7z a {0}.7z {0}.zip {1}\" ", fileName, password);

            var zipPackagesAdbResponse = runCMD(zipAllPackagesCommand, deviceId);
            var sevenZPackageAdbResponse = runCMD(sevenZPackageCommand, deviceId);
            //var 7zPackageAdbResponse = run
            var pullResponse = runCMD(String.Format("pull /{0}.7z \"{1}\" ", fileName, filePath), deviceId);

            runCMD("shell \"rm -rf *.zip\"", deviceId);
            runCMD("shell \"rm -rf *.7z\"", deviceId);
            runCMD("shell \"rm -rf /system/build.prop.min\"", deviceId);
            runCMD("shell \"rm -rf /system/vendor/build.prop.min\"", deviceId);

            return zipPackagesAdbResponse.Contains("adding")
                && pullResponse.Contains("1 file pulled")
                && sevenZPackageAdbResponse.Contains("Everything is Ok");
        }

        public static bool hasSimcardInserted(string deviceId)
        {
            var adbResponse = runCMD("shell service call phone 78", deviceId);
            return adbResponse.Contains("1");
        }
        public static void clearPackagesBeforeRestore(string deviceId)
        {
            forceStopPackage(PackageName.VENDING, deviceId);
            forceStopPackage(PackageName.GMS, deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.media"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.contacts"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.downloads"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.downloads.ui"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.GMS), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.webview"), deviceId);
            // disable quicksearchbox
            runCMD(String.Format("shell pm clear {0}", "com.google.android.googlequicksearchbox"), deviceId);
            runCMD(String.Format("shell pm disable {0}", "com.google.android.googlequicksearchbox"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "tugapower.codeaurora.browser"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.chrome"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "org.lineageos.jelly"), deviceId);
        }
        public static void clearDatasAfterRestore(string deviceId)
        {
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/suggestions* \"", PackageName.VENDING), deviceId);
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/*-wal \"", PackageName.VENDING), deviceId);
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/*-shm \"", PackageName.VENDING), deviceId);
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/*-wal \"", PackageName.GMS), deviceId);
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/*-shm \"", PackageName.GMS), deviceId);
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/*-wal \"", PackageName.GSF), deviceId);
            runCMD(String.Format("shell \"rm -rf //data/data/{0}/databases/*-shm \"", PackageName.GSF), deviceId);
        }

        public static void updateFileDateTimeModification(string filePath, string deviceId)
        {
            runCMD(string.Format("shell \"find {0} -exec touch -m -a {{}} +\"", filePath), deviceId);
        }

        public static void updateDateTimeModification(string prefix, string appName, string deviceId)
        {
            runCMD(string.Format("shell \"find ./system/{1}/{0} -exec touch -m -a {{}} +\"", appName, prefix), deviceId);
            runCMD(string.Format("shell \"find ./system/{1}/{0}/{0}.apk -exec touch -m -a {{}} +\"", appName, prefix), deviceId);
        }
        public static void cleanGMSPackagesAndAccounts(string deviceId, bool isRootAndRemount = false)
        {
            if (isRootAndRemount)
            {
                runCMD("root", deviceId);
                runCMD("remount", deviceId);
            }
            //runCMD("shell \"mount -o rw,remount rootfs\"  ", deviceId);
            // run clean package
            runCMD(String.Format("shell pm clear {0}", "com.android.chrome"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.vending"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.contacts"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.htmlviewer"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.location.fused"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.media"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.downloads"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.providers.downloads.ui"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "org.lineageos.jelly"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.android.webview"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.webview"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.gsf"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.ims"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.gm"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.calendar"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.play.games"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.gsf.login"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.youtube"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.apps.magazines"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.apps.docs"), deviceId);
            //runCMD(String.Format("shell pm clear {0}", "tugapower.codeaurora.browser"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.configupdater"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.onetimeinitializer"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.setupwizard"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.tts"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.apps.restore"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.backuptransport"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.carriersetup"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.ext.services"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.feedback"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.partnersetup"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.syncadapters.calendar"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.syncadapters.contacts"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.gms"), deviceId);
            //runCMD("shell stop", deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.configupdater/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.onetimeinitializer/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.setupwizard/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.tts/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.apps.restore/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.backuptransport/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.carriersetup/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.ext.services/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.feedback/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.partnersetup/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.syncadapters.calendar/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "com.google.android.syncadapters.contacts/*"), deviceId);
            //run delete package
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.GSF), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.VENDING), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.SYSTEM_SYNC), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.SYSTEM_CE), deviceId);
            //runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.SYSTEM_DE), deviceId); // Pixel is starting...
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/accounts_de.db"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/accounts_de.db-journal"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/LocalesFromDelegatePrefs.xml"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/persisted_taskIds.txt"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/apprestriction"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/app_lock"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/powerstats"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/ringtones"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/system"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/accounts_de.db"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/accounts_de.db-journal"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/LocalesFromDelegatePrefs.xml"), deviceId);
            runCMD(String.Format("shell \"rm {0} \"", "/data/system_de/0/persisted_taskIds.txt"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/apprestriction"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/app_lock"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/powerstats"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/ringtones"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_de/0/system"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.CHROME), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.IMS), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.CALENDAR), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.GMAIL), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.PLAY_GAME), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.HTML_VIEWER), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.WEBVIEW), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.GOOGLE_WEBVIEW), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.GOOGLE_BACKUPTRANSPORT), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.LOCATION), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.JELLY), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", Package_Data.GMS), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/drm/fwdlock/kek.dat"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/sdcard/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system/sync/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system/slice/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/vendor/mediadrm/IDM1013/L3/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/system/product/app/webview/oat/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/system/product/app/webview/*/oat/*"), deviceId);
            // update 20/2/2025
            runCMD(String.Format("shell \"rm -rf {0} \"", "/mnt/user/0/self/primary/Android/data/com.google.android.gms"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/mnt/user/0/self/primary/Android/data/com.android.vending"), deviceId);
            // clear wifi data
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/misc/apexdata/com.android.tethering/netstats/*"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system/netstats/*"), deviceId);
            //runCMD(String.Format("shell \"rm -rf {0} \"", "/data/property"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/per_boot"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/preloads"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/resource-cache"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/rollback"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/rollback-history"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/rollback-observer"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/server_configurable_flags"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/ss"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/ssh"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/tombstones"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/vendor"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/vendor_ce"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/vendor_de"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/media"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/mediadrm"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/misc_de"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/nfc"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/ota"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/ota_package"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/misc_ce"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/drm"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/evolution_updates"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/fonts"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/gsi"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/gsi_persistent_data"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/incremental"), deviceId);
            runCMD(String.Format("shell \"find /data/local -mindepth 1 -maxdepth 1 -type d -not -path '/data/local/tmp' -exec rm -rf {{}} +\""), deviceId);
            //runCMD(String.Format("shell \"find /data/local -type f -not -name 'keybox.xml' -delete; find /data/local -type d -empty -not -path '/data/local/tmp' -delete\""), deviceId);
            //runCMD(String.Format("shell \"rm -rf {0} \"", "/data/local"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/lost+found"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/backup"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/bootanim"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/bootchart"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/cache"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/dalvik-cache"), deviceId);
            var getPixelExp = runCMD(String.Format("shell getprop | findstr pixelexperience"), deviceId);
            if (string.IsNullOrEmpty(getPixelExp))
            {
                runCMD(String.Format("shell \"rm -rf {0} \"", "/data/data"), deviceId);
            }
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/dpm"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/system_ce"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/user_de"), deviceId);
            //runCMD(String.Format("shell \"rm -rf {0} \"", "/data/adb"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/anr"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/apex"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/app-asec"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/app-ephemeral"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/app-lib"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/app-private"), deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "/data/app-staging"), deviceId);
            //DeviceService.setNewUserKey(deviceId);
            runCMD(String.Format("shell \"rm -rf {0} \"", "./etc/security/otacerts.zip"), deviceId);
            //runCMD(String.Format("shell \"rm -rf {0} \"", "/data/app"), deviceId);
            updateFileDateTimeModification("./system/product/priv-app", deviceId);
            updateFileDateTimeModification("./system/product/app", deviceId);
        }
        public static void cleanFacebookData(string deviceId)
        {
            forceStopPackage("com.facebook.katana", deviceId);
            forceStopPackage("com.facebook.lite", deviceId);
            Thread.Sleep(300);
            clearPackage("com.facebook.katana", deviceId);
            clearPackage("com.facebook.lite", deviceId);
            Thread.Sleep(300);
            clearPackage("com.facebook.katana", deviceId);
            clearPackage("com.facebook.lite", deviceId);
            Thread.Sleep(300);
            clearPackage("com.facebook.katana", deviceId);
            clearPackage("com.facebook.lite", deviceId);
            Thread.Sleep(300);
            runCMD("root", deviceId);
            runCMD("remount", deviceId);
            Thread.Sleep(300);
            runCMD(string.Format("shell \"rm -rf {0} \"", "com.facebook.katana/*"), deviceId);
            runCMD(string.Format("shell \"rm -rf {0} \"", "com.facebook.lite/*"), deviceId);
            Thread.Sleep(300);
            runCMD(string.Format("shell \"rm -rf {0} \"", "com.facebook.katana/*"), deviceId);
            runCMD(string.Format("shell \"rm -rf {0} \"", "com.facebook.lite/*"), deviceId);
            Thread.Sleep(300);
            runCMD(string.Format("shell \"rm -rf {0} \"", "com.facebook.katana/*"), deviceId);
            runCMD(string.Format("shell \"rm -rf {0} \"", "com.facebook.lite/*"), deviceId);
            Thread.Sleep(300);
        }
        public static void clearGMSWebview(string deviceId)
        {
            runCMD(String.Format("shell pm clear {0}", "com.android.webview"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.webview"), deviceId);
            runCMD(String.Format("shell pm clear {0}", "com.google.android.gms"), deviceId);
        }

        public static void cleanYoutubeData(string deviceId)
        {
            forceStopPackage(PackageName.YOUTUBE, deviceId);
            clearPackage(PackageName.YOUTUBE, deviceId);
        }
        public static void Start()
        {
            runCMD("adb start-server");
        }
        public static void Shutdown()
        {
            runCMD("adb kill-server");
        }
        public static void restartDevice(string deviceId)
        {
            runCMDRoot("reboot", deviceId);
        }

        public static void shellSetProp(Dictionary<string, string> settings, string deviceId)
        {
            //runCMD("root", deviceId);
            //runCMD("remount", deviceId);
            foreach (var item in settings)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    var value = Regex.Replace(item.Value, @"[^0-9a-zA-Z _./-]+:", "").Replace(" ", @"\ ");
                    runCMD(String.Format("shell setprop {0} \"{1}\"", item.Key, value), deviceId);
                }
            }
            //runCMD("unroot", deviceId);
        }

        public static bool settingTransfer(string deviceId, FileTransferAction action, string[] settingDesktopPaths, string[] settingTypePaths)
        {
            bool result = true;
            string fileTransferAction = Enum.GetName(typeof(FileTransferAction), action).ToLower();
            runCMD("root", deviceId);
            string remountResponse = runCMD("remount", deviceId);
            runCMD("shell \"mount -o rw,remount rootfs\"  ", deviceId);
            if (remountResponse.Contains("remount succeeded"))
            {
                foreach (var localPath in settingDesktopPaths)
                {
                    string command = "";
                    string settingAndroidPath = "";
                    switch (action)
                    {
                        case FileTransferAction.PUSH:
                            settingAndroidPath = localPath.Split('\\').Last();
                            command = String.Format("{0} \"{1}\" {2}", fileTransferAction, localPath + @"\build.prop", settingAndroidPath);
                            break;
                        case FileTransferAction.PULL:
                            settingAndroidPath = localPath.Split('\\').Last() + "/build.prop";
                            command = String.Format("{0} /{1} \"{2}\"", fileTransferAction, settingAndroidPath, localPath);
                            break;
                        default:
                            break;
                    }
                    if (!runCMD(command, deviceId).Contains("1 file pu"))
                    {
                        result = false;
                    }
                }

                foreach (var localPath in settingTypePaths)
                {
                    string command = "";
                    string settingAndroidPath = "";
                    switch (action)
                    {
                        case FileTransferAction.PUSH:
                            command = String.Format("{0} \"{1}\" {2}", fileTransferAction, localPath, Settings_Type.BASE_PATH);
                            break;
                        case FileTransferAction.PULL:
                            settingAndroidPath = Settings_Type.BASE_PATH + localPath.Split('\\').Last();
                            command = String.Format("{0} /{1} \"{2}\"", fileTransferAction, settingAndroidPath, localPath);
                            break;
                        default:
                            break;
                    }

                    if (!runCMD(command, deviceId).Contains("1 file pu"))
                    {
                        result = false;
                    }
                }
            }
            //runCMD("unroot", deviceId);
            return result;
        }
        public static void replaceBuildProp(string androidFilePath, Dictionary<string, string> newSettingValues, string deviceId)
        {
            runCMDRoot("root", deviceId);
            runCMDRoot("remount", deviceId);
            runCMDRoot("shell \"mount -o rw,remount rootfs\"", deviceId);

            string buildPropContent = runCMDRoot(string.Format("shell cat {0}", androidFilePath), deviceId);
            foreach (var item in newSettingValues)
            {
                int startIndex = buildPropContent.IndexOf(item.Key);
                if (item.Key == "ro.build.product")
                {
                    startIndex = buildPropContent.LastIndexOf(item.Key);
                }
                if (startIndex >= 0)
                {
                    int endIndex = buildPropContent.IndexOf('\n', startIndex);
                    string source = buildPropContent.Substring(startIndex, endIndex - startIndex - 1);
                    //const string NewValue = "\/";
                    string destination = string.Format("{0}={1}", item.Key, item.Value).Replace("/", @"\/");
                    runCMDRoot(string.Format("shell \"sed -i 's|{0}|{1}|g' {2}\" ", source, destination, androidFilePath), deviceId);
                }
            }
        }
        //public static bool writeSettingToFileBuildProp(string settingDesktopPath, Dictionary<string, string> newSettingValues)
        //{
        //    if (!File.Exists(settingDesktopPath))
        //    {
        //        return false;
        //    }

        //    string buildPropContent = File.ReadAllText(settingDesktopPath);

        //    foreach (var item in newSettingValues)
        //    {
        //        int startIndex = buildPropContent.IndexOf(item.Key.ToString());
        //        if (startIndex >= 0)
        //        {
        //            int endIndex = buildPropContent.IndexOf('\n', startIndex);
        //            buildPropContent = buildPropContent.Remove(startIndex, endIndex - startIndex)
        //                .Insert(startIndex, String.Format("{0}={1}", item.Key, item.Value));
        //        }
        //    }

        //    try
        //    {
        //        using (StreamWriter sw = new StreamWriter(settingDesktopPath))
        //        {
        //            sw.Write(buildPropContent);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception : {0}", ex.Message);
        //        return false;
        //    }

        //    return true;
        //}
        public static string getPhoneInfo(string deviceId, IPhoneSubInfo infoType, string optional_param = "")
        {
            string adbResponse = runCMD(String.Format("shell service call iphonesubinfo {0} {1}", (int)infoType, optional_param), deviceId);
            return convertFromResultParcel(adbResponse);
        }

        public static string getSetting(string key, string deviceId)
        {
            string response = runCMD(String.Format("shell settings get global {0}", key), deviceId).Trim();
            if (response.Equals("null"))
                return String.Empty;
            return response;
        }
        public static bool checkFileOnDevice(string path, string deviceId)
        {
            string response = runCMDRoot(String.Format("shell ls {0}", path), deviceId);
            if (response.Equals("No such file or directory"))
            {
                return false;
            }
            else if (string.IsNullOrEmpty(response))
            {
                return false;
            }
            return true;
        }

        public static void putSetting(string key, string value, string deviceId, string settingType = "global")
        {
            if (!string.IsNullOrEmpty(value))
            {
                runCMDRoot(String.Format("shell settings put {0} {1} '{2}'", settingType, key, value), deviceId);
            }
        }
        public static void deleteSetting(string key, string deviceId)
        {
            runCMDRoot(String.Format("shell settings delete global {0}", key), deviceId);
        }
        private static string convertFromResultParcel(string resultParcel)
        {
            var temp = resultParcel.Split(new[] { ".", "'" }, StringSplitOptions.RemoveEmptyEntries).Where(c => c.Length == 1);
            return String.Join(String.Empty, temp);
        }
        public static System.Collections.Generic.List<string> GetDevices()
        {
            var processStartInfo = new ProcessStartInfo("./Resources/adb", "devices")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var devices = new System.Collections.Generic.List<string>();

            foreach (var line in lines)
            {
                if (line.EndsWith("device"))
                {
                    // Tách ID thiết bị (chỉ lấy phần đầu trước "device")
                    string deviceId = line.Split('\t')[0];
                    devices.Add(deviceId);
                }
            }

            return devices;
        }


        /// <summary>
        /// source ui tool 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        /// 
        public static bool CaptureAndPullScreenshot(string deviceSerial)
        {
            try
            {
                string devicePath = "/sdcard/screenshot.png";
                string localFolder = $"./{deviceSerial}";
                string localFilePath = Path.Combine(localFolder, $"{deviceSerial}_Tesseract.png");

                Directory.CreateDirectory(localFolder);

                runCMDRootTool($" -s {deviceSerial} shell screencap -p {devicePath}");
                runCMDRootTool($" -s {deviceSerial} pull {devicePath} \"{localFilePath}\"");
                runCMDRootTool($" -s {deviceSerial} shell rm {devicePath}");

                return File.Exists(localFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ADB screenshot failed: {ex.Message}");
                return false;
            }
        }


        public static bool IsDeviceActive(string deviceId)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "./Resources/adb.exe";
                process.StartInfo.Arguments = $"-s {deviceId} shell getprop sys.boot_completed";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return !string.IsNullOrEmpty(output) && output.Contains("1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking device status: {ex.Message}");
                return false;
            }
        }
        public static string ExecuteADBCommandDetail(string deviceID, string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "./Resources/adb.exe",
                Arguments = $"-s {deviceID} {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
        public static void ExecuteAdbCommand(string command, string deviceId)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "./Resources/adb.exe",
                Arguments = $"-s {deviceId} {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process adbProcess = Process.Start(startInfo);
            adbProcess.WaitForExit();
        }
        public static void ExecuteAdbCommand(string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "adb.exe"),
                    Arguments = arguments, // chỉ truyền arguments, không cần "/c"
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Đợi tiến trình kết thúc (đồng bộ)
                    process.WaitForExit();

                    // Đọc output và error đồng bộ
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("ADB Output: " + output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("ADB Error: " + error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gọi adb: " + ex.Message);
            }
        }


        public static string[] GetConnectedDevices()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = @"./Resources/adb.exe",
                Arguments = "devices",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = Process.Start(startInfo);
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            var devices = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(line => !line.StartsWith("List") && !line.StartsWith("---------"))
                                 .Select(line => line.Split('\t')[0])
                                 .ToArray();

            return devices;
        }
        public static bool IsDeviceOnline(string deviceId)
        {
            var process = new Process();
            process.StartInfo.FileName = "./Resources/adb";
            process.StartInfo.Arguments = $"-s {deviceId} shell getprop sys.boot_completed";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return !string.IsNullOrEmpty(output) && output.Contains("1");
        }

        public static string FindBoundsByText(string xmlPath, string text)
        {
            try
            {
                if (!File.Exists(xmlPath))
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.

                XDocument doc = XDocument.Load(xmlPath);
                XElement root = doc.Root;

#pragma warning disable CS8604 // Possible null reference argument.
                XElement foundNode = FindNodeWithText(root, text);
#pragma warning restore CS8604 // Possible null reference argument.
                if (foundNode == null)
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                XAttribute boundsAttr = foundNode.Attribute("bounds");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (boundsAttr == null)
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.

                return boundsAttr.Value;
            }
            catch
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }



        // Đệ quy tìm node có attribute text = text
        private static XElement FindNodeWithText(XElement element, string text)
        {
            if (element.Attribute("text")?.Value == text)
                return element;

            foreach (var child in element.Elements())
            {
                XElement found = FindNodeWithText(child, text);
                if (found != null)
                    return found;
            }
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        // Phân tích chuỗi bounds "[left,top][right,bottom]" thành tọa độ tâm (x,y)
        public static (int x, int y) ParseCenter(string bounds)
        {
            if (string.IsNullOrWhiteSpace(bounds))
                throw new ArgumentException("Bounds string is null or empty.");

            var parts = bounds.Split(new[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                throw new FormatException("Bounds string is not in expected format.");

            int left = int.Parse(parts[0]);
            int top = int.Parse(parts[1]);
            int right = int.Parse(parts[2]);
            int bottom = int.Parse(parts[3]);

            int centerX = (left + right) / 2;
            int centerY = (top + bottom) / 2;

            return (centerX, centerY);
        }

        public static void WaitFileExists(string filePath, int timeoutMs = 5000)
        {
            int waited = 0;
            while (!File.Exists(filePath) && waited < timeoutMs)
            {
                System.Threading.Thread.Sleep(100);
                waited += 100;
            }

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {filePath} was not found after waiting {timeoutMs}ms");
        }

        public static void EnsureFileDeleted(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                while (File.Exists(path))
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        /* public static string EscapeAdbInputText(string input)
         {
             if (string.IsNullOrEmpty(input))
                 return string.Empty;

             var sb = new StringBuilder();

             foreach (char c in input)
             {
                 if (c == ' ')
                 {
                     sb.Append("%s");
                 }
                 else if (char.IsLetterOrDigit(c))
                 {
                     sb.Append(c);
                 }
                 else
                 {
                     sb.AppendFormat("%{0:X2}", (int)c);
                 }
             }

             return sb.ToString();
         }*/
        public static string EscapeAdbInputText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (input.Contains(" "))
            {
                var sb = new StringBuilder();
                foreach (char c in input)
                {
                    if (c == ' ')
                        sb.Append("%s");
                    else
                        sb.Append(c); // giữ nguyên tất cả ký tự, kể cả đặc biệt
                }
                return sb.ToString();
            }
            else
            {
                input = input.Replace("'", "'\\''");
                return $"'{input}'";
            }
        }



        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }
        public static string ExecuteAdbCommandString(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        /// <summary>
        /// end
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static DeviceStatus getDeviceStatus(string deviceId)
        {
            var adbRootRunning = getProp("init.svc.adb_root", deviceId).Contains("running");
            var adbRootRunning1 = getProp("lineage.service.adb.root", deviceId).Trim().Equals("1");
            var adbRootRunning2 = runCMDRoot("root", deviceId).Contains("running");
            if (adbRootRunning || adbRootRunning1 || adbRootRunning2)
            {
                return DeviceStatus.ReadyToChange;
            }
            return DeviceStatus.Undefined;
        }
        public static List<Device> getCurrentDevices()
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            var devices = new List<Device>();
            string command = "adb devices";
            string result = runCMD(command);
            //watch.Stop(); Console.WriteLine("1.Time for command {0} takes {1}s", command, watch.ElapsedMilliseconds);
            //watch.Restart();
            foreach (string device in result.Replace("List of devices attached\r\n", String.Empty).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var deviceId = device.Replace("\tdevice", "").Replace("\toffline", "");
                if (!deviceId.Contains("emulator"))
                {
                    devices.Add(new Device
                    {
                        DeviceId = deviceId,
                        Status = device.Contains("device") ? getDeviceStatus(deviceId) : DeviceStatus.Undefined,
                        HasSimCardInserted = ADBService.hasSimcardInserted(deviceId),
                        GoogleEmail = ADBService.getOneAccountGoogle(deviceId),
                        Packages = string.Join(";", ADBService.get3rdPackagesFromDevice(deviceId))
                    });
                }
            }
            //watch.Stop();
            //Console.WriteLine("2.Time taken getCurrentDevices {0}s", watch.ElapsedMilliseconds);
            return devices;

        }
        public static Device getDeviceBySerial(string deviceId)
        {
            try
            {
                var device = new Device
                {
                    DeviceId = deviceId,
                    Status = DeviceStatus.ReadyToChange,
                    HasSimCardInserted = hasSimcardInserted(deviceId),
                    GoogleEmail = getOneAccountGoogle(deviceId),
                    Packages = string.Join(";", get3rdPackagesFromDevice(deviceId)),
                    SerialNo = getSerialno(deviceId),
                    CodeName = getDeviceModel(deviceId)

                };
                return device;
            }
            catch
            {
                return new Device();
            }
        }
        public static DeviceCodeName getDeviceModel(string deviceId)
        {
            var result = getProp("ro.product.name", deviceId);
            if (deviceId.Length < 12)
            {
                return DeviceCodeName.STARLTE;
            }
            return DeviceCodeName.STARLTE;
        }
        public static List<Device> getConnectedDevices()
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            var devices = new List<Device>();
            string command = "adb devices";
            string result = runCMD(command);
            //watch.Stop(); Console.WriteLine("1.Time for command {0} takes {1}s", command, watch.ElapsedMilliseconds);
            //watch.Restart();
            foreach (string device in result.Replace("List of devices attached\r\n", String.Empty).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!device.Contains("emulator"))
                {
                    var deviceId = device.Substring(0, device.IndexOf('\t'));
                    var status = device.Contains("\tdevice") ? DeviceStatus.Online : DeviceStatus.Offline;
                    devices.Add(new Device
                    {
                        DeviceId = deviceId,
                        Status = status
                    });
                }
            }
            //watch.Stop();
            //Console.WriteLine("2.Time taken getCurrentDevices {0}s", watch.ElapsedMilliseconds);
            return devices;

        }

        private static string runCMD(string commandline, int timeout = 0)
        {
            //return CmdProcessSingleton.Instance.ExecuteCommand(String.Format("/C {0}", commandline));
            return CmdProcess.ExecuteCommand(string.Format("/C {0}", commandline), timeout);
        }
        private static string runCMDRootTool(string commandline, int timeout = 0)
        {
            //return CmdProcessSingleton.Instance.ExecuteCommand(String.Format("/C {0}", commandline));
            return CmdProcess.ExecuteCommandRoot(commandline);
        }
        private static string runCMD(string commandline, string deviceId, int timeout = 0)
        {
            Console.WriteLine(string.Format("/C adb -s {0} {1}", deviceId, commandline));
            //return CmdProcessSingleton.Instance.ExecuteCommand(string.Format("/C adb -s {0} {1}", deviceId, commandline));
            return CmdProcess.ExecuteCommand(string.Format("/C adb -s {0} {1}", deviceId, commandline), timeout);
        }

        private static string runCMDRoot(string commandline, string deviceId, int timeout = 0)
        {
            string adbArgs = $"-s {deviceId} {commandline}";
            Console.WriteLine(adbArgs);
            return CmdProcess.ExecuteCommandRoot(adbArgs, timeout);
        }

        public static void FakeLocation(string latitude, string longitude, string deviceId)
        {
            rootAndRemount(deviceId);
            runCMDRoot($"shell settings put global mi_latitude {latitude}", deviceId);
            runCMDRoot($"shell settings put global mi_longitude {longitude}", deviceId);
        }

        public static void OpenUrl(string url, string deviceId)
        {
            runCMDRoot($"shell am start -a android.intent.action.VIEW -d {url}", deviceId);
        }
        public static void ScreenShotDevice(string device)
        {
            runCMDRoot($"shell screencap -p /sdcard/screen.png", device);
        }
        public static void Dispose()
        {
            CmdProcessSingleton.Instance.Dispose();
        }
        public static Dictionary<string, string> getSystemProperties(string deviceId)
        {
            var result = new Dictionary<string, string>();
            var systemProperties = runCMD("shell getprop", deviceId).Trim().Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (var property in systemProperties)
            {
                if (!string.IsNullOrEmpty(property))
                {
                    try
                    {
                        var temp = property.Replace("[", String.Empty).Replace("]", String.Empty);//.Replace(" ", String.Empty);
                        var key = temp.Substring(0, temp.IndexOf(":"));
                        var value = temp.Substring(temp.IndexOf(":") + 2);
                        result.Add(key, value);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex.Message);
#endif
                    }

                }
            }
            Console.WriteLine("Done ADB getSystemProperties");
            return result;
        }

        public static string getOneAccountGoogle(string deviceId)
        {
            runCMD("root", deviceId);
            string xmlString = runCMD("shell cat /data/system/sync/accounts.xml", deviceId);
            if (string.IsNullOrEmpty(xmlString))
                return string.Empty;
            try
            {
                return XmlService.getGoogleAccountFromXML(xmlString);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static string getAccountsGoogleNoRoot(string deviceId)
        {
            try
            {
                string result = runCMD("shell dumpsys account | findstr \"Account {name=.*type=com.google}\"", deviceId);
                if (!string.IsNullOrEmpty(result))
                {
                    string pattern = @"Account {name=(.*?),";
                    Match match = Regex.Match(result, pattern);

                    if (match.Success)
                    {
                        string email = match.Groups[1].Value;
                        return email;
                    }
                    else
                    {
                        Console.WriteLine("Email not found.");
                        return String.Empty;
                    }
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        public static bool AccountExist(string account, string deviceId)
        {
            string xmlString = runCMD("shell cat /data/system/sync/accounts.xml", deviceId);
            return xmlString.ToLower().Contains(account.ToLower());
        }

        public static List<String> get3rdPackagesFromDevice(string deviceId)
        {
            return runCMD("shell pm list packages -3", deviceId)
                .Replace("package:", "")
                .Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .ToList<String>();
        }

        public static bool HasPackage(string pkg, string deviceId)
        {
            return runCMD($"shell \"pm list packages | grep {pkg}\"", deviceId).Trim() != "";
        }

        public static void wipePackage(string packageName, string deviceId)
        {
            forceStopPackage(packageName, deviceId);
            runCMD(string.Format("shell pm clear {0}", packageName), deviceId);
            runCMD(string.Format("shell pm clear {0}", packageName), deviceId);
            runCMD(string.Format("shell pm clear {0}", packageName), deviceId);
            runCMD(string.Format("shell pm clear {0}", packageName), deviceId);
            runCMD(string.Format("shell pm clear {0}", packageName), deviceId);
            //runCMD(String.Format("shell \"rm -rf {0}/* \"", packageName), deviceId);
        }

        public static void restartZygoteService(string deviceId)
        {
            //runCMD("root", deviceId);
            runCMD("shell setprop ctl.restart zygote", deviceId);
        }

        public static string getGSFNumber(string deviceId)
        {
            const string gsfXMLTokenPath = "data/data/com.google.android.gms/files/checkin_id_token";
            var response = runCMD(string.Format("shell cat {0}", gsfXMLTokenPath), deviceId);
            if (!string.IsNullOrEmpty(response) && !response.Contains("No such file or directory"))
                return response.Substring(0, response.IndexOf(":"));
            return string.Empty;
        }

        public static string getSerialno(string deviceId)
        {
            return runCMD("get-serialno", deviceId).Replace("\r\n", "");
        }

        public static bool isLoadCompleted(string deviceId)
        {
            //var str = runCMD("shell getprop sys.boot_completed", deviceId).Replace("\r\n", "");
            //return str.Equals("1");
            var str = runCMD("shell getprop init.svc.bootanim", deviceId).Trim();
            return str.Equals("stopped");
        }

        public static bool isWifiConnected(string deviceId)
        {
            var str = runCMDRoot("shell ping -c 4 google.com", deviceId).Replace("\r\n", "");
            return str.Contains("4 packets transmitted");
        }
        public static void pingDevice(string message, string deviceId)
        {
            runCMD($"shell am start -a android.intent.action.INSERT -t vnd.android.cursor.dir/contact -e name \"{message}\"", deviceId);
            runCMD($"shell am start -a android.intent.action.SENDTO -d sms:\"{message}\"", deviceId);
            //adb shell input keyevent 26
        }

        public static string getConnectingWifiSSID(string deviceId)
        {
            try
            {
                var result = runCMD("shell \"dumpsys netstats | grep -E 'iface=wlan.*networkId' | head -n 1\"", deviceId);
                return result.Split('\"')[1];
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return string.Empty;
            }
        }
        public static string getPackageVersion(string package, string deviceId)
        {
            try
            {
                var res = runCMD(string.Format("shell \"dumpsys package {0} | grep versionName | tail -n 1 | awk '{{print $1}}'\"", package), deviceId);
                return res.Substring(res.IndexOf('=') + 1).Trim();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return string.Empty;
            }
        }
        public static string getOTACerts(string deviceId)
        {
            return runCMD("shell \"cat /system/etc/security/otacerts.zip\"", deviceId);
        }
        public static DeviceCodeName getDeviceCodeName(string deviceId)
        {
            var serialLength = deviceId.Length;
            var certs = getOTACerts(deviceId);
            switch (serialLength)
            {
                case 8:
                    {
                        //
                        // check certs of jasmine sprout
                        //
                        return DeviceCodeName.JASMINE_SPROUT;
                    }

                case 18:
                    {
                        if (certs.Contains(OTACerts.HEROLTE))
                            return DeviceCodeName.HEROLTE;
                        goto default;
                    }
                default: return DeviceCodeName.TISSOT;
            }
        }
        public static DeviceCodeName getDeviceCodeNameByID(string deviceId)
        {
            if (deviceId.Length >= 12)
            {
                return DeviceCodeName.STARLTE;
            }
            else
            {
                return DeviceCodeName.STARLTE;
            }
        }
        public static string getCurrentProxyConfig(string deviceId)
        {
            var proxy = getSetting("http_proxy", deviceId);
            if (string.IsNullOrEmpty(proxy) || proxy.Equals(":0"))
            {
                proxy = getProxyWifiManual(deviceId);
                //var res = runCMD("shell \"dumpsys wifi | grep HttpProxy | tail -1 | sed -e 's|.*HttpProxy:\\(.*\\)xl.*|\\1|'\"", deviceId);
                //var entries = res.Split(new char[] { '[', ']' }).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim());
                //proxy = string.Join(":", entries);
            }
            return proxy;
        }
        public static string getProxyWifiManual(string deviceId)
        {
            var proxy = "";
            var xml = runCMD("shell cat /data/misc/wifi/WifiConfigStore.xml", deviceId);
            var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), deviceId, "WifiConfigStore.xml");
            LocalFileService.writeTextFile(fileName, xml);
            var ssid = getConnectedWifiSSID(deviceId);
            var wifiProxy = XmlService.getProxyOverXMLWifiConfig(fileName, ssid);
            if (wifiProxy.Count == 2)
            {
                proxy = wifiProxy["host"] + ":" + wifiProxy["port"];
            }
            return proxy;
        }
        public static string getConnectedWifiSSID(string deviceId)
        {
            var wifiName = "";
            var wifiInfo = runCMD("shell \"dumpsys wifi | grep -E 'SSID=' | tail -1\"", deviceId);
            if (!string.IsNullOrEmpty(wifiInfo) && wifiInfo.Contains("SSID="))
            {
                wifiName = wifiInfo.Split(',').FirstOrDefault(x => x.Contains("SSID=")).Split('=')[1];//.Replace("\"", string.Empty);
            }
            return wifiName;
        }

        public static string getClipboard(string deviceId)
        {
            // Install clipboard app before start
            runCMD("shell \"am startservice ca.zgrs.clipper/.ClipboardService\"", deviceId); // start clipboard service
            return runCMD("shell \"am broadcast -a clipper.get\"", deviceId); // get clipboard value
        }

        //public static void setRandomizedWifiMacAddress(string value, string deviceId)
        //{
        //    runCMD("root", deviceId);
        //    runCMD(String.Format("shell \"ip link set wlan0 address {0}\"", value), deviceId);
        //}
        //public static List<string> runCMD(string commandline, params string[] deviceIdList)
        //{
        //    List<string> adbResponses = new List<string>();
        //    foreach (var deviceId in deviceIdList)
        //    {
        //        adbResponses.Add(runCMD(commandline, deviceId));
        //    }
        //    return adbResponses;
        //}

        //public static bool zipPackages(string deviceId, string fileZipName, params string[] listFileNeedToZip)
        //{
        //    //adb shell "zip -r /data/product.zip /product";
        //    StringBuilder listFiles = new StringBuilder();
        //    foreach (var file in listFileNeedToZip)
        //    {
        //        listFiles.Append(file + " ");
        //    }
        //    var adbResponse = runCMD(String.Format("shell \"zip -r {0} {1} \"", fileZipName, listFiles), deviceId);
        //    return (adbResponse.Contains("adding") && !adbResponse.Contains("error"));
        //}

        public static string GetRomVersion(string deviceId)
        {
            return getProp("ro.android.DateTime", deviceId);
        }

        public static string getVersionCode(string package, string deviceId)
        {
            var adbResponse = runCMD(string.Format("shell \"dumpsys package {0} | grep versionCode\"", package), deviceId);
            return adbResponse.Split(new[] { ' ', '=', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "0";
        }
        public static string GetWebviewPackageName(string deviceId)
        {
            var rs = runCMD("shell \"pm list packages | grep webview\"", deviceId).Replace("package:", "").Trim();
            return rs ?? PackageName.DEFAULT_WEBVIEW;
        }
        public static string getVersionName(string package, string deviceId)
        {
            var adbResponse = runCMD(string.Format("shell \"dumpsys package {0} | grep versionName\"", package), deviceId).Trim();
            return adbResponse.Split(new[] { ' ', '=', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "0";
        }
        public static bool installGMSAPK(string deviceIP)
        {
            var command = "shell pm install -r //data/local/tmp/apps/gms.apk";
            return runCMD(command, deviceIP).Contains("Success");
        }
        public static void connectWifiSSID(string ssid, string password, string deviceId)
        {
            var sdk = int.Parse(getProp("ro.build.version.sdk", deviceId).Trim());
            var systemPathWifiConfig = "//data/misc/wi4fi/WifiConfigStore.xml";
            if (sdk > 28)
            {
                systemPathWifiConfig = "//data/misc/apexdata/com.android.wifi/WifiConfigStore.xml";
            }
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", $"WifiConfigStore-{sdk}.xml");
            runCMD($"shell \"rm -rf {systemPathWifiConfig}*\"", deviceId);
            pullOrPushFile(FileTransferAction.PUSH, path, systemPathWifiConfig, deviceId);
            shellStringReplace(systemPathWifiConfig, "SSID_NAME", ssid, deviceId);
            shellStringReplace(systemPathWifiConfig, "SSID_PASSWORD", password, deviceId);

            setPermission("600", systemPathWifiConfig, deviceId);
            setOwnerFile("1000", "1000", systemPathWifiConfig, deviceId);
        }
        public static void connectWifiSSIDWithRandomMAC(string ssid, string password, string deviceId)
        {
            var sdk = int.Parse(getProp("ro.build.version.sdk", deviceId).Trim());
            var systemPathWifiConfig = "//data/misc/wifi/WifiConfigStore.xml";
            if (sdk > 28)
            {
                systemPathWifiConfig = "//data/misc/apexdata/com.android.wifi/WifiConfigStore.xml";
            }
#pragma warning disable CS8604 // Possible null reference argument.
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", $"WifiConfigStoreRandomMAC-{sdk}.xml");
#pragma warning restore CS8604 // Possible null reference argument.
            runCMD($"shell \"rm -rf {systemPathWifiConfig}*\"", deviceId);
            pullOrPushFile(FileTransferAction.PUSH, path, systemPathWifiConfig, deviceId);
            shellStringReplace(systemPathWifiConfig, "SSID_NAME", ssid, deviceId);
            shellStringReplace(systemPathWifiConfig, "SSID_PASSWORD", password, deviceId);
            shellStringReplace(systemPathWifiConfig, "RANDOM_MAC", RandomService.generateMacAddress(), deviceId);
            setPermission("600", systemPathWifiConfig, deviceId);
            setOwnerFile("1000", "1000", systemPathWifiConfig, deviceId);
        }
        public static void connectWifiByWifiConfigStoreFile(string filePath, string formatContent, string deviceId)
        {
            var sdk = int.Parse(getProp("ro.build.version.sdk", deviceId).Trim());
            var systemPathWifiConfig = "//data/misc/wi4fi/WifiConfigStore.xml";
            if (sdk > 28)
            {
                systemPathWifiConfig = "//data/misc/apexdata/com.android.wifi/WifiConfigStore.xml";
            }
            runCMD($"shell \"rm -rf {systemPathWifiConfig}*\"", deviceId);
            pullOrPushFile(FileTransferAction.PUSH, filePath, systemPathWifiConfig, deviceId);
            setPermission("600", systemPathWifiConfig, deviceId);
            setOwnerFile("1000", "1000", systemPathWifiConfig, deviceId);
        }

        public static List<string> getSSIDWifi(string deviceId)
        {
            string path = "/data/misc/apexdata/com.android.wifi/WifiConfigStore.xml";
            List<string> result = new List<string>();
            string SSID = runCMD($"shell cat {path} | findstr SSID", deviceId);
            string extractSSID = ExtractSSIDOrPass(SSID);
            result.Add(extractSSID);
            string Password = runCMD($"shell cat {path} | findstr PreSharedKey", deviceId);
            string extractPass = ExtractSSIDOrPass(Password);
            result.Add(extractPass);
            Console.WriteLine(result);
            return result;
        }
        public static string ExtractSSIDOrPass(string input)
        {
            string pattern = @"&quot;([^&]*)&quot;";
            MatchCollection matches = Regex.Matches(input, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string extractedValue = match.Groups[1].Value;
                    Console.WriteLine(extractedValue);
                    return extractedValue;
                }
            }
            return string.Empty;
        }
        public static void openWifiSettings(string deviceId)
        {
            runCMDRoot($"shell am start -a android.settings.WIFI_SETTINGS", deviceId);
        }

        public static void InputTextFromNumericKeyboard(string text, string deviceId)
        {
            var listPoints = getKeysCoordinatorNumeric(text);
            for (int i = 0; i < listPoints.Count(); i++)
            {
                var keyStrokePoint = listPoints[i];
                inputTapEvent(keyStrokePoint.X, keyStrokePoint.Y, deviceId);
                Thread.Sleep(RandomService.randomInRange(0, 123));
            }
        }


        public static List<Point> getKeysCoordinatorNumeric(string text, DeviceCodeName deviceCodeName = DeviceCodeName.TISSOT)
        {
            List<Point> result = new List<Point>();
            Dictionary<char, Point> keyDic = new Dictionary<char, Point>();
            if (deviceCodeName == DeviceCodeName.TISSOT)
            {
                keyDic.Add('1', new Point(140, 1325));
                keyDic.Add('2', new Point(425, 1325));
                keyDic.Add('3', new Point(710, 1325));
                keyDic.Add('4', new Point(140, 1500));
                keyDic.Add('5', new Point(425, 1500));
                keyDic.Add('6', new Point(710, 1500));
                keyDic.Add('7', new Point(140, 1650));
                keyDic.Add('8', new Point(425, 1650));
                keyDic.Add('9', new Point(710, 1650));
                keyDic.Add('0', new Point(425, 1830));
                keyDic.Add('-', new Point(950, 1325));
                keyDic.Add(',', new Point(950, 1300));
                keyDic.Add('.', new Point(140, 1830));
            }
            if (deviceCodeName == DeviceCodeName.STARLTE)
            {
                keyDic.Add('1', new Point(RandomService.randomInRange(30, 360), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('2', new Point(RandomService.randomInRange(400, 750), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('3', new Point(RandomService.randomInRange(790, 1130), RandomService.randomInRange(1930, 2080)));

                keyDic.Add('4', new Point(RandomService.randomInRange(30, 360), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('5', new Point(RandomService.randomInRange(400, 750), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('6', new Point(RandomService.randomInRange(790, 1130), RandomService.randomInRange(2150, 2300)));

                keyDic.Add('7', new Point(RandomService.randomInRange(30, 360), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('8', new Point(RandomService.randomInRange(400, 750), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('9', new Point(RandomService.randomInRange(790, 1130), RandomService.randomInRange(2370, 2520)));

                keyDic.Add('0', new Point(RandomService.randomInRange(400, 750), RandomService.randomInRange(2600, 2730)));
                keyDic.Add('-', new Point(RandomService.randomInRange(1170, 1380), RandomService.randomInRange(1930, 2080)));
                //keyDic.Add(',', new Point(RandomService.randomInRange(30, 360), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('.', new Point(RandomService.randomInRange(1170, 1380), RandomService.randomInRange(2150, 2300)));
            }

            foreach (var character in text)
            {
                result.Add(keyDic[char.ToLower(character)]);
            }
            return result;
        }

        // Deprecated
        public static List<Point> inputByKeysCoordinator(string text, string deviceId) //renamed from getKeysCoordinator
        {
            List<Point> result = new List<Point>();
            Dictionary<char, Point> keyDic = new Dictionary<char, Point>();
            keyDic.Add('q', new Point(100, 1300));
            keyDic.Add('w', new Point(200, 1300));
            keyDic.Add('e', new Point(300, 1300));
            keyDic.Add('r', new Point(400, 1300));
            keyDic.Add('t', new Point(500, 1300));
            keyDic.Add('y', new Point(600, 1300));
            keyDic.Add('u', new Point(700, 1300));
            keyDic.Add('i', new Point(800, 1300));
            keyDic.Add('o', new Point(900, 1300));
            keyDic.Add('p', new Point(1000, 1300));

            keyDic.Add('a', new Point(150, 1500));
            keyDic.Add('s', new Point(250, 1500));
            keyDic.Add('d', new Point(350, 1500));
            keyDic.Add('f', new Point(450, 1500));
            keyDic.Add('g', new Point(550, 1500));
            keyDic.Add('h', new Point(650, 1500));
            keyDic.Add('j', new Point(750, 1500));
            keyDic.Add('k', new Point(850, 1500));
            keyDic.Add('l', new Point(950, 1500));


            keyDic.Add('z', new Point(220, 1660));
            keyDic.Add('x', new Point(320, 1660));
            keyDic.Add('c', new Point(420, 1660));
            keyDic.Add('v', new Point(520, 1660));
            keyDic.Add('b', new Point(620, 1660));
            keyDic.Add('n', new Point(720, 1660));
            keyDic.Add('m', new Point(820, 1660));

            keyDic.Add('1', new Point(60, 1300));
            keyDic.Add('2', new Point(170, 1300));
            keyDic.Add('3', new Point(280, 1300));
            keyDic.Add('4', new Point(390, 1300));
            keyDic.Add('5', new Point(500, 1300));
            keyDic.Add('6', new Point(610, 1300));
            keyDic.Add('7', new Point(720, 1300));
            keyDic.Add('8', new Point(830, 1300));
            keyDic.Add('9', new Point(940, 1300));
            keyDic.Add('0', new Point(1050, 1300));
            keyDic.Add('.', new Point(850, 1830));
            keyDic.Add(',', new Point(220, 1830));
            keyDic.Add(' ', new Point(555, 1830));

            var acceptSpecials = new List<char> { ' ', ',', '.' };
            var emj = new List<char>();
            var prevIsSpecial = false;
            foreach (var character in text)
            {
                //if (char.IsUpper(character))
                //    result.Add(new Point(75, 1665));
                //if (char.IsDigit(character))
                //    result.Add(new Point(75, 1800));
                //result.Add(keyDic[char.ToLower(character)]);
                //if (char.IsDigit(character))
                //    result.Add(new Point(75, 1800));
                if (keyDic.TryGetValue(char.ToLower(character), out Point point) || acceptSpecials.Contains(character))
                {
                    if (prevIsSpecial)
                    {
                        inputTextBase64Event(new string(emj.ToArray()), deviceId);
                        emj.Clear();
                    }
                    prevIsSpecial = false;
                    if (char.IsUpper(character)) inputTapEvent(75, 1665, deviceId);
                    if (char.IsDigit(character)) inputTapEvent(75, 1800, deviceId);

                    inputTapEvent(point.X, point.Y, deviceId);
                    // if (char.IsUpper(character)) inputTapEvent(75, 1665, deviceId);
                    if (char.IsDigit(character)) inputTapEvent(75, 1800, deviceId);

                    Thread.Sleep(RandomService.randomInRange(0, 123));
                    continue;
                }
                emj.Add(character);
                prevIsSpecial = true;
            }
            if (emj.Count > 0)
            {
                inputTextBase64Event(new string(emj.ToArray()), deviceId);
                emj.Clear();
            }
            return result;//76,1665
        }

        public static void inputTextBase64Event(string inputText, string deviceIP)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(inputText);
            var inputBase64 = System.Convert.ToBase64String(plainTextBytes);
            runCMD("shell ime set com.android.adbkeyboard/.AdbIME", deviceIP); // Switch to adbkeyboard
            Thread.Sleep(555);
            runCMD(string.Format("shell am broadcast -a ADB_INPUT_B64 --es msg '{0}'", inputBase64), deviceIP);
            Thread.Sleep(555);
            runCMD("shell ime set com.android.inputmethod.latin/.LatinIME", deviceIP); //Switch back to default keyboard
        }

        // Deprecated
        public static string monkeyTextTranslation(string inputText, long delayTimePerChar = 500)
        {
            var translated = "";
            var chars = inputText.ToCharArray();
            foreach (char c in chars)
            {
                switch (c)
                {
                    case '@':
                        translated += "DispatchPress(KEYCODE_AT)\r\n";
                        break;
                    case '.':
                        translated += "DispatchString(.)\r\n";
                        break;
                    default:
                        translated += $"DispatchPress(KEYCODE_{c.ToString().ToUpper()})\r\n";
                        break;
                }
                translated += $"UserWait({delayTimePerChar})\r\n"; // wait in millis seconds
            }
            return translated;
        }

        public static string monkeyTapTranslation(Point point)
        {
            if (point == null || (point.X == 0 && point.Y == 0)) return string.Empty;
            return string.Format("Tap({0},{1})\r\n", point.X, point.Y);
        }

        public static List<Point> textToPointInKeyboard(string text, DeviceCodeName deviceCodeName = DeviceCodeName.TISSOT)
        {
            List<Point> result = new List<Point>();
            Dictionary<char, Point> keyDic = new Dictionary<char, Point>();
            var pLeftShift = new Point(0, 0);
            var pNumeric = new Point(0, 0);

            if (deviceCodeName == DeviceCodeName.TISSOT)
            {
                keyDic.Add('q', new Point(100, 1300));
                keyDic.Add('w', new Point(200, 1300));
                keyDic.Add('e', new Point(300, 1300));
                keyDic.Add('r', new Point(400, 1300));
                keyDic.Add('t', new Point(500, 1300));
                keyDic.Add('y', new Point(600, 1300));
                keyDic.Add('u', new Point(700, 1300));
                keyDic.Add('i', new Point(800, 1300));
                keyDic.Add('o', new Point(900, 1300));
                keyDic.Add('p', new Point(1000, 1300));

                keyDic.Add('a', new Point(150, 1500));
                keyDic.Add('s', new Point(250, 1500));
                keyDic.Add('d', new Point(350, 1500));
                keyDic.Add('f', new Point(450, 1500));
                keyDic.Add('g', new Point(550, 1500));
                keyDic.Add('h', new Point(650, 1500));
                keyDic.Add('j', new Point(750, 1500));
                keyDic.Add('k', new Point(850, 1500));
                keyDic.Add('l', new Point(950, 1500));


                keyDic.Add('z', new Point(220, 1660));
                keyDic.Add('x', new Point(320, 1660));
                keyDic.Add('c', new Point(420, 1660));
                keyDic.Add('v', new Point(520, 1660));
                keyDic.Add('b', new Point(620, 1660));
                keyDic.Add('n', new Point(720, 1660));
                keyDic.Add('m', new Point(820, 1660));
                keyDic.Add(' ', new Point(555, 1830));

                keyDic.Add('1', new Point(60, 1300));
                keyDic.Add('2', new Point(170, 1300));
                keyDic.Add('3', new Point(280, 1300));
                keyDic.Add('4', new Point(390, 1300));
                keyDic.Add('5', new Point(500, 1300));
                keyDic.Add('6', new Point(610, 1300));
                keyDic.Add('7', new Point(720, 1300));
                keyDic.Add('8', new Point(830, 1300));
                keyDic.Add('9', new Point(940, 1300));
                keyDic.Add('0', new Point(1050, 1300));
                keyDic.Add('.', new Point(850, 1830));
                keyDic.Add(',', new Point(220, 1830));

                pLeftShift = new Point(75, 1665);
                pNumeric = new Point(75, 1800);
            }
            if (deviceCodeName == DeviceCodeName.STARLTE)
            {
                keyDic.Add('q', new Point(RandomService.randomInRange(30, 125), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('w', new Point(RandomService.randomInRange(170, 270), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('e', new Point(RandomService.randomInRange(310, 420), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('r', new Point(RandomService.randomInRange(450, 550), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('t', new Point(RandomService.randomInRange(590, 700), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('y', new Point(RandomService.randomInRange(740, 840), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('u', new Point(RandomService.randomInRange(880, 990), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('i', new Point(RandomService.randomInRange(1030, 1130), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('o', new Point(RandomService.randomInRange(1170, 1280), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('p', new Point(RandomService.randomInRange(1320, 1415), RandomService.randomInRange(1930, 2080)));

                keyDic.Add('a', new Point(RandomService.randomInRange(95, 200), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('s', new Point(RandomService.randomInRange(235, 345), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('d', new Point(RandomService.randomInRange(380, 485), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('f', new Point(RandomService.randomInRange(525, 630), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('g', new Point(RandomService.randomInRange(665, 775), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('h', new Point(RandomService.randomInRange(810, 915), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('j', new Point(RandomService.randomInRange(960, 1065), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('k', new Point(RandomService.randomInRange(1100, 1205), RandomService.randomInRange(2150, 2300)));
                keyDic.Add('l', new Point(RandomService.randomInRange(1254, 1350), RandomService.randomInRange(2150, 2300)));


                keyDic.Add('z', new Point(RandomService.randomInRange(235, 340), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('x', new Point(RandomService.randomInRange(380, 485), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('c', new Point(RandomService.randomInRange(520, 630), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('v', new Point(RandomService.randomInRange(665, 700), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('b', new Point(RandomService.randomInRange(815, 915), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('n', new Point(RandomService.randomInRange(960, 1065), RandomService.randomInRange(2370, 2520)));
                keyDic.Add('m', new Point(RandomService.randomInRange(1105, 1210), RandomService.randomInRange(2370, 2520)));
                keyDic.Add(' ', new Point(RandomService.randomInRange(445, 980), RandomService.randomInRange(2630, 2700)));

                keyDic.Add('1', new Point(RandomService.randomInRange(30, 125), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('2', new Point(RandomService.randomInRange(170, 270), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('3', new Point(RandomService.randomInRange(310, 420), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('4', new Point(RandomService.randomInRange(450, 550), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('5', new Point(RandomService.randomInRange(590, 700), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('6', new Point(RandomService.randomInRange(740, 840), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('7', new Point(RandomService.randomInRange(880, 990), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('8', new Point(RandomService.randomInRange(1030, 1130), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('9', new Point(RandomService.randomInRange(1170, 1280), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('0', new Point(RandomService.randomInRange(1320, 1415), RandomService.randomInRange(1930, 2080)));
                keyDic.Add('.', new Point(RandomService.randomInRange(1105, 1205), RandomService.randomInRange(2600, 2730)));
                keyDic.Add(',', new Point(RandomService.randomInRange(235, 340), RandomService.randomInRange(2600, 2730)));

                pLeftShift = new Point(RandomService.randomInRange(35, 180), RandomService.randomInRange(2375, 2520));
                pNumeric = new Point(RandomService.randomInRange(20, 200), RandomService.randomInRange(2590, 2740));
            }

            foreach (var character in text)
            {
                if (keyDic.TryGetValue(char.ToLower(character), out Point point))
                {
                    if (char.IsUpper(character)) result.Add(pLeftShift);
                    if (char.IsDigit(character)) result.Add(pNumeric);
                    result.Add(point);
                    if (char.IsDigit(character)) result.Add(pNumeric);
                }
            }
            return result;
        }

        public static string monkeyWait(long delayTimePerTap = 500)
        {
            return $"UserWait({delayTimePerTap})\r\n";
        }

        public static string monkeyTapScriptFromPoints(List<Point> points, bool isRandomDelay = false, long delayTimePerTap = 500)
        {
            var script = "";
            foreach (var point in points)
            {
                script += monkeyTapTranslation(point);
                if (isRandomDelay)
                    delayTimePerTap = RandomService.randomInRange(250, 600);
                script += $"UserWait({delayTimePerTap})\r\n";
            }
            return script;
        }

        public static void monkeyInitFileScript(string scriptBody, string deviceId, string monkeyScriptSystemPath = "/data/local/monkeyscript")
        {
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var monkeyScriptPCPath = Path.Combine(exePath, $"{deviceId}/monkeyscript");

            var scriptHeader =
                    "type = raw events\r\n" +
                    "speed = 1\r\n" +
                    "start data >>\r\n";
            var script = string.Concat(scriptHeader, scriptBody);

            LocalFileService.writeTextFile(monkeyScriptPCPath, script);
            pullOrPushFile(FileTransferAction.PUSH, monkeyScriptPCPath, monkeyScriptSystemPath, deviceId);
        }

        public static void monkeyExecutionScript(string deviceId, string monkeyScriptSystemPath = "/data/local/monkeyscript")
        {
            runCMD($"shell \"monkey -f {monkeyScriptSystemPath} 1\"", deviceId);
        }

        public static bool isFileExist(string path, string deviceId)
        {
            var response = runCMD(string.Format("shell \"(ls {0} >> /dev/null 2>&1 && echo true) || echo false\"", path), deviceId);
            return response.Contains("true");
        }

        public static bool reInstallWebview(string webviewApkAndroidPath, string webviewPackage, string deviceId)
        {
            try
            {
                var result = runCMD($"shell \"pm install -r -d {webviewApkAndroidPath}\"", deviceId);
                if (result.Contains("Success"))
                {
                    moveFile($"/data/app/{webviewPackage}*/", "/system/product/app/webview/", deviceId);
                    runCMD($"shell pm uninstall {webviewPackage}", deviceId);
                    return true;
                }
                return false;
            }
            catch
            {
                Console.WriteLine("INSTALL WEBVIEW FAILED!!!");
                return false;
            }
        }

        public static void fakeLocalHostNameV6(string deviceId)
        {
            string ipv6 = RandomService.generateIpv6();
            string hostname = string.Concat(ipv6, "%wlan0");
            makeAndWriteToFiles(hostname, "/system/etc/hook_ipv6", deviceId);
        }

        public static void FakeTimezone(string timezone, string deviceId)
        {
            rootAndRemount(deviceId);
            runCMDRoot($"shell setprop persist.sys.timezone \"{timezone}\"", deviceId);
            runCMDRoot($"shell am broadcast -a android.intent.action.TIMEZONE_CHANGED", deviceId);
            runCMDRoot($"shell settings put system time_12_24 24", deviceId);
            runCMDRoot($"shell am broadcast -a android.intent.action.TIME_SET", deviceId);
            //runCMD($"shell \"cmd time_zone_detector suggest_telephony_time_zone --suggestion --slot_index 0 --zone_id {timezone} --quality multiple_same --match_type country\"", deviceId);
        }

        public static void AdjustVolume(bool isUp, string deviceId)
        {
            var val = isUp ? 1 : -1;
            runCMD($"shell \"service call audio 9 i32 3 i32 {val} i32 1\"", deviceId);
        }
        public static void AdjustVolume(int level, string deviceId)
        {
            runCMD($"shell \"service call audio 10 i32 3 i32 {level}\"", deviceId);
        }

        public static void SetFixedOrientation(string deviceId, bool isFixed = true)
        {
            var state = isFixed ? "enabled" : "disabled";
            runCMD($"shell \"wm set-fix-to-user-rotation {state}\"", deviceId);
        }

        public static void SetOrientation(Orientation orientation, string deviceId)
        {
            var state = (orientation is Orientation.LANDSCAPE) ? 1 : 0;
            runCMD($"shell \"wm set-user-rotation lock {state}\"", deviceId);
        }
        public static (int Width, int Height) GetScreenSize(string deviceId)
        {
            var width = 1440;
            var height = 2960;
            var res = runCMD("shell \"wm size\"", deviceId);
            var value = res.Substring(res.LastIndexOf(' '));
            int.TryParse(value.Split('x')[0], out width);
            int.TryParse(value.Split('x')[1], out height);
            return (width, height);
        }

        public static void SetScreenSize(int width, int height, string deviceId)
        {
            runCMD($"shell \"wm size {width}x{height}\"", deviceId);
        }
        public static void ResetScreenSize(string deviceId)
        {
            runCMD($"shell \"wm size reset\"", deviceId);
        }
        public static int GetScreenDensity(string deviceId)
        {
            var density = 560;
            var res = runCMD("shell \"wm density\"", deviceId);
            int.TryParse(res, out density);
            return density;
        }
        public static void SetScreenDensity(int dens, string deviceId)
        {
            runCMD($"shell \"wm density {dens}\"", deviceId);
        }
        public static void ResetScreenDensity(string deviceId)
        {
            runCMD($"shell \"wm density reset\"", deviceId);
        }
        public static string GetHexColorByCoord(Point p, string deviceId)
        {
            //screencap screen.dump; dd if='screen.dump' bs=4 count=1 skip=1725293 2>/dev/null | xxd -p
            var screenWith = GetScreenSize(deviceId).Width;
            var offset = screenWith * p.Y + p.X;
            var res = runCMD($"shell \"screencap screen.dump; dd if='screen.dump' bs=4 count=1 skip={offset} 2>/dev/null | xxd -p\"", deviceId)?.Trim();
            return res;
        }

        public static void OpenBrowserWithUrl(string url, string deviceId)
        {
            runCMDRoot(string.Format("shell am start -a android.intent.action.VIEW -d {0}", url), deviceId);
        }
        public static void ScaleDevice(string deviceId)
        {
            runCMD($"shell wm density 440", deviceId);
            Thread.Sleep(500);
            runCMD($"shell wm size 1080x2220", deviceId);
            Thread.Sleep(500);
            runCMD($"shell cmd overlay enable com.android.internal.systemui.navbar.gestural", deviceId);
        }
        public static void HideAndShowGestures(bool isHide, string deviceId)
        {
            if (isHide)
            {
                runCMD("shell cmd overlay enable com.android.internal.systemui.navbar.gestural", deviceId);
                Thread.Sleep(500);
            }
            else
            {
                runCMD("shell cmd overlay disable com.android.internal.systemui.navbar.gestural", deviceId);
                Thread.Sleep(500);
            }

        }

        public static string GetDumsysMediaSession(string deviceId)
        {
            return runCMD("shell dumpsys media_session", deviceId);
        }
        public static void DeleteString(string text, string deviceId)
        {
            string adbCommand = "shell input keyevent --longpress 67";
            for (int i = 0; i < text.Length; i++)
            {
                adbCommand += " 67";
            }
            runCMD(adbCommand, deviceId);
        }
        public static bool CheckPackageInstall(string package, string deviceId)
        {
            var packageReturn = runCMD($"shell pm list packages -3 | findstr {package}", deviceId);
            if (packageReturn.Contains(package))
            {
                return true;
            }
            return false;
        }
        ////OCR Rectangle of Nhat
        //public static async Task<string> OcrScreen(string deviceId, OcrEngine engine = null, bool isLower = true)
        //{
        //    return await LockAsync(async () =>
        //    {
        //        if (engine == null) engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
        //        using (var bitmap = await screenCapToBitmapAsync(deviceId))
        //        {
        //            var ocrResult = await engine.RecognizeAsync(bitmap);
        //            if (ocrResult.Text.Length > 0)
        //            {
        //                if (isLower) return ocrResult.Text.ToLower();
        //                return ocrResult.Text;
        //            }
        //            return "";
        //        }
        //    });
        //}
        //private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5, 5);
        //private static async Task<T> LockAsync<T>(Func<Task<T>> worker)
        //{
        //    await _semaphore.WaitAsync();
        //    try
        //    {
        //        return await worker();
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}
        //public static async Task<string> OcrScreenRectangle(string deviceId, Rectangle rectangle, OcrEngine engine = null, bool isLower = true)
        //{
        //    return await LockAsync(async () =>
        //    {
        //        if (engine == null)
        //            engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));

        //        using (var bitmap = await screenCapToBitmapAsync(deviceId, rectangle))
        //        {
        //            var ocrResult = await engine.RecognizeAsync(bitmap);
        //            if (ocrResult.Text.Length > 0)
        //            {
        //                if (isLower)
        //                    return ocrResult.Text.ToLower();
        //                return ocrResult.Text;
        //            }
        //            return "";
        //        }
        //    });
        //}
        //private static byte[] runCMDByteReturn(string commandline, string deviceId, int timeout = 0)
        //{
        //    Console.WriteLine(string.Format("/C adb -s {0} {1}", deviceId, commandline));
        //    //return CmdProcessSingleton.Instance.ExecuteCommand(string.Format("/C adb -s {0} {1}", deviceId, commandline));
        //    return CmdProcess.ExecuteCommandByteReturn(string.Format("/C adb -s {0} {1}", deviceId, commandline), timeout);
        //}
        //public static async Task<SoftwareBitmap> screenCapToBitmapAsync(string deviceId)
        //{
        //    byte[] buff = runCMDByteReturn(@" exec-out screencap -p ", deviceId);
        //    if (buff == null)
        //        return null;

        //    using (MemoryStream inMemoryCopy = new MemoryStream(buff))
        //    {
        //        var decoder = await BitmapDecoder.CreateAsync(inMemoryCopy.AsRandomAccessStream());
        //        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        //    }
        //}

        //public static async Task<SoftwareBitmap> screenCapToBitmapAsync(string deviceId, Rectangle rectangle)
        //{
        //    byte[] buff = runCMDByteReturn($" exec-out screencap -p > /sdcard/screenshot.png && exec-out convert /sdcard/screenshot.png -crop {rectangle.Width}x{rectangle.Height}+{rectangle.X}+{rectangle.Y} /sdcard/cropped.png && exec-out cat /sdcard/cropped.png", deviceId);
        //    if (buff == null)
        //        return null;

        //    using (MemoryStream inMemoryCopy = new MemoryStream(buff))
        //    {
        //        var decoder = await BitmapDecoder.CreateAsync(inMemoryCopy.AsRandomAccessStream());
        //        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        //    }
        //}
        //public static async Task<Rect> GetTextLocation(string deviceId, string targetText, OcrEngine engine = null)
        //{
        //    if (engine == null)
        //        engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));

        //    using (var bitmap = await screenCapToBitmapAsync(deviceId))
        //    {
        //        var ocrResult = await engine.RecognizeAsync(bitmap);
        //        Console.WriteLine(ocrResult);
        //        if (ocrResult.Text.Length > 0)
        //        {
        //            var lines = ocrResult.Lines;
        //            foreach (var line in lines)
        //            {
        //                foreach (var word in line.Words)
        //                {
        //                    if (word.Text.ToLower().Contains(targetText.ToLower()))
        //                    {
        //                        return word.BoundingRect;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return Rect.Empty;
        //}

        //public static async Task<List<Rect>> FindTextCoordinates(string deviceId, string targetText, OcrEngine engine = null)
        //{
        //    return await LockAsync(async () =>
        //    {
        //        if (engine == null)
        //            engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
        //        using (var bitmap = await screenCapToBitmapAsync(deviceId))
        //        {
        //            var ocrResult = await engine.RecognizeAsync(bitmap);
        //            if (ocrResult.Text.Length > 0)
        //            {
        //                List<Rect> coordinates = new List<Rect>();
        //                foreach (var line in ocrResult.Lines)
        //                {
        //                    foreach (var word in line.Words)
        //                    {
        //                        if (word.Text.Equals(targetText, StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            coordinates.Add(word.BoundingRect);
        //                        }
        //                    }
        //                }
        //                return coordinates;
        //            }
        //            return null;
        //        }
        //    });
        //}
        //public static async Task<List<Rect>> FindMultiTextCoordinatesOCR(string targetText, string deviceId, OcrEngine engine = null)
        //{
        //    return await LockAsync(async () =>
        //    {
        //        // Initializing engine for detecting text
        //        if (engine == null)
        //            engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
        //        // Capture the screen
        //        using (var bitmap = await screenCapToBitmapAsync(deviceId))
        //        {
        //            // use engine for detect text on the screen
        //            var ocrResult = await engine.RecognizeAsync(bitmap);
        //            if (ocrResult.Text.Length > 0)
        //            {
        //                List<Rect> coordinates = new List<Rect>();
        //                foreach (var line in ocrResult.Lines)
        //                {
        //                    if (line.Text.ToLowerInvariant().Contains(targetText.ToLowerInvariant()))
        //                    {
        //                        double minX = double.MaxValue;
        //                        double minY = double.MaxValue;
        //                        double maxX = double.MinValue;
        //                        double maxY = double.MinValue;
        //                        foreach (var word in line.Words)
        //                        {
        //                            minX = Math.Min(minX, word.BoundingRect.Left);
        //                            minY = Math.Min(minY, word.BoundingRect.Top);
        //                            maxX = Math.Max(maxX, word.BoundingRect.Right);
        //                            maxY = Math.Max(maxY, word.BoundingRect.Bottom);
        //                        }
        //                        Rect lineBoundingRect = new Rect(minX, minY, maxX - minX, maxY - minY);
        //                        coordinates.Add(lineBoundingRect);
        //                    }
        //                }
        //                return coordinates;
        //            }
        //            return null;
        //        }
        //    });
        //}
        //public static void TouchMultiTextByOCR(string targetText, string deviceId)
        //{
        //    List<Rect> textCoordinates = ADBService.FindMultiTextCoordinatesOCR(targetText, deviceId, null).GetAwaiter().GetResult();
        //    if (textCoordinates != null && textCoordinates.Count > 0)
        //    {
        //        Console.WriteLine("Coordinates of detected text:");
        //        foreach (var rect in textCoordinates)
        //        {
        //            Console.WriteLine($"Left: {rect.Left}, Top: {rect.Top}, Width: {rect.Width}, Height: {rect.Height}");
        //            int x = ((int)(rect.Left + rect.Width / 2));
        //            int y = ((int)(rect.Top + rect.Height / 2));
        //            ADBService.inputTapEvent(x, y, deviceId);
        //            Thread.Sleep(4000);
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Text not found or no text detected.");
        //    }
        //}
        //public static string GetTextScreenByOCR(string deviceId, string language = "en")
        //{
        //    var _engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language(language));
        //    var screenText = ADBService.OcrScreen(deviceId, _engine).GetAwaiter().GetResult();

        //    return screenText;
        //}
        //public static bool GetScreenContainsByOCR(string targetText, string deviceId, string language = "en")
        //{
        //    var _engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language(language));
        //    var screenText = ADBService.OcrScreen(deviceId, _engine).GetAwaiter().GetResult();
        //    if (screenText.Contains(targetText))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //public static string GetTextScreenContainsByOCR(string targetText, string deviceId, int retryTimes = 3, CancellationToken? token = null)
        //{
        //    var foundText = "";
        //    var isContain = false;
        //    var _engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
        //    /*foreach (string text in targetText)
        //        {
        //            if (screenText.Contains(text.ToLower()))
        //            {
        //                isContain = true;
        //                foundText = text;
        //            }
        //        }*/
        //    while (!isContain && retryTimes-- > 0)
        //    {
        //        var screenText = ADBService.OcrScreen(deviceId, _engine).GetAwaiter().GetResult();
        //        Console.WriteLine(screenText);
        //        if (screenText.Contains(targetText.ToLower()))
        //        {
        //            return targetText;
        //        }
        //    }
        //    if (retryTimes <= 0)
        //    {
        //        return null;
        //    }
        //    return null;
        //}
        //public static string GetTextScreenContainsByOCR(string[] targetText, string deviceId, int retryTimes = 3, CancellationToken? token = null)
        //{
        //    var foundText = "";
        //    var isContain = false;
        //    var _engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
        //    /*foreach (string text in targetText)
        //        {
        //            if (screenText.Contains(text.ToLower()))
        //            {
        //                isContain = true;
        //                foundText = text;
        //            }
        //        }*/
        //    while (!isContain && retryTimes-- > 0)
        //    {
        //        var screenText = ADBService.OcrScreen(deviceId, _engine).GetAwaiter().GetResult();
        //        Console.WriteLine(screenText);
        //        isContain = Array.Exists(targetText, c => screenText.Contains(c.ToLower()));
        //        if (isContain)
        //            foundText = targetText[Array.FindIndex(targetText, c => screenText.Contains(c.ToLower()))];
        //    }
        //    if (retryTimes <= 0)
        //    {
        //        return null;
        //    }
        //    return foundText;
        //}
        public static string getPackageInstall(string package, string deviceId)
        {
            string result = runCMD(string.Format("shell pm list packages -3 | findstr {0}", package), deviceId);
            return result;
        }
        public static void InstallMultipleGameApkToDevice(string[] apkPaths, string deviceId)
        {
            apkPaths = apkPaths.Select(p => $"\"{p}\"").ToArray();
            var singleLinePath = string.Join(" ", apkPaths);
            runCMD(String.Format("install-multiple {0}", singleLinePath), deviceId);
        }
        public static string FirstInstallTime(string package, string deviceId)
        {
            string result = runCMD(string.Format("shell dumpsys package {0} | findstr firstInstallTime", package), deviceId);
            /*string firstInstallTimeString = result.Substring(result.IndexOf('=') + 1);
            DateTime firstInstallDateTime = DateTime.ParseExact(firstInstallTimeString, "yyyy-MM-dd HH:mm:ss", null);*/
            /*DateTime firstInstallTimeParse = DateTime.ParseExact(result.Substring(result.IndexOf("=") + 1), format, CultureInfo.InvariantCulture);*/
            return result;
        }
        public static string LastUpdateTime(string package, string deviceId)
        {
            string result = runCMD(string.Format("shell dumpsys package {0} | findstr lastUpdateTime", package), deviceId);
            /*adb push com.igg.android.lordsmobile.apk / data / local / tmp /*/
            /*string format = "yyyy-MM-dd HH:mm:ss";
            DateTime lastInstallTimeParse = DateTime.ParseExact(result.Substring(result.IndexOf("=") + 1), format, CultureInfo.InvariantCulture);*/
            return result;
        }

        public static void TurnDeviceAutoRotationTo(string deviceId, bool isOff = true)
        {
            runCMD($"shell settings put system accelerometer_rotation {(isOff ? "0" : "1")}", deviceId);
            return;
        }

        public static string CheckDeviceActive(string deviceId)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "Resources/adb",
                        Arguments = $"-s {deviceId} shell getprop init.svc.bootanim",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return output == "stopped" ? "YES" : "NO";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking device active status for {deviceId}: {ex.Message}");
                return "NO";
            }
        }
        public static void ScreenshotAdb(string id)
        {
            runCMDRoot(id, "shell screencap /sdcard/screenshot.png");
        }
        public static void RunAdbCommand(string arguments)
        {
            var psi = new ProcessStartInfo("Resources/adb.exe", arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process.WaitForExit();
        }

    }
}
