using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using POCO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public class JsonService<T>
    {
        public static void saveJsonToFile(string path, T obj, bool camelCaseConverted = false)
        {
            try
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    if (!camelCaseConverted)
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(streamWriter, obj);
                    }
                    else
                    {
                        DefaultContractResolver contractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        };

                        string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                        {
                            ContractResolver = contractResolver,
                            Formatting = Formatting.Indented
                        });
                        streamWriter.Write(json);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public static void saveConfiguration(string path, List<T> listObj)
        {
            try
            {
                LocalFileService.createFileIfNotExist(path, "[]");
                var contentWriteToFile = JsonConvert.SerializeObject(listObj, Formatting.Indented);
                LocalFileService.writeTextFile(path, contentWriteToFile);
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Console.WriteLine(ex.Message);
#endif
            }
        }
        public static void deleteBySerial(string path, string serial)
        {
            try
            {
                LocalFileService.createFileIfNotExist(path, "[]");
                var content = File.ReadAllText(path);
                var listObj = JsonConvert.DeserializeObject<List<DeviceConfigModel>>(content);
                var deviceModel = listObj.FirstOrDefault(x => x.Serial.Equals(serial));
                if(deviceModel != null)
                {
                    listObj.Remove(deviceModel);
                    JsonService<DeviceConfigModel>.saveConfiguration(path, listObj);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Console.WriteLine(ex.Message);
#endif
            }
        }
        public static List<T> loadConfiguration(string path)
        {
            try
            {
                
                
                
                LocalFileService.createFileIfNotExist(path, "[]");
                var content = File.ReadAllText(path);
                var listObj = JsonConvert.DeserializeObject<List<T>>(content);
#if DEBUG
                System.Console.WriteLine("Done loadConfiguration get type {0}", typeof(T).FullName);
#endif
                return listObj;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Console.WriteLine(ex.Message);
#endif
                return null;
            }
        }
        public static List<T> loadConfigurationFromResource(string resourceFullName)
        {
            var pocoAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name.Contains("POCO"));
            using (var stream = pocoAssembly.GetManifestResourceStream("POCO.Resources." + resourceFullName))
            {
                TextReader tr = new StreamReader(stream);
                var listCarriers = JsonConvert.DeserializeObject<List<T>>(tr.ReadToEnd());
#if DEBUG
                System.Console.WriteLine("Done loadConfiguration get type {0}", typeof(T).FullName);
#endif
                return listCarriers;
            }
        }

        public static Dictionary<string, string> loadDeviceIP(string responseAPI)
        {
            var settings = new JsonSerializerSettings { Error = (se, ev) => ev.ErrorContext.Handled = true };
            Dictionary<string, string> ipResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseAPI, settings);
            return ipResponse;
        }
        public static string bindingAppDatasToDecriptionJson(List<AppData> listObject)
        {
            listObject.ForEach(obj =>
            {
                obj.AbsolutePath = obj.AbsolutePath.TrimEnd('/');
                obj.AbsolutePath = obj.AbsolutePath.Substring(0, obj.AbsolutePath.LastIndexOf('/') + 1);
            });
            return JsonConvert.SerializeObject(listObject).Replace("\"", "\\\"");
        }
        public static List<T> textToJsonListObjects(string rawText)
        {
            return JsonConvert.DeserializeObject<List<T>>(rawText);
        }
        public static T textToJsonObject(string rawText)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(rawText);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#pragma warning disable CS8603 // Possible null reference return.
                return default(T);
#pragma warning restore CS8603 // Possible null reference return.
            }
        }
        public static string objectToJson(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T loadSingleConfiguration(string path)
        {
            try
            {
                var jsonPath = File.ReadAllText(path);
                var obj = JsonConvert.DeserializeObject<T>(jsonPath);
                return obj;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                return default(T);
            }
        }
    }
}
