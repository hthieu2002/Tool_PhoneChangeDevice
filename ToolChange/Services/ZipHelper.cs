using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.Services
{
    using ICSharpCode.SharpZipLib.Zip;
    using System.IO;

    public static class ZipHelper
    {
        public static void ExtractZipWithPassword(string zipFilePath, string extractPath, string password = null)
        {
            using (FileStream fs = File.OpenRead(zipFilePath))
            using (ZipFile zipFile = new ZipFile(fs))
            {
                if (!string.IsNullOrEmpty(password))
                    zipFile.Password = password;

                foreach (ZipEntry entry in zipFile)
                {
                    if (!entry.IsFile) continue; // skip directories

                    string entryFileName = entry.Name;
                    string fullZipToPath = Path.Combine(extractPath, entryFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullZipToPath));

                    using (Stream zipStream = zipFile.GetInputStream(entry))
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        zipStream.CopyTo(streamWriter);
                    }
                }
            }
        }
    }

}
