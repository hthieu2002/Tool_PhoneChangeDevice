using POCO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public static class RandomService
    {
        private static Random rand = new Random(); //global random for double value
        public static string getRandomStringHex16Digit()
        {
            int num1 = rand.Next(0, int.MaxValue);
            int num2 = rand.Next(0, int.MaxValue);

            return string.Concat(num1.ToString("x"), num2.ToString("x"));
        }
        public static string getRandomStringHex64Digit()
        {
            byte[] keyBytes = new byte[32];
            rand.NextBytes(keyBytes);
            return BitConverter.ToString(keyBytes).Replace("-", string.Empty);
        }
        public static string generateIpv6()
        {
            
            var ipClass = new List<string>();
            ipClass.Add("fe80");
            ipClass.Add("");
            for (int i = 0; i < 4; i++)
            {
                byte[] bytes = new byte[2];
                rand.NextBytes(bytes);
                ipClass.Add(BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower());
            }
            return string.Join(":", ipClass);
        }
        public static string generateRandomHostName()
        {
            string prefix = "SWDH";
            return string.Concat(prefix, randomInRange(1111, 8888).ToString());
        }
        private static int generateCheckSumICCID(string number)
        {
            //https://www.theiphonewiki.com/wiki/ICCID
            //https://www.smartjac.biz/index.php/support/main-menu?view=kb&kbartid=4&tmpl=component&print=1
            //http://phone.fyicenter.com/1155_ICCID_SIM_Card_Number_Checker_Decoder.html#Result
            var sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                if (i % 2 != 0 && ((int)char.GetNumericValue(number[i])) * 2 >= 10)
                {
                    var currentNum = ((int)char.GetNumericValue(number[i])) * 2;
                    var num1 = (int)char.GetNumericValue(currentNum.ToString()[0]);
                    var num2 = (int)char.GetNumericValue(currentNum.ToString()[1]);
                    sum += (num1 + num2);
                }
                else
                {
                    sum += (int)char.GetNumericValue(number[i]);
                }
            }
            return (sum * 9) % 10;
        }

        public static int randomFromArrayLength(int length)
        {
            var random = new Random();
            return random.Next(length);
        }

        public static T PickRandomFromEnumarable<T>(IEnumerable<T> enumerable) 
        {
            return enumerable.ElementAtOrDefault(randomFromArrayLength(enumerable.Count()));
        } 

        public static string generateIMSI(string MCC, string MNC)
        {
            var msin = rand.NextDouble().ToString().Substring(2, (MCC.Length + MNC.Length) == 6 ? 9 : 10);
            return String.Concat(MCC, MNC, msin);
        }

        public static string generateICCID(string countryCode, string mnc)
        {
            var mm = "89";// constant of industry telecom
            var cc = countryCode;
            var ii = mnc;
            var account_id = rand.NextDouble().ToString().Substring(2, 12); // get random 12 digits
            var iccid = mm + cc + ii + account_id;
            var checksum_digit = generateCheckSumICCID(iccid).ToString();

            return iccid + checksum_digit;
        }
        public static string generateICCID(string countryCode)
        {
            var mm = "89";// constant of industry telecom
            var cc = countryCode;
            var ii = "01";
            var account_id = rand.NextDouble().ToString().Substring(2, 12); // get random 12 digits
            var iccid = mm + cc + ii + account_id;
            var checksum_digit = generateCheckSumICCID(iccid).ToString();

            return iccid + checksum_digit;
        }
        public static string generateWifiMacAddress()
        {
            var listStr = new List<string>();
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var preparedList = File.ReadAllLines(Path.Combine(exePath, "Resources/mac.txt")).ToList();
                var prefixMac = preparedList[rand.Next(preparedList.Count)].ToLower();
                listStr.AddRange(prefixMac.Split(':').ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at random mac: {0}", ex);
                listStr.Add("38");
                listStr.Add("e6");
                listStr.Add("a0");
            }
            finally
            {
                for (int i = 0; i < 3; i++)
                {
                    var randomInt = rand.Next(0, 253);
                    listStr.Add(randomInt.ToString("x2"));
                }
            }
            return string.Join(":", listStr);
        }
        public static string generateMacAddress()
        {
            var listStr = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                var randomInt = rand.Next(0, 256);
                listStr.Add(randomInt.ToString("x2"));
            }
            return string.Join(":", listStr);
        }
        public static int randomInRangeButExclude(int min, int max, int minExclude, int maxExclude)
        {
            var result = rand.Next(min, max);
            while (result <= maxExclude && result >= minExclude)
            {
                result = rand.Next(min, max);
            }
            return result;
        }
        public static int randomInRange(int min, int max)
        {
            return rand.Next(min, max);// could be equal to min but should not equal max
        }
        public static string randomInStringArray(string[] collection)
        {
            var number = rand.Next(0, collection.Length);
            return collection[number];
        }
        public static bool percentSaveRRS(decimal percent)
        {
            var random = new Random();
            return random.Next(0, 100) <= (int)percent;
        }

        public static BuildVersion randomOS()
        {
            var listOsVersions = Android_Version.BUILD_VERSIONS;
            var randomIndex = randomInRange(0, listOsVersions.Count);
            return listOsVersions[randomIndex];
        }

        public static string generateBuildID()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var str1 = new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[rand.Next(s.Length)]).ToArray());

            const string nums = "0123456789";

            var str2 = new string(Enumerable.Repeat(nums, 6)
                .Select(s => s[rand.Next(s.Length)]).ToArray());

            var str3 = new string(Enumerable.Repeat(nums, 3)
                .Select(s => s[rand.Next(s.Length)]).ToArray());

            return string.Format("{0}.{1}.{2}", str1, str2, str3);
        }

        public static string generatePhoneNumber()
        {
            int min = 100000000;
            int max = 999999999;
            return randomInRange(min, max).ToString();
        }

        public static string generateRadioVersion()
        {
            string[] preparedList = { "J410FXXS2ATF1", "A260FJVU6ATF1", "G610FDDS1CTE2", "J610GUBS4BTF2", "A260GDDS7ATH2", "J701FXXU6BRI2", "G935FXXS8ETC2", "J260MUBU4ASF1", "A307FNXXU1ASI1", "A015MUBU1ASLC" };
            string[] postFix = { "X", "Y", "Z", "T", "W", "A", "B", "C" };
            return string.Concat(preparedList[randomInRange(0, preparedList.Length)], postFix[randomInRange(0, postFix.Length)]);
        }
        public static string generateUser()
        {
            string[] preparedList = { "jiren", "toppo", "mai", "tien", "roshi", "trunks", "vados", "vegito", "gogeta", "zeno", "whis", "beerus", "zamasu", "dabura", "chaozu", "goten", "yamcha", "gohan", "babara", "freeza", "cells", "majinbu", "moro", "kukirin", "kakarot", "vegeta", "piccolo", "karin", "tranmai", "rebecca", "diego", "pele", "anneste", "john", "barry", "stewarts", "perry", "barron", "henry", "richard", "monica", "rachel", "rafael", "fernando", "michael", "david", "terry", "joey", "lenon" };
            return preparedList[rand.Next(preparedList.Length)];

        }

        public static string generateSSID()
        {
            string[] preparedList = { "Tecenco152", "BuffalloAKA", "CISCO_KOA", "Tenda_5GHZ", "FAST_JC_AK", "Qualcom_AKC", "Aruba_BOC", "MICRO_JSC", "Tenda_JKX", "", "DrayTek_JKK", "TOTOLINK_MOX", "TECHI_AIDA", "CISCO_57A", "Cambium_JUI", "Fortinet_10A", "Ubiquiti_MOX" };
            string[] preparedList1 = { "_MCA", "_X1", "_X2", "_X3", "_X4", "_Tollouse", "_MiA", "_Snap", "_JNEY", "_KAM", "_MCK", "", "_LCU", "_AKO", "", "_JUO" };
            return string.Concat(preparedList[rand.Next(preparedList.Length)], preparedList1[rand.Next(preparedList1.Length)], randomInRange(10, 200).ToString());
        }
        public static Dictionary<string, string> randomBirthdayAndGender()
        {
            var basicInfo = new Dictionary<string, string>();
            var month = BasicGmailInfo.MONTHS[rand.Next(0, BasicGmailInfo.MONTHS.Length - 1)];
            var day = rand.Next(1, 28).ToString();
            var year = rand.Next(1990, 2002).ToString();
            var gender = BasicGmailInfo.GENDERS[rand.Next(0, BasicGmailInfo.GENDERS.Length - 1)];
            basicInfo.Add(BasicGmailInfo.MONTH_FLAG, month);
            basicInfo.Add(BasicGmailInfo.DAY_FLAG, day);
            basicInfo.Add(BasicGmailInfo.YEAR_FLAG, year);
            basicInfo.Add(BasicGmailInfo.GENDER_FLAG, gender);
            return basicInfo;
        }

        public static string getLineageNumberVersion(string releaseVersion)
        {
            string result = "16.0";
            switch (releaseVersion)
            {
                case "8.1.0":
                    result = "15.1";
                    break;
                case "9":
                    result = "16.0";
                    break;
                case "10":
                    result = "17.1";
                    break;
                case "11":
                    result = "18.1";
                    break;
                default:
                    break;
            }
            return result;
        }
        public static string generateLineageOsVersion(string releaseVersion)
        {
            DateTime minDate = new DateTime(2020, 01, 01);
            TimeSpan timeDiff = new System.TimeSpan(
                RandomService.randomInRange(0, 300)
                , RandomService.randomInRange(0, 24)
                , RandomService.randomInRange(0, 60)
                , RandomService.randomInRange(0, 40));
            var dateTimeRandomToString = minDate.Add(timeDiff).ToString("yyyyMMdd");
            return string.Format("{0}-{1}-OFFICIAL", getLineageNumberVersion(releaseVersion), dateTimeRandomToString);
        }
        public static string randomPasswordByName(string firstName, string lastName)
        {
            return string.Concat(firstName, lastName, rand.Next(1000, 99999)).Replace(" ", "").ToLower();
        }
        public static string randomCharacters(int length = 1)
        {
            const string chars = "abcdefghijklmnopqrstuvw0123456789ABCDEFGHIJKLMNOPQRSTUVW";
            var str = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rand.Next(s.Length)]).ToArray());
            return str;
        }
        // let's return min and max in one go as a value tuple
        public static (int min, int max) MaxIntWithXDigits(this int x)
        {
            if (x <= 0 || x > 9)
                throw new ArgumentOutOfRangeException(nameof(x));

            int min = (int)Math.Pow(10, x - 1);

            return (min == 1 ? 0 : min, min * 10 - 1);
        }
        public static string generatePasswordBip0039(int maxChar = 10) // at least 10 char
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                var listWord = File.ReadAllLines(Path.Combine(exePath, "Resources/bip0039.txt"));
                var wordSelected = listWord[randomInRange(0, listWord.Length)];
                var numOfPostFixNumber = maxChar - wordSelected.Length;
                var numberSelected = randomInRange(MaxIntWithXDigits(numOfPostFixNumber).min, MaxIntWithXDigits(numOfPostFixNumber).max);
                return string.Concat(wordSelected, numberSelected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public static string generateGateway()
        {
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //string exePath = AppDomain.CurrentDomain.BaseDirectory; // For Unit test
                var resources = File.ReadAllLines(Path.Combine(exePath, "Resources/gateway.txt"));
                var gateway = resources[randomInRange(0, resources.Length)];
                return gateway;
            }
            catch
            {
                return "10.0.0.";
            }
        }
        public static Dictionary<string, string> generateSubnetMask()
        {
            var dicts = new Dictionary<string, string>[] 
            { 
                new Dictionary<string, string>{ { "mask", "255.255.255.255" }, { "length", "32" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.254" }, { "length", "31" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.252" }, { "length", "30" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.248" }, { "length", "29" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.240" }, { "length", "28" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.224" }, { "length", "27" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.192" }, { "length", "26" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.128" }, { "length", "25" } },
                new Dictionary<string, string>{ { "mask", "255.255.255.0" }, { "length", "24" } },

            };
            
            return dicts[randomFromArrayLength(dicts.Length)];
        }
        public static string generateName(bool requireSingle = false)
        {
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var listName = File.ReadAllLines(Path.Combine(exePath, "Resources/names.txt"));
                var isSingleWord = randomInRange(1, 7) % 4 != 0; //only 16.66% is double word
                if (isSingleWord || requireSingle)
                {
                    return listName[randomInRange(0, listName.Length)];
                }
                else
                {
                    var name1 = listName[randomInRange(0, listName.Length)];
                    var name2 = listName[randomInRange(0, listName.Length)];
                    return string.Format("{0} {1}", name1, name2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public static string getFbName()
        {
            try
            {
                var result = string.Empty;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                var listName = File.ReadAllLines(Path.Combine(exePath, "Resources/fb_name.txt")).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
                listName.RemoveAll(x => string.IsNullOrEmpty(x));
                if (listName.Count > 0)
                {
                    result = listName.First();
                    listName.RemoveAt(0);
                    File.WriteAllLines(Path.Combine(exePath, "Resources/fb_name.txt"), listName.ToArray());
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;
            }
        }
        public static Dictionary<string, string> randomNameFromResource()
        {
            var fullname = new Dictionary<string, string>();
            var listNames = JsonService<Username>.loadConfigurationFromResource("usernames.json");
            var name = listNames.Where(n => n.CountryCode.Equals("vn")).First();
            fullname.Add(BasicGmailInfo.FIRSTNAME_FLAG, name.FirstNames[randomFromArrayLength(name.FirstNames.Length - 1)]);
            fullname.Add(BasicGmailInfo.LASTNAME_FLAG, name.LastNames[randomFromArrayLength(name.LastNames.Length - 1)]);
            fullname.Add(BasicGmailInfo.MIDNAME_FLAG, name.MidNames[randomFromArrayLength(name.MidNames.Length - 1)]);
            return fullname;
        }
        public static string generateGSFNumber()
        {
            var number = "3"; // gsf begin with 3
            for (int i = 1; i < 19; i++)
            {
                number = string.Concat(number, rand.Next(0, 10));
            }
            //Enumerable.Range(1, 9).Select(n => number = string.Concat(number, rand.Next(0, 10))).Take(18);
            return number;
        }

        public static DateTime generateDate()
        {
            DateTime date = new DateTime(2018, 11, 27);
            TimeSpan timeDiff = new System.TimeSpan(
                RandomService.randomInRange(0, 700)
                , RandomService.randomInRange(0, 24)
                , RandomService.randomInRange(0, 60)
                , RandomService.randomInRange(0, 40));
            date.Add(timeDiff);
            return date;
        }

        public static string generateRandomTimeFromBaseTimeInHex(string hexValue)
        {
            long currentTimeInHexa = Int64.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
            int offsetValue = randomInRange(-99999999, 99999999); Console.WriteLine("Offset value = {0}", offsetValue);
            currentTimeInHexa += offsetValue;
            return currentTimeInHexa.ToString("x");
        }

        public static string generateBase64String()
        {
            var resultBase64Safe = "/+"; ;
            while (resultBase64Safe.Contains('/') || resultBase64Safe.Contains('+'))
            {
                var random = new Random();
                Byte[] b = new Byte[16];
                random.NextBytes(b);
                //return Convert.ToBase64String(b).Replace('/', '_').Replace('+', '-');
                resultBase64Safe = Convert.ToBase64String(b);
            }
            return resultBase64Safe;
        }

        public static string generateWebviewVersion()
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                var listWebview = File.ReadAllLines(Path.Combine(exePath, "Resources/webview.txt"));
#pragma warning restore CS8604 // Possible null reference argument.
                return listWebview[randomInRange(0, listWebview.Length)];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
