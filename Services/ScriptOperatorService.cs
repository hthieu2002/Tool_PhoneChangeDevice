using POCO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    [Obfuscation(Exclude = true)]
    public enum ControlCoordinator
    {
        GooglePlaySigninButton,
        CreateAccountButton,
        ForMySelfButton,
        ForMySelfButtonNew,
        NextButton,
        FirstNameTextBox,
        LastNameTextBox,
        MonthDropdownlist,
        MonthListMenu,
        DayTextBox,
        YearTextBox,
        GenderDropdownlist,
        GenderListMenu,
        TheFirstEmailOpion,
        TheThirdEmailOption,
        DesiredEmailOption,
        CustomEmailTextBox,
        RepasswordTextBox,
        ShowPasswordCheckBox,
        HeaderLogoGoogleSquare,
        SkipButton,
        AgreeButton,
        //Recovery Steps
        SelectTheFirstEmailFromAccounts,
        GoogleAccountButton, //x = 230-780, y = 560-700
        GetStartedButton,
        GoogleAccountMenuButton,
        SecurityMenuButton,
        AddRecoveryEmailButton,
        PasswordRecoveryTextBox,
        AddRecoveryEmailTextBox,
        EmailOrPhone,
        Password
    }
    // either 
    // press enter
    // press keypad enter in x,y 
    // tap on header and press next button
    // swipe Up or Down (slowly) on header and press next button
    public enum TransitionScreenAction
    {
        PressEnter,
        PressEnterKeypad,
        TabOnHeaderAndNextButton,
        SwipeOnHeaderAndNextButton,
        SwipeOnHeaderAndNextButtonOnFormPassword
    }
    public class Point
    {
        private int x = 0;
        private int y = 0;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(Point point)
        {
            this.x = point.X;
            this.Y = point.Y;
        }

        public int Y { get => y; set => y = value; }
        public int X { get => x; set => x = value; }
    }
    public class ScriptOperatorService
    {

        private string deviceIP = string.Empty;
        private Action<string> callBack;
        private CancellationToken token;
        public static string normalizeCode(string[] linesOfCode)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var line in linesOfCode.Where(c => !string.IsNullOrEmpty(c)))
            {
                var indexComment = line.Trim().IndexOf('#');
                // check if comment character # is not at index = 0 --> do append this line of code
                if (indexComment != 0)
                {
                    var tempStr = indexComment <= 0 ? line : line.Remove(indexComment);
                    builder.AppendFormat("{0};", tempStr.Trim());
                }
            }
            return builder.ToString();
        }
        public ScriptOperatorService(string deviceIP, Action<string> callBack, CancellationToken token)
        {
            this.deviceIP = deviceIP;
            this.callBack = callBack;
            this.token = token;
        }
        public static void touch(int x, int y, string deviceIP, int delay = 0)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            //Console.WriteLine("Touch at x = {0}, y = {1}, log = {2}", x, y, log);
            ADBService.inputTapEvent(x, y, deviceIP);
            Thread.Sleep(delay);
            //if (!string.IsNullOrEmpty(log)) callBack(log);

        }
        public void wait(int s, string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            ADBService.shellSleep(s, deviceIP);
            if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public static void key(string keyCode, string deviceIP)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            ADBService.inputKeyEvent(keyCode, deviceIP);
            //if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public static void send(string text, string deviceIP, bool sendTextInBlock = true)// string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            //ADBService.inputTextEvent(text, deviceIP);
            //if (sendTextInBlock)
            //{
            //    ADBService.inputTextEvent(text, deviceIP);
            //    return;
            //}
            //foreach (char character in text)
            //{
            //    ADBService.inputCharEvent(character, deviceIP);
            //    Thread.Sleep(RandomService.randomInRange(88, 888));
            //}
            if (sendTextInBlock)
            {
                if (text.Contains(' '))
                {
                    var strResult = text.Split(' ');
                    for (int i = 0; i < strResult.Length; i++)
                    {
                        ADBService.inputTextEvent(strResult[i], deviceIP);
                        Thread.Sleep(RandomService.randomInRange(200, 500));
                        if (i != strResult.Length - 1)
                        {
                            ADBService.inputTextEvent(" ", deviceIP);
                        }
                    }
                    return;
                }
                var blockTextLength = text.Length / 3;
                for (int i = 0; i < 3; i++)
                {
                    string blockOfText = string.Empty;
                    if (i != 2)
                        blockOfText = text.Substring(i * blockTextLength, blockTextLength);
                    else
                        blockOfText = text.Substring(i * blockTextLength);
                    ADBService.inputTextEvent(blockOfText, deviceIP);
                    Thread.Sleep(RandomService.randomInRange(300, 600));
                }
                return;
            }
            foreach (char character in text)
            {
                ADBService.inputCharEvent(character, deviceIP);
                Thread.Sleep(RandomService.randomInRange(11, 111));
            }
            //if (text.Contains(' '))
            //{
            //    var strResult = text.Split(' ');
            //    for (int i = 0; i < strResult.Length; i++)
            //    {
            //        ADBService.inputTextEvent(strResult[i], deviceIP);
            //        Thread.Sleep(RandomService.randomInRange(200, 500));
            //        if (i != strResult.Length - 1)
            //        {
            //            ADBService.inputTextEvent(" ", deviceIP);
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (char character in text)
            //    {
            //        ADBService.inputCharEvent(character, deviceIP);
            //        Thread.Sleep(RandomService.randomInRange(88, 333));
            //    }
            //}


            //if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public static void swipeRandom(int numOfSwipe, int minSpeed, int maxSpeed, string deviceIP)//, string log = "")
        {
            for (int i = 0; i < numOfSwipe; i++)
            {
                int x1 = RandomService.randomInRange(500, 700);
                int y1 = RandomService.randomInRange(1000, 1400);
                int x2 = RandomService.randomInRange(850, 950);
                int y2 = RandomService.randomInRange(500, 750);
                int speed = RandomService.randomInRange(minSpeed, maxSpeed);
                ADBService.inputSwipeEvent(x1, y1, x2, y2, speed, deviceIP);
            }
        }
        public static void swipe(int x1, int y1, int x2, int y2, int duration, string deviceIP)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            ADBService.inputSwipeEvent(x1, y1, x2, y2, duration, deviceIP);
            //if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public void openPackage(string package, string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            ADBService.openPackage(package, deviceIP);
            if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public static void touchByText(string text, string deviceIP)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var xy = findViewLocation(FindDumpNodeByType.TEXT, text, deviceIP);
            if (xy != null)
            {
                var x = xy["x"];
                var y = xy["y"];
                ADBService.inputTapEvent(x, y, deviceIP);
                Console.WriteLine($"Coor x: {x} and y: {y}");
            }
            Console.WriteLine($"Coor xy: {xy}");
            //if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public static bool touchByTextContains(string text, string deviceIP)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var xy = findViewLocationContains(FindDumpNodeByType.TEXT, text, deviceIP);
            if (xy != null)
            {
                var x = xy["x"];
                var y = xy["y"];
                ADBService.inputTapEvent(x, y, deviceIP);
                return true;
            }
            Console.WriteLine($"Coor xy: {xy}");
            return false;
            //if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public Dictionary<string, string> randomName(string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var randomName = RandomService.randomNameFromResource();
            callBack(string.Format("Username: {0} {1}", randomName[BasicGmailInfo.FIRSTNAME_FLAG], randomName[BasicGmailInfo.LASTNAME_FLAG]));
            if (!string.IsNullOrEmpty(log)) callBack(log);
            return randomName;
        }
        public Dictionary<string, string> randomBasicInfo(string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var randomBasicInfo = RandomService.randomBirthdayAndGender();
            if (!string.IsNullOrEmpty(log)) callBack(log);
            return randomBasicInfo;
        }
        public string randomChars(int length = 1, string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var str = RandomService.randomCharacters(length);
            if (!string.IsNullOrEmpty(log)) callBack(log);
            return str;
        }
        public string randomPassword(string firstname, string lastname, string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var password = RandomService.randomPasswordByName(firstname, lastname);
            callBack("Password: " + password);
            if (!string.IsNullOrEmpty(log)) callBack(log);
            return password;
        }
        public int getAPIID(string log = "")
        {
            if (!string.IsNullOrEmpty(log)) callBack(log);
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var apiId = 0;
            var taskSuccess = Task.Run(() =>
            {
                apiId = APIService.getRentCodeID(deviceIP);
            }).Wait(20000);
            if (!taskSuccess) throw new Exception("Timeout");
            callBack("API ID: " + apiId);
            return apiId;
        }
        public string getPhoneNumber(int apiId, string log = "")
        {
            if (!string.IsNullOrEmpty(log)) callBack(log);
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var phoneNumber = string.Empty;
            var cancelCallAPI = false;
            var taskGetPhoneNum = Task.Run(() =>
            {
                while (!token.IsCancellationRequested && !cancelCallAPI && string.IsNullOrEmpty(phoneNumber))
                {
                    Thread.Sleep(3000);
                    phoneNumber = APIService.getRentCodePhoneNumber(apiId, deviceIP);
                }
            }).Wait(240000);
            if (!taskGetPhoneNum)
            {
                cancelCallAPI = true;
                throw new Exception("Timeout");
            }
            callBack("Phone number: " + phoneNumber);
            return phoneNumber;
        }
        public string getGoogleCode(int apiId, string log = "")
        {
            if (!string.IsNullOrEmpty(log)) callBack(log);
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var gCode = string.Empty;
            var cancelCallAPI = false;
            var taskGetCode = Task.Run(() =>
            {
                while (!token.IsCancellationRequested && !cancelCallAPI && (string.IsNullOrEmpty(gCode) || gCode.Contains("NotFound")))
                {
                    Thread.Sleep(3000);
                    gCode = APIService.getRentCodeMessage(apiId, deviceIP);
                }
            }).Wait(120000);
            if (!taskGetCode)
            {
                cancelCallAPI = true;
                throw new Exception("Timeout");
            }
            callBack("G-Code: " + gCode);
            return gCode.Substring(2, 6);
        }
        public static bool waitByText(string text, string deviceIP, int retryTimes = 5, string messageException = "", OutputInfo info = null, Action actionExecutingWhileWaiting = null)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var isContain = false;
            retryTimes++;
            while (!isContain && retryTimes-- > 0)
            {
                if (!ADBService.checkUIDumpSuccess(deviceIP))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                var dumpXml = ADBService.readFromFile(AndroidPath.DEFAULT_DUMP_XML, deviceIP);
                isContain = dumpXml.ToLower().Contains(text.ToLower());
                if (actionExecutingWhileWaiting != null && isContain)
                {
                    actionExecutingWhileWaiting.Invoke();
                    Thread.Sleep(555);
                }
            }
            //if (!string.IsNullOrEmpty(log)) callBack(log);
            if (!string.IsNullOrEmpty(messageException) && retryTimes <= 0)
            {
                Console.WriteLine("Exception wait by text : {0}, retryTime = {1}", messageException, retryTimes);
                if (info != null)
                {
                    string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    Task.Run(() => LocalFileService.writeTextFile(string.Format(@"{0}\log\{1}.{2}.log.csv", exePath, DateTime.Now.ToString("ddMM"), System.Environment.MachineName), string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}"
                                     , DateTime.Now.ToString("HH:mm:ss")
                                     , info.FirstName
                                     , info.LastName
                                     , info.ProxyHostFull
                                     , info.ProxyPublicIP
                                     , info.CountryCode
                                     , info.DeviceFingerPrint
                                     , info.DeviceMacAddress
                                     , info.DeviceSourceInfo
                                     , info.IndexOfRetrying
                                     , info.VerifyStatus
                                     , info.ExceptionMessage
                                     , info.DeviceId
                                     , info.WebViewVersion
                                     , info.GmsVersion
                                     , info.Rom
                                     ), FileMode.Append, true));
                }
                throw new Exception(messageException);
            }
            Console.WriteLine("Wait by text : {0}, retryTime = {1}", text, retryTimes);
            return isContain;
        }

        public static string findByText(string[] text, string deviceIP, int retryTimes = 5, string messageException = "", CancellationToken? token = null)//, string log = "")
        {
            var isContain = false;
            retryTimes++;
            var result = "";
            while (!isContain && retryTimes-- > 0)
            {
                /*if (token != null && token.HasValue && token.Value.IsCancellationRequested) break;*/
                if (!ADBService.checkUIDumpSuccess(deviceIP))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                var dumpXml = ADBService.readFromFile("/sdcard/window_dump.xml", deviceIP);
                isContain = Array.Exists(text, c => dumpXml.Contains(c));
                if (isContain)
                    result = text[Array.FindIndex(text, c => dumpXml.Contains(c))];
            }

            if (!string.IsNullOrEmpty(messageException) && retryTimes <= 0)
            {
                Console.WriteLine("Exception wait by text : {0}, retryTime = {1}", messageException, retryTimes);
                throw new Exception(messageException);
            }
            Console.WriteLine("Wait by text : {0}, retryTime = {1}, result found = {2}, text Found = {3}", text, retryTimes, isContain, result);
            return result;
        }
        public static bool waitByTextEx(string text, string deviceId, int retryTimes = 5, OutputInfo info = null, string deviceName = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var isContain = false;
            retryTimes++;
            while (!isContain && retryTimes-- > 0)
            {
                if (!ADBService.checkUIDumpSuccess(deviceId))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                var dumpXml = ADBService.readFromFile(AndroidPath.DEFAULT_DUMP_XML, deviceId);
                isContain = dumpXml.ToLower().Contains(text.ToLower());
            }
            //if (!string.IsNullOrEmpty(log)) callBack(log);
            if (info != null && !string.IsNullOrEmpty(info.ExceptionMessage) && retryTimes <= 0)
            {
                Console.WriteLine("Exception wait by text : {0}, retryTime = {1}", info.ExceptionMessage, retryTimes);
                if (info != null)
                {
                    string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    Task.Run(() => LocalFileService.writeTextFile(string.Format(@"{0}\log\{1}.{2}.log.csv", exePath, DateTime.Now.ToString("ddMM"), System.Environment.MachineName), string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}"
                                    , DateTime.Now.ToString("HH:mm:ss")
                                     , info.FirstName
                                     , info.LastName
                                     , info.ProxyHostFull
                                     , info.ProxyPublicIP
                                     , info.CountryCode
                                     , info.DeviceFingerPrint
                                     , info.DeviceMacAddress
                                     , info.DeviceSourceInfo
                                     , info.IndexOfRetrying
                                     , info.VerifyStatus
                                     , info.ExceptionMessage
                                     , info.DeviceId
                                     , info.WebViewVersion
                                     , info.GmsVersion
                                     , info.Rom
                                     , deviceName), FileMode.Append, true));
                }
                return false;
            }
            Console.WriteLine("Wait by text : {0}, retryTime = {1}", text, retryTimes);
            return isContain;
        }
        public static string waitByMultipleText(List<string> text_list, string deviceIP, int retryTimes = 5, string messageException = "", OutputInfo info = null)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            var findString = string.Empty;
            var isContain = false;
            retryTimes++;
            while (!isContain && retryTimes-- > 0)
            {
                if (!ADBService.checkUIDumpSuccess(deviceIP))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                var dumpXml = ADBService.readFromFile(AndroidPath.DEFAULT_DUMP_XML, deviceIP);
                foreach (var text in text_list)
                {
                    isContain = dumpXml.ToLower().Contains(text.ToLower());
                    if (isContain)
                    {
                        findString = text;
                        break;
                    }
                }
            }
            //if (!string.IsNullOrEmpty(log)) callBack(log);
            if (!string.IsNullOrEmpty(messageException) && retryTimes <= 0)
            {
                Console.WriteLine("Exception wait by text : {0}, retryTime = {1}", messageException, retryTimes);
                if (info != null)
                {
                    string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    Task.Run(() => LocalFileService.writeTextFile(string.Format(@"{0}\log\{1}.{2}.log.csv", exePath, DateTime.Now.ToString("ddMM"), System.Environment.MachineName), string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}"
                                     , DateTime.Now.ToString("HH:mm:ss")
                                     , info.FirstName
                                     , info.LastName
                                     , info.ProxyHostFull
                                     , info.ProxyPublicIP
                                     , info.CountryCode
                                     , info.DeviceFingerPrint
                                     , info.DeviceMacAddress
                                     , info.DeviceSourceInfo
                                     , info.IndexOfRetrying
                                     , info.VerifyStatus
                                     , info.ExceptionMessage
                                     , info.DeviceId
                                     , info.WebViewVersion
                                     , info.GmsVersion
                                     , info.Rom
                                     ), FileMode.Append, true));
                }
                throw new Exception(messageException);
            }
            Console.WriteLine("Wait by text : {0}, retryTime = {1}", findString, retryTimes);
            return findString;
        }
        public void saveText(string path, string text, string log = "")
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            LocalFileService.writeTextFile(path, text, System.IO.FileMode.Append, true);
            callBack("Content saved to " + path);
            if (!string.IsNullOrEmpty(log)) callBack(log);
        }
        public void log(object obj)
        {
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            callBack(obj.ToString());
        }
        public void cleanDevice(string log = "")
        {
            if (!string.IsNullOrEmpty(log)) callBack(log);
            ADBService.cleanGMSPackagesAndAccounts(deviceIP);
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
        }
        public static void roll(int begin, int end, string deviceIP)//, string log = "")
        {
            //if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            ADBService.inputRollEvent(begin, end, deviceIP);
            //if (!string.IsNullOrEmpty(log)) callBack(log);
        }

        public void closePackage(string package, string log = "")
        {
            if (!string.IsNullOrEmpty(log)) callBack(log);
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            ADBService.forceStopPackage(package, deviceIP);
        }

        public void restart(string log = "")
        {
            if (!string.IsNullOrEmpty(log)) callBack(log);
            if (token.IsCancellationRequested) { Console.WriteLine("Action {0} cancelled.", MethodBase.GetCurrentMethod().Name); token.ThrowIfCancellationRequested(); };
            throw new Exception("Restart");
        }

        private static Dictionary<string, int> findViewLocation(FindDumpNodeByType type, string attrValue, string deviceId)
        {
            int retryTimes = 2;
            Dictionary<string, int> elementBounds = null;
            while (elementBounds == null)
            {
                if (retryTimes-- == 0) return null;
                if (!ADBService.checkUIDumpSuccess(deviceId))
                {
                    Thread.Sleep(RandomService.randomInRange(1000, 2000));
                    elementBounds = null;
                    continue;
                }
                var dumpXml = ADBService.readFromFile(AndroidPath.DEFAULT_DUMP_XML, deviceId);
                if (type.Equals(FindDumpNodeByType.TEXT))
                {
                    elementBounds = XmlService.getCoordinateDumpNodeByAttr(dumpXml, "text", attrValue);
                    if (elementBounds == null)
                    {
                        elementBounds = XmlService.getCoordinateDumpNodeByAttr(dumpXml, "content-desc", attrValue);
                    }
                }
                else
                {
                    elementBounds = XmlService.getCoordinateDumpNodeByAttr(dumpXml, "resource-id", attrValue);
                }
            }

            return elementBounds;
        }
        private static Dictionary<string, int> findViewLocationContains(FindDumpNodeByType type, string attrValue, string deviceId)
        {
            int retryTimes = 2;
            Dictionary<string, int> elementBounds = null;
            while (elementBounds == null)
            {
                if (retryTimes-- == 0) return null;
                if (!ADBService.checkUIDumpSuccess(deviceId))
                {
                    Thread.Sleep(RandomService.randomInRange(1000, 2000));
                    elementBounds = null;
                    continue;
                }
                var dumpXml = ADBService.readFromFile(AndroidPath.DEFAULT_DUMP_XML, deviceId);
                if (type.Equals(FindDumpNodeByType.TEXT))
                {
                    elementBounds = XmlService.getCoordinateDumpNodeByAttrContains(dumpXml, "text", attrValue);
                    if (elementBounds == null)
                    {
                        elementBounds = XmlService.getCoordinateDumpNodeByAttrContains(dumpXml, "content-desc", attrValue);
                    }
                }
                else
                {
                    elementBounds = XmlService.getCoordinateDumpNodeByAttrContains(dumpXml, "resource-id", attrValue);
                }
            }

            return elementBounds;
        }

        #region customAction for Gmail
        public static void transitionToNextScreen(TransitionScreenAction transitionScreenAction, string deviceId)
        {
            Console.WriteLine("Transition to next screen = action {0}", transitionScreenAction.ToString());
            var swipeNum = RandomService.randomInRange(1, 10) % 4 == 0 ? 2 : 1;// 20% swipe 2 times
            switch (transitionScreenAction)
            {
                case TransitionScreenAction.PressEnter:
                    key("ENTER", deviceId);
                    break;
                case TransitionScreenAction.PressEnterKeypad:
                    ADBService.inputTapEvent(RandomService.randomInRange(965, 1030), RandomService.randomInRange(1800, 1850), deviceId);
                    break;
                case TransitionScreenAction.TabOnHeaderAndNextButton:
                    var xHeader = RandomService.randomInRange(660, 1000);
                    var yHeader = RandomService.randomInRange(200, 500);
                    // tap on header
                    ADBService.inputTapEvent(xHeader, yHeader, deviceId);
                    // tap on the next button
                    var xNextButton = RandomService.randomInRange(770, 990);
                    var yNextButton = RandomService.randomInRange(1750, 1830);
                    ADBService.inputTapEvent(xNextButton, yNextButton, deviceId);
                    break;
                case TransitionScreenAction.SwipeOnHeaderAndNextButton:
                    // swipe up
                    for (int i = 0; i < swipeNum; i++)
                    {
                        int x1 = RandomService.randomInRange(610, 800);
                        int y1 = RandomService.randomInRange(375, 500);
                        int x2 = RandomService.randomInRange(808, 990);
                        int y2 = RandomService.randomInRange(100, 200);
                        int swipeDuration = RandomService.randomInRange(222, 333);
                        swipe(x1, y1, x2, y2, swipeDuration, deviceId);
                    }
                    // tap on the next button
                    var xNewNextButton = RandomService.randomInRange(770, 990);
                    var yNewNextButton = RandomService.randomInRange(960, 1030);
                    ADBService.inputTapEvent(xNewNextButton, yNewNextButton, deviceId);
                    break;
                case TransitionScreenAction.SwipeOnHeaderAndNextButtonOnFormPassword:
                    // swipe up
                    for (int i = 0; i < swipeNum; i++)
                    {
                        int x1 = RandomService.randomInRange(610, 800);
                        int y1 = RandomService.randomInRange(375, 500);
                        int x2 = RandomService.randomInRange(808, 990);
                        int y2 = RandomService.randomInRange(100, 200);
                        int swipeDuration = RandomService.randomInRange(222, 333);
                        swipe(x1, y1, x2, y2, swipeDuration, deviceId);
                    }
                    // tap on the next button
                    var xNewNextButtonFormPassword = RandomService.randomInRange(770, 990);
                    var yNewNextButtonFormPassword = RandomService.randomInRange(1100, 1150);
                    ADBService.inputTapEvent(xNewNextButtonFormPassword, yNewNextButtonFormPassword, deviceId);
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region autoActionGmail
        public static Point touchSpecificControl(ControlCoordinator control, string deviceIP, int customX = 0, int customY = 0)//, string log = "")
        {
            int x = 0, y = 0;
            switch (control)
            {
                case ControlCoordinator.GooglePlaySigninButton:
                    x = RandomService.randomInRange(380, 700);
                    y = RandomService.randomInRange(1400, 1480);
                    break;
                case ControlCoordinator.CreateAccountButton:
                    x = RandomService.randomInRange(120, 360);
                    y = RandomService.randomInRange(1750, 1800);
                    break;
                case ControlCoordinator.ForMySelfButton:
                    x = RandomService.randomInRange(123, 333);
                    y = RandomService.randomInRange(1610, 1678);
                    break;
                case ControlCoordinator.ForMySelfButtonNew:
                    x = RandomService.randomInRange(123, 333);
                    y = RandomService.randomInRange(1438, 1530);
                    break;
                case ControlCoordinator.NextButton:
                    x = RandomService.randomInRange(770, 990);
                    y = RandomService.randomInRange(1750, 1830);
                    break;
                case ControlCoordinator.SkipButton:
                    x = RandomService.randomInRange(80, 160);
                    y = RandomService.randomInRange(1740, 1830);
                    break;
                case ControlCoordinator.FirstNameTextBox:
                    x = RandomService.randomInRange(160, 850);
                    y = RandomService.randomInRange(700, 777);
                    break;
                case ControlCoordinator.LastNameTextBox:
                    x = RandomService.randomInRange(160, 850);
                    y = RandomService.randomInRange(920, 1010);
                    break;

                case ControlCoordinator.MonthDropdownlist:
                    x = RandomService.randomInRange(120, 270);
                    y = RandomService.randomInRange(650, 750);
                    break;
                case ControlCoordinator.MonthListMenu:
                    x = RandomService.randomInRange(140, 950);
                    y = RandomService.randomInRange(150, 1850);
                    int[] excludesNum = new int[] { 288, 289, 290,
                                                    459,460,461,
                                                    630,631,632,
                                                    801,802,803,
                                                    972,973,974,
                                                    1143,1144,1145,
                                                    1314,1315,1316,
                                                    1485,1486,1487,
                                                    1485,1486,1487,
                                                    1656,1657,1658,
                                                    1827,1828,1829
                    };
                    while (Array.Find(excludesNum, e => e == y) != default(int))
                    {
                        y = RandomService.randomInRange(150, 1850);
                    }
                    break;
                case ControlCoordinator.DayTextBox:
                    x = RandomService.randomInRange(460, 650);
                    y = RandomService.randomInRange(666, 750);
                    break;
                case ControlCoordinator.YearTextBox:
                    x = RandomService.randomInRange(750, 950);
                    y = RandomService.randomInRange(650, 750);
                    break;
                case ControlCoordinator.GenderDropdownlist:
                    x = RandomService.randomInRange(300, 850);
                    y = RandomService.randomInRange(900, 980);
                    break;
                case ControlCoordinator.GenderListMenu:
                    x = RandomService.randomInRange(130, 900);
                    //y = RandomService.randomInRange(850, 950); //only male
                    y = RandomService.randomInRangeButExclude(777, 950, 822, 825);
                    //y = 850 - 950(male)

                    break;
                case ControlCoordinator.TheFirstEmailOpion:
                    x = RandomService.randomInRange(150, 555);
                    //y = RandomService.randomInRange(666, 750); //only the first option
                    y = RandomService.randomInRangeButExclude(666, 888, 767, 769); //cover both the first and second option
                    break;
                case ControlCoordinator.TheThirdEmailOption:
                    x = RandomService.randomInRange(300, 600);
                    y = RandomService.randomInRange(970, 1050);
                    break;
                case ControlCoordinator.DesiredEmailOption:
                    x = RandomService.randomInRange(300, 600);
                    y = RandomService.randomInRange(1234, 1300);
                    break;
                case ControlCoordinator.CustomEmailTextBox:
                    x = RandomService.randomInRange(130, 650);
                    y = RandomService.randomInRange(730, 830);
                    break;
                case ControlCoordinator.RepasswordTextBox:
                    x = RandomService.randomInRange(125, 250);
                    y = RandomService.randomInRange(970, 1070);
                    break;
                case ControlCoordinator.ShowPasswordCheckBox:
                    x = RandomService.randomInRange(100, 450);
                    y = RandomService.randomInRange(1150, 1180);
                    break;
                case ControlCoordinator.AgreeButton:
                    x = RandomService.randomInRange(800, 980);
                    y = RandomService.randomInRange(1775, 1840);
                    break;
                case ControlCoordinator.GoogleAccountButton:
                    x = RandomService.randomInRange(230, 780);
                    y = RandomService.randomInRange(560, 700);
                    break;
                case ControlCoordinator.SelectTheFirstEmailFromAccounts:
                    x = RandomService.randomInRange(150, 500);
                    y = RandomService.randomInRange(450, 550);
                    break;
                case ControlCoordinator.HeaderLogoGoogleSquare:
                    x = RandomService.randomInRange(660, 1000);
                    y = RandomService.randomInRange(200, 500);
                    break;
                case ControlCoordinator.GetStartedButton:
                    x = RandomService.randomInRange(365, 700);
                    y = RandomService.randomInRange(1690, 1760);
                    break;
                case ControlCoordinator.GoogleAccountMenuButton:
                    x = RandomService.randomInRange(900, 1000);
                    y = RandomService.randomInRange(1800, 1900);
                    break;
                case ControlCoordinator.SecurityMenuButton:
                    x = RandomService.randomInRange(220, 700);
                    y = RandomService.randomInRange(1510, 1580);
                    break;
                case ControlCoordinator.AddRecoveryEmailButton:
                    x = RandomService.randomInRange(100, 700);
                    y = RandomService.randomInRange(1555, 1666);
                    break;
                case ControlCoordinator.PasswordRecoveryTextBox:
                    x = RandomService.randomInRange(150, 700);
                    y = RandomService.randomInRange(900, 970);
                    break;
                case ControlCoordinator.EmailOrPhone:
                    x = RandomService.randomInRange(200, 800);
                    y = RandomService.randomInRange(640, 770);
                    break;
                case ControlCoordinator.Password:
                    x = RandomService.randomInRange(200, 800);
                    y = RandomService.randomInRange(700, 810);
                    break;
                default:
                    break;
            }
            if (customX != 0 && customY != 0)
            {
                x = customX;
                y = customY;
            }
            Console.WriteLine("Touch control {0} at ({1},{2})", control.ToString(), x, y);
            ADBService.inputTapEvent(x, y, deviceIP);
            Thread.Sleep(RandomService.randomInRange(100, 333));
            return new Point(x, y);
        }

        #endregion
        public static Point findBoundOfSpecificControl(string textOfControl, string deviceIP, int retryTimes = 5, string messageException = "")//, string log = "")
        {
            int indexOf5StarButton = -1;
            Point boundA = new Point(0, 0);
            Point boundB = new Point(0, 0);
            retryTimes++;
            while (indexOf5StarButton < 0 && retryTimes-- > 0)
            {
                if (!ADBService.checkUIDumpSuccess(deviceIP))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                var dumpXml = ADBService.readFromFile("/sdcard/window_dump.xml", deviceIP);
                indexOf5StarButton = dumpXml.IndexOf(textOfControl);
                if (indexOf5StarButton > 0) // if found
                {
                    var textBoundLeft = "[";
                    var textBoundRight = ">";
                    var indexLeft = dumpXml.IndexOf(textBoundLeft, indexOf5StarButton);
                    var indexRight = dumpXml.IndexOf(textBoundRight, indexOf5StarButton) - 1;
                    var stringContainBound = dumpXml.Substring(indexLeft, indexRight - indexLeft);
                    var tempStr = stringContainBound.Split(new[] { '[', ']', ',', ' ', '\\', '\"' }, StringSplitOptions.RemoveEmptyEntries);
                    boundA.X = int.TryParse(tempStr[0], out int ax) ? ax : 0;
                    boundA.Y = int.TryParse(tempStr[1], out int ay) ? ay : 0;
                    boundB.X = int.TryParse(tempStr[2], out int bx) ? bx : 0;
                    boundB.Y = int.TryParse(tempStr[3], out int by) ? by : 0;
                }
            }
            return new Point((boundA.X + boundB.X) / 2, (boundA.Y + boundB.Y) / 2);
        }

    }
}
