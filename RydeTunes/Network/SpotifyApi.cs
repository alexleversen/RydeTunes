using System;
using System.Collections.Generic;
using System.Text;

namespace RydeTunes.Network
{
    class SpotifyApi
    {
        public static SpotifyApi Instance;

        public event EventHandler SessionInvalidated;

        public bool CheckSpotifySession()
        {
            return true; //false if playlist is empty AND a song has been added to it this session
        }

        public bool PlaylistIsEmpty()
        {
            return true; //TODO: Implement
        }

        public void DisconnectFromPlaylist()
        {
            //TODO: Implement
        }
    }
}
