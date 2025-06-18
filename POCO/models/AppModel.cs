using Newtonsoft.Json;
namespace POCO.Models
{
    public class App
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TimeCreate { get; set; }
        public string Package { get; set; }
        public string LastRun { get; set; }
        public int Count { get; set; }
        public string IP { get; set; }
        public string Note { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public ScriptApp Script { get; set; }
    }
    public class ScriptApp
    {
        private string namePackage;
        private string fullPath;
        public string NamePackage { get => namePackage; set => namePackage = value; }
        public string FullPath { get => fullPath; set => fullPath = value; }
    }
}
