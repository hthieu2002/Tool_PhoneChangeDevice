using POCO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public class LocalFileService
    {
        public static void createFileIfNotExist(string path, string writer = "")
        {
            try
            {
                if (!File.Exists(path))
                {
                    var directory_path = Path.GetDirectoryName(path);
                    if(!string.IsNullOrEmpty(directory_path) && !Directory.Exists(directory_path))
                    {
                        Directory.CreateDirectory(directory_path);
                    }
                    using (FileStream fileStream = File.Create(path))
                    {
                        if (fileStream.CanWrite)
                        {
                            using (StreamWriter streamReader = new StreamWriter(fileStream))
                            {
                                streamReader.Write(writer);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static string pathLocalOffer(string localPath, string deviceId)
        {
            return string.Format(@"{0}\Devices\{1}\{2}", localPath, deviceId, "offer.json");
        }
        public static string pathLocalApp(string localPath, string deviceId)
        {
            return string.Format(@"{0}\Devices\{1}\{2}", localPath, deviceId, "apps.json");
        }
        public static string pathLocalExportFiles(string localPath, string deviceId)
        {
            return string.Format(@"{0}\Devices\{1}\{2}\", localPath, deviceId, "Export");
        }
        public static string pathLocalScript(string localPath, string package, string scriptType)
        {
            return string.Format(@"{0}\Scripts\{1}\{2}", localPath, package, scriptType);
        }
        public static string pathLocalCache(string localPath, string deviceId)
        {
            return string.Format(@"{0}\Devices\{1}\Cache", localPath, deviceId);
        }
        public static string[] readAllLinesTextFile(string path)
        {
            createFileIfNotExist(path);
            return File.ReadAllLines(path);
        }
        public static string readAllTextFile(string path)
        {
            createFileIfNotExist(path);
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
        public static void writeTextFile(string path, string content, FileMode fileMode = FileMode.Truncate, bool appendLine = false)
        {
            createFileIfNotExist(path);
            FileInfo fi = new FileInfo(path);
            try
            {
                using (TextWriter txtWriter = new StreamWriter(fi.Open(fileMode, (fileMode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None)))
                {
                    if (appendLine)
                    {
                        txtWriter.WriteLine(content);
                    }
                    else
                    {
                        txtWriter.Write(content);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Write file ex: {0}", ex.Message);
            }
        }
        public static void saveFile(string content, string path)
        {
            createFileIfNotExist(path);
            FileInfo fi = new FileInfo(path);
            using (TextWriter txtWriter = new StreamWriter(fi.Open(FileMode.Truncate)))
            {
                txtWriter.Write(content);
            }
        }
        public static List<ScriptOffer> getScriptsLeadFromLocal(string path)
        {
            createFileIfNotExist(string.Concat(path, @"/Custom/Lead"));
            string[] arrayScriptPackages = Directory.GetDirectories(path);
            List<ScriptOffer> scripts = new List<ScriptOffer>();
            foreach (string script in arrayScriptPackages)
            {
                scripts.Add(new ScriptOffer
                {
                    NamePackage = Path.GetFileName(script),
                    FullPath = string.Concat(Path.GetFileName(script), @"/Lead")
                });
            }
            return scripts;
        }
        public static List<ScriptApp> getScriptsAppFromLocal(string path)
        {
            createFileIfNotExist(string.Concat(path, @"/Custom/App"));
            string[] arrayScriptPackages = Directory.GetDirectories(path);
            List<ScriptApp> scripts = new List<ScriptApp>();
            foreach (string script in arrayScriptPackages)
            {
                scripts.Add(new ScriptApp
                {
                    NamePackage = Path.GetFileName(script),
                    FullPath = string.Concat(Path.GetFileName(script), @"/App")
                });
            }
            return scripts;
        }
        public static void syncCacheSettingForAllDevice(string sourceFilePath, string destinationDirPath, string srcSerial)
        {
            var dirs = Directory.GetDirectories(destinationDirPath, "*", SearchOption.TopDirectoryOnly).Where(d => !d.EndsWith(srcSerial));
            foreach (string dir in dirs)
            {
                var dest = string.Concat(dir, @"\Cache\settings.txt");
                File.Copy(sourceFilePath, dest, true);
            }
        }

        public static long getDirectorySize(string dirPath)
        {
            try
            {
                return Directory.EnumerateFiles(dirPath).Sum(x => new FileInfo(x).Length)
                        + Directory.EnumerateDirectories(dirPath).Sum(x => getDirectorySize(x));
            }
            catch
            {
                return 0L;
            }
        }
        public static void replaceLineInFileContain(string txtPath, string textContain, string newStr)
        {
            try
            {
                var file = File.ReadAllText(txtPath);
                string lineOfTextTobeReplaced = File.ReadAllLines(txtPath).FirstOrDefault(line => line.Contains(textContain));
                file = file.Replace(lineOfTextTobeReplaced, newStr);
                File.WriteAllText(txtPath, file);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
            }
        }
        public static void replaceAllTextInFile(string txtPath, string oldStr, string newStr)
        {
            try
            {
                var file = File.ReadAllText(txtPath);
                file = file.Replace(oldStr, newStr);
                File.WriteAllText(txtPath, file);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
            }
        }


        //        public static bool setAccessDirectories(string dirName)
        //        {
        //            try
        //            {
        //                if (Directory.Exists(dirName) == false)
        //                    throw new Exception(string.Format("Directory {0} does not exist, so permissions cannot be set.", dirName));

        //                DirectoryInfo dinfo = new DirectoryInfo(dirName);
        //                DirectorySecurity dSecurity = dinfo.GetAccessControl();
        //                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
        //                dinfo.SetAccessControl(dSecurity);
        //                return true;
        //            } catch (Exception ex)
        //            {
        //#if DEBUG
        //                Console.WriteLine(ex.Message);
        //#endif
        //                return false;
        //            }
        //        }
        public static void writeAllLinesTextFile(string filePath, string[] lines)
        {
            try
            {
                createFileIfNotExist(filePath);
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static string getAvailableLine(string filePath, char sep_char, int correct_column, string deviceId)
        {
            var result = string.Empty;
            var lines = readAllLinesTextFile(filePath).ToList();
            for(var i = 0; i < lines.Count; i++)
            {
                if(lines[i].Split(sep_char).Length == correct_column)
                {
                    result = lines[i];
                    lines[i] += sep_char.ToString() + deviceId;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(result)) //update origin file
            {
                writeAllLinesTextFile(filePath, lines.ToArray());
            }

            return result;
        }
    }
}