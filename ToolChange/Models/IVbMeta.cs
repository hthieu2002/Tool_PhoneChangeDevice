using System;

namespace DeepDroid.Models
{
    public interface IVbMeta
    {
        string VbmetaVersion { get; set; }
        string VbmetaSize { get; set; }
        string VbmetaAlgorithm { get; set; }
        string VbmetaDigest { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="deviceId">Mã thiết bị Android.</param>
        /// <returns>Chuỗi hash digest.</returns>
        string generateDigest(string deviceId);
    }
}