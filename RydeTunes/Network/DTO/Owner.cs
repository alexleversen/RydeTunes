using System;

namespace RydeTunes.Network.DTO
{
  public class Owner
  {
      public string display_name { get; set; }
      public ExternalUrls external_urls { get; set; }
      public string href { get; set; }
      public string id { get; set; }
      public string type { get; set; }
      public string uri { get; set; }
  }
}
