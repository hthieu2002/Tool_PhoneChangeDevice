using OpenPop.Mime;

namespace POCO.Models
{
    public class Pop3Mail
    {
        public int MessageNumber { get; set; }
        public Message Message { get; set; }
    }
}
