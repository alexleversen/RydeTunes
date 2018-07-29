using System;

namespace RydeTunes.Network.DTO
{
  public class CreatePlaylistRequest {
    public string name { get; set; }
    public bool @public { get; set; }
  }
}
