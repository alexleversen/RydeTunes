using System;

namespace RydeTunes.Network.DTO
{
  public class PlaylistTrackResponse {
        public string href { get; set; }
        public List<PlaylistTrack> items { get; set; }
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
  }

  public class AddedBy
  {
      public ExternalUrls external_urls { get; set; }
      public string href { get; set; }
      public string id { get; set; }
      public string type { get; set; }
      public string uri { get; set; }
  }
  public class VideoThumbnail
  {
      public object url { get; set; }
  }

  public class PlaylistTrack
  {
      public DateTime added_at { get; set; }
      public AddedBy added_by { get; set; }
      public bool is_local { get; set; }
      public object primary_color { get; set; }
      public Track track { get; set; }
      public VideoThumbnail video_thumbnail { get; set; }
  }
}
