using System.Reflection;
using System.Security.Cryptography;

namespace Services.RSALib
{
    [Obfuscation(Exclude = false)]
    public static class RSAGenerator
    {
        public static string generateRSAPubKey(int keySize = 2048)
        {
            RSA rsa = new RSACryptoServiceProvider(keySize);
            string publicKeyXML = rsa.ToXmlString(false);
            return RsaKeyConverter.XmlToPem(publicKeyXML, true);
        }
    }
}
