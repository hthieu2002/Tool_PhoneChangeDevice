using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public static partial class ADBService
    {
        public static bool checkUIDumpSuccess(string deviceIP)
        {
            var res = runCMD("shell uiautomator dump -x", deviceIP);
            return res.Contains("dumped to");
        }
        public static string curl(string sites, string deviceId, string method = "", string header = "")
        {
            if (!string.IsNullOrEmpty(header))
                header = string.Format("-H '{0}'", header);
            if (!string.IsNullOrEmpty(method))
                method = string.Format("-X {0}", method);
            return runCMD(string.Format("shell \"curl -s -S {0} '{1}' {2}\"", method, sites, header), deviceId);
        }
        public static void inputTrackBallPress(string deviceId)
        {
            runCMD("shell input trackball press", deviceId);
        }
        public static void inputRollEvent(int dx, int dy, string deviceIP)
        {
            runCMD(string.Format("shell input roll {0} {1}", dx, dy), deviceIP);
        }
        public static void inputPressEvent(string deviceIP)
        {
            runCMD("shell input press", deviceIP);
        }
        public static string shellGetAttrXmlValue(string androidPath, string attr, string value, string deviceId, string customValue = "")
        {
            customValue = string.IsNullOrEmpty(customValue) ? "value" : customValue;
            var command = string.Format("shell \"sed -rn 's|.*{0}=\"\"{1}\"\".*{2}=\"\"([^\"\"]*)\"\".*|\\1|p' {3} | head -1\"", attr, value, customValue, androidPath);
            return runCMD(command, deviceId).Trim();
        }
        public static string shellGetTagXmlValue(string androidPath, string contentIncluded, string deviceId)
        {
            var command = string.Format("shell \"awk -F\"\"<|>\"\" '/\"{0}\"/{{print $3}}' {1}\"", contentIncluded, androidPath);
            return runCMD(command, deviceId).Trim();
        }

        public static void changeHttpProxy(string IP, string deviceId)
        {
            runCMD(string.Format("shell settings put global http_proxy {0}", IP), deviceId);

        }
        public static void changeHttpProxyWithAccount(string IP, string user, string deviceId)
        {
            /*runCMD(string.Format("shell settings put global http_proxy {0}", IP), deviceId);*/
            runCMD(string.Format("shell settings put global http_proxy {0} {1}", IP, user), deviceId);

        }
        public static void updateInitRc(string imei1, string imei2, string serialNumber, string bootloader, string baseband, string model, string deviceId, string hardware = "", string platform = "")
        {
            var cert = X509CertService.GenerateCertificate(generateCertSubject());
            var rawSignatureData = cert.GetRawCertDataString();
            var leftPadding = "".PadLeft(4); // an unit of "TAB" character in .rc/shell format
            var props = $"{leftPadding}setprop ro.boot.bootloader \"{bootloader}\"\r\n" +
                        $"{leftPadding}setprop ro.bootloader \"{bootloader}\"\r\n" +
                        //$"{leftPadding}setprop ro.serialno \"{serialNumber}\"\r\n" +
                        //$"{leftPadding}setprop ro.boot.serialno \"{serialNumber}\"\r\n" +
                        //$"{leftPadding}setprop ril.serialnumber \"{serialNumber}\"\r\n" +
                        //$"{leftPadding}setprop sys.serialnumber \"{serialNumber}\"\r\n" +
                        $"{leftPadding}setprop ro.baseband \"{baseband}\"\r\n" +
                        $"{leftPadding}setprop gsm.version.baseband \"{baseband}\"\r\n";
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var originalInitRcPath = Path.Combine(exePath, $"Resources/init.lineage20.rc");
            var systemPathInitRc = "/etc/init/hw/init.rc";
            props += $"{leftPadding}setprop ril.product_code \"{bootloader}\"\r\n" +
                    $"{leftPadding}setprop ro.boot.em.model \"{model}\"\r\n" +
                    $"{leftPadding}setprop ril.sw_ver \"{bootloader}\"\r\n" +
                    //$"{leftPadding}setprop ro.boot.hardware \"{hardware}\"\r\n" +
                    $"{leftPadding}setprop ro.board.platform \"{platform}\"\r\n" +
                    $"{leftPadding}setprop ro.boot.hardware.platform \"{platform}\"\r\n" +
                    $"{leftPadding}setprop ro.hardware \"{hardware}\"\r\n";
            props += $"{leftPadding}setprop ro.boot.baseband \"{baseband}\"\r\n";
            try
            {
                var clonedInitRcPath = Path.Combine(exePath, $"Resources/{deviceId}_init.rc");
                File.Copy(originalInitRcPath, clonedInitRcPath, true);
                LocalFileService.replaceAllTextInFile(clonedInitRcPath, "FLAGS_REPLACE_SYSTEM_PROPERTIES", props);
                LocalFileService.replaceAllTextInFile(clonedInitRcPath, "FLAGS_REPLACE_WEBVIEW", $"{leftPadding}setprop ro.lineage.wvversion \"{RandomService.generateWebviewVersion()}\"");
                if (RandomService.randomInRange(0, 100) % 3 == 0)
                {
                    LocalFileService.replaceAllTextInFile(clonedInitRcPath, "#FLAGS_REPLACE_SIGNATURE", $"{leftPadding}setprop ro.android.sign \"{rawSignatureData}\"");
                }
                runCMDRoot($"push \"{clonedInitRcPath}\" {systemPathInitRc}", deviceId);
                File.Delete(clonedInitRcPath);
            }
            catch
            {
                Console.WriteLine("Init RC Error");
                runCMDRoot($"push \"{originalInitRcPath}\" {systemPathInitRc}", deviceId);
                throw new Exception("CREATE FILE INIT.RC ERROR");
            }
        }
        private static string generateMacAddressToPersistentString(string macAddress)
        {
            StringBuilder wifiMacAddress = new StringBuilder();
            int lastByteOfMacAddress = Convert.ToInt32(macAddress.Substring(macAddress.LastIndexOf(':') + 1), 16);
            for (int index = 0; index < 4; index++)
            {
                string lastByteInHex = (lastByteOfMacAddress++).ToString("x2");
                string[] temp = macAddress.Split(':');
                for (int i = 0; i < temp.Length - 1; i++)
                {
                    wifiMacAddress.Append("\\x" + temp[i]);
                }
                wifiMacAddress.Append("\\x" + lastByteInHex);
            }
            return wifiMacAddress.ToString();
        }
        public static void fakeWifiMacAddress(string macAddress, string deviceId)
        {
            var strWifiMacAddress = generateMacAddressToPersistentString(macAddress);
            runCMDRoot(string.Format("shell \"printf '%b' '{0}' > /persist/wlan_mac.bin\"", strWifiMacAddress), deviceId);
            runCMDRoot("shell chmod 644 /persist/wlan_mac.bin", deviceId);
        }

        public static bool isKeyboardShown(string deviceId)
        {
            var result = runCMD($"shell \"dumpsys input_method | grep mInputShown\"", deviceId);
            return result.Contains("true");
        }

        public static bool isWifiEnable(string deviceId)
        {
            return runCMDRoot("shell \"dumpsys wifi | sed -n '1p'\"", deviceId).Contains("Wi-Fi is enabled");
        }

        public static void enableWifi(bool isEnabled, string deviceId)
        {
            var enableStr = isEnabled ? "enable" : "disable";
            runCMDRoot(string.Format("shell svc wifi {0}", enableStr), deviceId);
        }
        public static void enableChPlay(bool isEnabled, string deviceId)
        {
            var enableStr = isEnabled ? "enable" : "disable-user";
            runCMD(string.Format("shell pm {0} com.android.vending", enableStr), deviceId);
        }
        public static void changePlatformBoard(string newPlatformBoard, string deviceId, string originalPlatformBoard = "msm8953")
        {

            var libHwPath = "/system/vendor/lib/hw";
            var lib64HwPath = "/system/vendor/lib64/hw";

            if (!isEmptyFolder(lib64HwPath, deviceId))  // Priority lib64/hw folder
            {
                // Delete old clone of previous version lib/hw folder
                deleteHardwareLibCloned(libHwPath, originalPlatformBoard, deviceId);
                libHwPath = lib64HwPath;
            }

            // Delete old cloned
            deleteHardwareLibCloned(libHwPath, originalPlatformBoard, deviceId);
            if (newPlatformBoard.Equals(originalPlatformBoard)) return;

            // Clone file from origin
            runCMD($"shell \"for FILE in $(find {libHwPath} | grep {originalPlatformBoard}); do cp -f ${{FILE}} ${{FILE/{originalPlatformBoard}/{newPlatformBoard}}}; done\"", deviceId);
            //runCMD($"shell \"lsof | grep deleted | awk '{{print $2}}' | xargs kill -9\"", deviceId);
        }

        private static void deleteHardwareLibCloned(string systemPath, string originalPlatformBoard, string deviceId)
        {
            var filePattern = new string[]
            {
                "activity_recognition.*.so",
                "audio.primary.*.so",
                "bootctrl.*.so",
                "camera.*.so",
                "gatekeeper.*.so",
                "gralloc.*.so",
                "hwcomposer.*.so",
                "keystore.*.so",
                "lights.*.so",
                "memtrack.*.so",
                "sound_trigger.primary.*.so",
                "thermal.*.so",
                "vr.*.so",
                "vulkan.*.so"
            };
            foreach (string path in filePattern)
            {
                var filesRemoved = runCMD($"shell \"find {systemPath} | grep '^{systemPath}/{path}' | grep -Ev '{originalPlatformBoard}|default'\"", deviceId)
                            .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string _f in filesRemoved)
                {
                    removeFile(_f, deviceId);
                }
            }
        }

        public static bool isEmptyFolder(string systemPath, string deviceId)
        {
            var result = runCMD($"shell \"[ -n '$(ls -A {systemPath})' ] && echo 'contains file' || echo 'empty folder'\"", deviceId);
            return result.Contains("empty folder");
        }

        public static void updateInitRc(string imei1, string imei2, string serialNumber, string bootloader, string baseband, string deviceId, DeviceCodeName deviceCodeName = DeviceCodeName.TISSOT)
        {
            if (deviceCodeName == DeviceCodeName.SARGO) return;
            if (deviceCodeName == DeviceCodeName.CROSSHATCH) return;
            var cert = X509CertService.GenerateCertificate(generateCertSubject());
            var rawSignatureData = cert.GetRawCertDataString();
            var pl = "".PadLeft(4); // an unit of "TAB" character in .rc/shell format
            var props = $"{pl}setprop persist.radio.imei1 \"{imei1}\"\r\n" +
                        $"{pl}setprop persist.radio.imei2 \"{imei2}\"\r\n" +
                        $"{pl}setprop ro.boot.bootloader \"{bootloader}\"\r\n" +
                        $"{pl}setprop ro.bootloader \"{bootloader}\"\r\n" +
                        $"{pl}setprop ro.serialno \"{serialNumber}\"\r\n" +
                        $"{pl}setprop ro.boot.serialno \"{serialNumber}\"\r\n" +
                        $"{pl}setprop ril.serialnumber \"{serialNumber}\"\r\n" +
                        $"{pl}setprop sys.serialnumber \"{serialNumber}\"\r\n" +
                        $"{pl}setprop ro.baseband \"{baseband}\"\r\n" +
                        $"{pl}setprop gsm.version.baseband \"{baseband}\"\r\n" +
                        $"{pl}setprop ro.boot.baseband \"{baseband}\"";

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var originalInitRcPath = Path.Combine(exePath, $"Resources/init.tissot.rc");
            var systemPathInitRc = "/init.rc"; // for tissot default
            if (deviceCodeName == DeviceCodeName.STARLTE)
            {
                originalInitRcPath = Path.Combine(exePath, $"Resources/init.starlte.rc");
                systemPathInitRc = "/etc/init/hw/init.rc";
            }
            try
            {
                var clonedInitRcPath = Path.Combine(exePath, $"Resources/{deviceId}_init.rc");
                File.Copy(originalInitRcPath, clonedInitRcPath, true);
                LocalFileService.replaceAllTextInFile(clonedInitRcPath, "FLAGS_REPLACE_SYSTEM_PROPERTIES", props);
                //LocalFileService.replaceAllTextInFile(clonedInitRcPath, "FLAGS_REPLACE_WEBVIEW", $"{pl}setprop ro.lineage.wvversion \"{RandomService.generateWebviewVersion()}\"");
                LocalFileService.replaceAllTextInFile(clonedInitRcPath, "FLAGS_REPLACE_SIGNATURE", $"{pl}setprop ro.lineage.sign \"{rawSignatureData}\"");
                runCMD($"push \"{clonedInitRcPath}\" {systemPathInitRc}", deviceId);
                File.Delete(clonedInitRcPath);
            }
            catch
            {
                Console.WriteLine("Init RC Error");
                runCMD($"push \"{originalInitRcPath}\" {systemPathInitRc}", deviceId);
                throw new Exception("CREATE FILE INIT.RC ERROR");
            }
        }
        public static string generateCertSubject()
        {
            var listCitiesNewYork = new string[] {"Los Angeles",
            "New York",
            "Buffalo",
            "Rochester",
            "Yonkers",
            "Syracuse",
            "Albany",
            "New Rochelle",
            "Mount Vernon",
            "Schenectady",
            "Utica",
            "White Plains",
            "Hempstead",
            "Troy",
            "Niagara Falls",
            "Binghamton",
            "Freeport",
            "Valley Stream" };
            var randomCity1 = RandomService.randomInRange(0, listCitiesNewYork.Length);
            var randomCity2 = RandomService.randomInRange(0, listCitiesNewYork.Length);
            var randomEmail = RandomService.generateUser() + RandomService.randomCharacters(10) + RandomService.randomInRange(10000, 100000);
            var domainMails = new string[] { "@yahoo.com", "@hotmail.com", "@outlook.com", "@outlook.ru", "@outlook.com.vn" };
            var domainMail = domainMails[RandomService.randomFromArrayLength(domainMails.Length)];
            return $"CN=Android, OU=Android, O={listCitiesNewYork[randomCity1]} Inc., L={listCitiesNewYork[randomCity2]}, ST=New York, C=US, emailAddress={randomEmail}{domainMail}";
        }

        public static void revertInitRc(string deviceId)
        {
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var originalInitRcPath = Path.Combine(exePath, "Resources/init.rc");
            runCMD($"push \"{originalInitRcPath}\" /init.rc", deviceId);
        }

        public static void importContacts(string deviceId)
        {
            var contentBasicCard =   "BEGIN:VCARD\r\n" +
                                "VERSION:2.1\r\n" +
                                "N:;{0};;;\r\n" +
                                "FN:{0}\r\n" +
                                "TEL;CELL:{1}\r\n" +
                                "END:VCARD\r\n";
            var fullContacts = "";
            var numberOfContacts = RandomService.randomInRange(2, 5);
            for (int i = 0; i < numberOfContacts; i++)
            {
                var name = RandomService.generateName();
                var number = string.Concat("+84", RandomService.generatePhoneNumber());
                fullContacts += string.Format(contentBasicCard, name, number);
            }

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var originalContactPath = Path.Combine(exePath, $"Resources/contacts_{deviceId}.vcf");

            LocalFileService.writeTextFile(originalContactPath, fullContacts);

            pullOrPushFile(FileTransferAction.PUSH, originalContactPath, "/sdcard/contacts.vcf", deviceId);
            File.Delete(originalContactPath);
            runCMD("shell \"am start -t 'text/vcard' -d 'file:///storage/emulated/0/contacts.vcf' -a android.intent.action.VIEW com.android.contacts\"", deviceId);
            Thread.Sleep(300);
            inputKeyEvent(KeyEventCode.ENTER.ToString(), deviceId);
            Thread.Sleep(100);
            inputKeyEvent(KeyEventCode.TAB.ToString(), deviceId);
            Thread.Sleep(100);
            inputKeyEvent(KeyEventCode.ENTER.ToString(), deviceId);

        }

    }
}
