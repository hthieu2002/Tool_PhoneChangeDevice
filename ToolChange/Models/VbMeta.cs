using Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DeepDroid.Models
{
    internal class VbMeta : IVbMeta
    {
        public string VbmetaVersion { get; set; } = "2.0";
        public string VbmetaSize { get; set; } = "16384";
        public string VbmetaAlgorithm { get; set; } = "sha256";
        public string VbmetaDigest { get; set; }

        public VbMeta(string deviceId, int sdk = 33)
        {
            if (sdk < 33)
            {
                this.VbmetaVersion = "1.1";
            }

            if (sdk < 33)
            {
                this.VbmetaSize = "4160";
            }

            this.VbmetaDigest = generateDigest(deviceId);
        }

        public string generateDigest(string deviceId)
        {
            string keybox = ADBService.runCMDRoot(string.Format("shell cat /data/local/tmp/keybox.xml"), deviceId);
            if (string.IsNullOrWhiteSpace(keybox))
            {
                Console.WriteLine("Không lấy được keybox từ thiết bị.");
                return "4d4ee7790367a25a451e83590d88e4572235c0a595f85366d98126e52bdf7841";
            }

            byte[] der = ExtractDerFromPemOrBase64(keybox);

            if (der == null || der.Length == 0)
            {
                Console.WriteLine("Không tìm thấy DER bytes hợp lệ trong keybox.xml");
                return "4d4ee7790367a25a451e83590d88e4572235c0a595f85366d98126e52bdf7841"; // avoid null reference / exception
            }

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(der);
                var sb = new StringBuilder(64);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private byte[] ExtractDerFromPemOrBase64(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var ecPemRegex = new Regex(@"-----BEGIN\s+EC\s+PRIVATE\s+KEY-----(.*?)-----END\s+EC\s+PRIVATE\s+KEY-----",
                                       RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var m = ecPemRegex.Match(text);
            if (m.Success)
            {
                string b64 = Regex.Replace(m.Groups[1].Value, @"\s+", ""); // remove whitespace/newlines
                if (TryBase64Decode(b64, out var bytes)) return bytes;
            }

            var privPemRegex = new Regex(@"-----BEGIN\s+(?:[A-Z0-9 \-]+PRIVATE KEY)-----(.*?)-----END\s+(?:[A-Z0-9 \-]+PRIVATE KEY)-----",
                                         RegexOptions.Singleline | RegexOptions.IgnoreCase);
            m = privPemRegex.Match(text);
            if (m.Success)
            {
                string b64 = Regex.Replace(m.Groups[1].Value, @"\s+", "");
                if (TryBase64Decode(b64, out var bytes)) return bytes;
            }

            var longBase64 = Regex.Match(text, @"[A-Za-z0-9+/=]{80,}");
            if (longBase64.Success)
            {
                string cleaned = Regex.Replace(longBase64.Value, @"\s+", "");
                if (TryBase64Decode(cleaned, out var bytes)) return bytes;
            }

            return null;
        }

        private bool TryBase64Decode(string s, out byte[] bytes)
        {
            bytes = null;
            if (string.IsNullOrEmpty(s)) return false;

            int mod = s.Length % 4;
            if (mod != 0) s = s.PadRight(s.Length + (4 - mod), '=');

            try
            {
                bytes = Convert.FromBase64String(s);
                if (bytes.Length < 16) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}