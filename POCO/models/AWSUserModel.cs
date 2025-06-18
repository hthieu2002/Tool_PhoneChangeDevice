namespace POCO.Models
{
    public class AWSUserModel
    {
        public string IdToken { get; set; }
        public string Custom_TelegramBot { get; set; } = "";
        public string Custom_TelegramRoom { get; set; } = "";
        public string Custom_GoogleDriveFolderId { get; set; } = "";
        public bool isActivated { get; set; } = false;
        public int Custom_UsingKeyStrokes { get; set; } = 0;

    }
}
