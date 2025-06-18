using Newtonsoft.Json.Linq;
using Services.RSALib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Services
{
    [Obfuscation(Exclude = false)]
    public class XmlService
    {
        //private static Random rand = new Random();
        //private static string getRandom64bitStringHex()
        //{
        //    int num1 = rand.Next(0, int.MaxValue);
        //    int num2 = rand.Next(0, int.MaxValue);

        //    return string.Concat(num1.ToString("X8"), num2.ToString("X8")).ToLower();
        //}
        public static string getPackagePubKeyIdentifier(string xmlPath, string packageName)
        {
            try
            {
                var listPackages = getElements(xmlPath, string.Format("//package[@name='{0}']/proper-signing-keyset", packageName));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return listPackages[0].Attribute("identifier").Value;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        //just get one account google from xml
        public static string getGoogleAccountFromXML(string xmlString)
        {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            XmlNodeList nodeAccounts = doc.SelectNodes("/accounts/authority");
            foreach (XmlNode node in nodeAccounts)
            {
                XmlAttribute typeNode = node.Attributes["type"];
                XmlAttribute accountNode = node.Attributes["account"];
                XmlAttribute authorityNode = node.Attributes["authority"];

                if (typeNode.Value.Equals("com.google")
                    && (authorityNode.Value.Equals("com.google.android.gms.auth.accountstate") || authorityNode.Value.Equals("com.google.android.gms.auth.confirm"))
                    && !string.IsNullOrEmpty(accountNode.Value)
                    //&& Regex.IsMatch(accountNode.Value,
                    //    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    //    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    //    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))
                    )
                {
                    return accountNode.Value;
                }
            }
            return string.Empty;
        }
        public static void generateRandomAndroidIdOverXmlSettings(string xmlPath)
        {
            //var cleanPath = string.Join("", xmlPath.Split(System.IO.Path.GetInvalidPathChars()));
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            XmlNodeList settingNodes = doc.SelectNodes("/settings/setting");
            var androidIdForGoogleGMS = RandomService.getRandomStringHex16Digit();
            foreach (XmlNode node in settingNodes)
            {
                XmlAttribute packageName = node.Attributes["package"];
                XmlAttribute value = node.Attributes["value"];
                XmlAttribute defaultValue = node.Attributes["defaultValue"];
                if (!packageName.Value.Equals("android"))
                {
                    value.Value = defaultValue.Value = RandomService.getRandomStringHex16Digit();
                }
                else
                {
                    value.Value = defaultValue.Value = RandomService.getRandomStringHex64Digit();
                }
                if (packageName.Value.Contains("com.google.android.gms")
                    || packageName.Value.Contains("com.android.vending")
                    || packageName.Value.Contains("com.google.android.syncadapters.calendar")
                    || packageName.Value.Contains("com.android.chrome")
                    || packageName.Value.Contains("com.google.android.calendar"))
                    value.Value = defaultValue.Value = androidIdForGoogleGMS;
            }
            doc.Save(xmlPath);
        }
        public static Dictionary<string, int> getCoordinateDumpNodeByAttr(string rawXml, string attr, string attrValue)
        {
            Dictionary<string, int> coordinates = null;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(rawXml.ToLower());
                XmlNodeList elements = doc.SelectNodes(string.Format("//node[@{0}='{1}']", attr, attrValue.ToLower()));
                XmlNode element = elements.Item(0);
                if (element != null)
                {
                    coordinates = new Dictionary<string, int>();
                    var bounds = element.Attributes["bounds"].Value.Split(new char[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var coordinateX = (Convert.ToInt32(bounds[0]) + Convert.ToInt32(bounds[2])) / 2;
                    var coordinateY = (Convert.ToInt32(bounds[1]) + Convert.ToInt32(bounds[3])) / 2;
                    coordinates.Add("x", coordinateX);
                    coordinates.Add("y", coordinateY);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Dump UI Error: " + ex.Message);
#endif
                coordinates = null;
            }
            return coordinates;
        }
        public static Dictionary<string, int> getCoordinateDumpNodeByAttrContains(string rawXml, string attr, string attrValue)
        {
            Dictionary<string, int> coordinates = null;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(rawXml.ToLower());
                XmlNodeList elements = FindElementsWithAttributeValue(doc, attr, attrValue.ToLower());
                XmlNode element = elements.Item(0);
                if (element != null)
                {
                    coordinates = new Dictionary<string, int>();
                    var bounds = element.Attributes["bounds"].Value.Split(new char[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var coordinateX = (Convert.ToInt32(bounds[0]) + Convert.ToInt32(bounds[2])) / 2;
                    var coordinateY = (Convert.ToInt32(bounds[1]) + Convert.ToInt32(bounds[3])) / 2;
                    coordinates.Add("x", coordinateX);
                    coordinates.Add("y", coordinateY);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Dump UI Error: " + ex.Message);
#endif
                coordinates = null;
            }
            return coordinates;
        }
        private static XmlNodeList FindElementsWithAttributeValue(XmlDocument doc, string attr, string attrValue)
        {
            /*string xpath = string.Format("//node[contains(translate(@{0}, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{1}')]", attr, attrValue.ToLower());*/
            string xpath = $"//node[contains(@{attr}, '{attrValue}')]";
            return doc.SelectNodes(xpath);
        }
        public static Point getCoordsOfNodeByAttr(string rawXml, string attr, string attrValue)
        {
            var point = new Point(0, 0);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(rawXml.ToLower());
                XmlNodeList elements = doc.SelectNodes(string.Format("//node[contains(@{0}, '{1}')]", attr, attrValue.ToLower()));
                XmlNode element = elements.Item(0);
                if (element != null)
                {
                    var bounds = element.Attributes["bounds"].Value.Split(new char[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    point.X = (Convert.ToInt32(bounds[0]) + Convert.ToInt32(bounds[2])) / 2;
                    point.Y = (Convert.ToInt32(bounds[1]) + Convert.ToInt32(bounds[3])) / 2;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Dump UI Error: " + ex.Message);
#endif
            }
            return point;
        }
        public static string getGAID(string rawXML)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                rawXML = rawXML.Substring(rawXML.LastIndexOf('?') + 2).Trim('\t');
                doc.LoadXml(rawXML);
                XmlNodeList elements = doc.SelectNodes("//string[@name='adid_key']");
                XmlNode element = elements.Item(0);
                return element.InnerText;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }
        public static List<XElement> getElementsByAttributeValue(string xmlPath, string attr, string attrValue, XElement xParent = null)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                IEnumerable<XElement> descendants = doc.Descendants();
                if (xParent != null)
                {
                    descendants = xParent.Descendants();
                }

                return descendants.Where(e => e.Attribute(attr) != null && e.Attribute(attr).Value.Equals(attrValue)).ToList();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return null;

            }
        }
        public static Dictionary<string, string> getProxyOverXMLWifiConfig(string xmlPath, string ssidName)
        {
            var result = new Dictionary<string, string>();
            try
            {
                var listSSID = getElementsByAttributeValue(xmlPath, "name", "SSID");
                var connectingSSIDProxy = listSSID.First(ssid => ssid.Value.Trim().Equals(ssidName.Trim()));
                var parent = connectingSSIDProxy.Parent.Parent;
                var proxyHost = getElementsByAttributeValue(xmlPath, "name", "ProxyHost", parent);
                var proxyPort = getElementsByAttributeValue(xmlPath, "name", "ProxyPort", parent);
                if (proxyHost.Count > 0 && proxyPort.Count > 0)
                {
                    result.Add("host", proxyHost[0].Value);
                    result.Add("port", proxyPort[0].Attribute("value").Value);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
            }
            return result;

        }
        public static string getAttributePackagesXML(string attr, string rawXML)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(rawXML);
                var nodes = doc.GetElementsByTagName("package");
                var node = nodes[0];
                return node.Attributes[attr].Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }
        public static void removeElements(string xmlPath, string xpathElement)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var elements = doc.XPathSelectElements(xpathElement);
                elements.Remove();
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Remove Element Exception: " + ex.Message);
            }
        }
        public static void insertElementsInParent(string xmlPath, string xpathParentElement, List<XElement> childElements)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var root = doc.XPathSelectElement(xpathParentElement);
                var firstSameTypeChild = root.XPathSelectElement(string.Format("{0}/{1}[1]", xpathParentElement, childElements[0].Name)); // Find first element has same tag name with insterted childElements
                firstSameTypeChild.AddBeforeSelf(childElements); // Add to first element
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert Elements Exception: " + ex.Message);
            }
        }
        public static List<XElement> getElements(string xmlPath, string xpathElement)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                return doc.XPathSelectElements(xpathElement).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get element exception: " + ex.Message);
                throw;
            }
        }
        public static void replaceAllAttributeElement(string xmlPath, string xPathDstElement, XElement srcElement)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var dstElement = doc.XPathSelectElement(xPathDstElement);
                dstElement.RemoveAttributes(); // Remove all dstElement attributes
                dstElement.Add(srcElement.Attributes()); // Add attributes of scrElement to dstElement
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Replace Attribute exception: " + ex.Message);
            }
        }
        public static void moveElementToFirst(string xmlPath, string xPathElement)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var element = doc.XPathSelectElement(xPathElement);
                var parent = element.Parent; // get parent element of current element
                var elementsByTagName = parent.Elements(element.Name).ToList(); // Get all childs of parent which has same tag name with current element
                if (!elementsByTagName[0].Equals(element))
                {
                    element.Remove(); // Remove old element
                    elementsByTagName[0].AddBeforeSelf(element); // Add element to first index of collections has same tag name
                }
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Move elements Error: " + ex.Message);
            }
        }
        public static void editAttribute(string xmlPath, string xPathElement, string attrName, string attrValue)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var element = doc.XPathSelectElement(xPathElement);
                element.Attribute(attrName).Value = attrValue;
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Edit Attribute exception: " + ex.Message);
            }
        }
        public static void UpdateValueInNode(string xmlPath, string nodeName, string attrValue)
        {
            try
            {
                XDocument doc = XDocument.Load(xmlPath);

                XElement element = doc.Descendants("int")
                                       .FirstOrDefault(x => (string)x.Attribute("name") == nodeName);

                if (element != null)
                {
                    XAttribute attribute = element.Attribute("value");
                    if (attribute != null)
                    {
                        attribute.Value = attrValue.ToString();
                        doc.Save(xmlPath);
                        Console.WriteLine("Value updated successfully and saved to file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        public static int GetValueInNode(string filePath, string nodeName)
        {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            string nodeValue = "";
#pragma warning restore CS0219 // Variable is assigned but its value is never used

            try
            {
                var a = ReadXmlFileDeleteNull(filePath);

                XDocument doc = XDocument.Load(filePath);

                // Tìm nút cần lấy giá trị
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                XElement element = doc.Descendants("int")
                                       .FirstOrDefault(x => (string)x.Attribute("name") == nodeName);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                // Lấy giá trị của nút
                if (element != null)
                {
                    XAttribute attribute = element.Attribute("value");
                    if (attribute != null)
                    {
                        int value;
                        if (int.TryParse(attribute.Value, out value))
                        {
                            return value;
                        }
                    }
                }

                // Trả về giá trị mặc định nếu không tìm thấy nút hoặc giá trị không hợp lệ
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 0;
            }
        }
        static string ReadXmlFileDeleteNull(string filePath)
        {
            try
            {
                // Đọc dữ liệu từ tệp XML dưới dạng mảng byte
                byte[] bytes = File.ReadAllBytes(filePath);

                // Loại bỏ bất kỳ ký tự NULL nào
                byte[] filteredBytes = Array.FindAll(bytes, b => b != 0x00);

                // Chuyển đổi mảng byte thành chuỗi UTF-8
                string xmlContent = Encoding.UTF8.GetString(filteredBytes);

                return xmlContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }
        public static void editDateTimeInstallGooglePackages(string xmlPath, params string[] packageNameList)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                foreach (var packageName in packageNameList)
                {
                    var element = doc.XPathSelectElement(string.Format("//package[@name='{0}']", packageName));
                    var valueIT = element.Attribute("it").Value;
                    var newValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
                    element.Attribute("it").Value = element.Attribute("ft").Value = element.Attribute("ut").Value = newValue;
                }
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Edit Attribute exception: " + ex.Message);
            }
        }

        public static void editBulkPublickeyIdentifier(string xmlPath)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var elements = doc.XPathSelectElements("//keyset-settings/keys/public-key");
                foreach (var element in elements)
                {
                    var pubKey = RSAGenerator.generateRSAPubKey(2048);
                    element.Attribute("value").Value = pubKey;
                }
                //                //    XmlService.editAttribute(pathXml, string.Format("//keyset-settings/keys/public-key[@identifier='{0}']", identifier), "value"

                //var gmsElement = doc.XPathSelectElement("//package[@name='com.google.android.gms']");
                //gmsElement.Attribute("it").Value;


                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Edit Attribute exception: " + ex.Message);
            }
        }

        public static void editPackagesInfo(string xmlPath, string base64StringRandom = "", params string[] packageNameList)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                foreach (var packageName in packageNameList)
                {
                    // edit time
                    var element = doc.XPathSelectElement(string.Format("//package[@name='{0}']", packageName));
                    if (element != null)
                    {
                        var valueIT = element.Attribute("it").Value;
                        var newValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
                        element.Attribute("it").Value = element.Attribute("ft").Value = element.Attribute("ut").Value = newValue;
                        // edit code path
                        element.Attribute("codePath").Value = string.Format("/data/app/{0}-{1}", packageName, base64StringRandom);
                        element.Attribute("nativeLibraryPath").Value = string.Format("/data/app/{0}-{1}/lib", packageName, base64StringRandom);
                    }
                }
                doc.Save(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Edit Attribute exception: " + ex.Message);
            }
        }
    }
}
