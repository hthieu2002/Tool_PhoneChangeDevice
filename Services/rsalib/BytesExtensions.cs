using System;

namespace Services.RSALib
{
    static class BytesExtensions
    {
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}
