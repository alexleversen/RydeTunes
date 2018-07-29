using System;
using System.Collections.Generic;

namespace RydeTunes.Network.DTO
{
  public class DeletePlaylistTrackRequest {
    public List<TrackToDelete> tracks { get; set; }
  }
  public class TrackToDelete {
    public string uri { get; set; }
    public List<int> positions { get; set; }
  }
}
