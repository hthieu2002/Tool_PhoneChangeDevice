using System;

namespace POCO.Models
{
    public class YoutubeVideoModel
    {
        public string VideoLink { get; set; }
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ChannelTitle { get; set; }
        public TimeSpan Duration { get; set; }
        public double TotalView { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
