namespace ToolChange.Models
{
    public class DeviceInfo
    {
        public string DisplayName { get; set; }
        public string DeviceId { get; set; }

        public DeviceInfo(string name, string id)
        {
            DisplayName = name;
            DeviceId = id;
        }
    }

}
