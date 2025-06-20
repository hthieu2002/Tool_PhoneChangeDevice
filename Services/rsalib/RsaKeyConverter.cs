﻿using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.RSALib
{
    [Obfuscation(Exclude = false)]
    static class RsaKeyConverter
    {
        public static string XmlToPem(string xml, bool isOnlyKeyContent = false)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(xml);

                AsymmetricCipherKeyPair keyPair = rsa.GetKeyPair(); // try get private and public key pair
                if (keyPair != null) // if XML RSA key contains private key
                {
                    PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
                    return FormatPem(privateKeyInfo.GetEncoded().ToBase64(), "RSA PRIVATE KEY", isOnlyKeyContent);
                }

                RsaKeyParameters publicKey = rsa.GetPublicKey(); // try get public key
                if (publicKey != null) // if XML RSA key contains public key
                {
                    SubjectPublicKeyInfo publicKeyInfo =
                        SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
                    return FormatPem(publicKeyInfo.GetEncoded().ToBase64(), "PUBLIC KEY", isOnlyKeyContent);
                }
            }

            throw new InvalidKeyException("Invalid RSA Xml Key");
        }

        public static async Task<string> XmlToPemAsync(string xml)
        {
            return await Task.Run(() => XmlToPem(xml));
        }

        private static string FormatPem(string pem, string keyType, bool isOnlyKeyContent)
        {
            var sb = new StringBuilder();
            if (!isOnlyKeyContent)
                sb.AppendFormat("-----BEGIN {0}-----\n", keyType);

            int line = 1, width = 64;

            while ((line - 1) * width < pem.Length)
            {
                int startIndex = (line - 1) * width;
                int len = line * width > pem.Length
                              ? pem.Length - startIndex
                              : width;
                if (!isOnlyKeyContent)
                    sb.AppendFormat("{0}\n", pem.Substring(startIndex, len));
                else
                    sb.AppendFormat("{0}", pem.Substring(startIndex, len));
                line++;
            }
            if (!isOnlyKeyContent)
                sb.AppendFormat("-----END {0}-----\n", keyType);
            return sb.ToString();
        }

        public static string PemToXml(string pem)
        {
            if (pem.StartsWith("-----BEGIN RSA PRIVATE KEY-----")
                || pem.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                return GetXmlRsaKey(pem, obj =>
                {
                    if ((obj as RsaPrivateCrtKeyParameters) != null)
                        return DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)obj);
                    var keyPair = (AsymmetricCipherKeyPair)obj;
                    return DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)keyPair.Private);
                }, rsa => rsa.ToXmlString(true));
            }

            if (pem.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                return GetXmlRsaKey(pem, obj =>
                {
                    var publicKey = (RsaKeyParameters)obj;
                    return DotNetUtilities.ToRSA(publicKey);
                }, rsa => rsa.ToXmlString(false));
            }

            throw new InvalidKeyException("Unsupported PEM format...");
        }

        public static async Task<string> PemToXmlAsync(string pem)
        {
            return await Task.Run(() => PemToXml(pem));
        }

        private static string GetXmlRsaKey(string pem, Func<object, RSA> getRsa, Func<RSA, string> getKey)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            using (var sr = new StreamReader(ms))
            {
                sw.Write(pem);
                sw.Flush();
                ms.Position = 0;
                var pr = new PemReader(sr);
                object keyPair = pr.ReadObject();
                using (RSA rsa = getRsa(keyPair))
                {
                    var xml = getKey(rsa);
                    return xml;
                }
            }
        }

    }
}
