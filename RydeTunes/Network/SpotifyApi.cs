using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RydeTunes.Network
{
    class SpotifyApi
    {
        public static SpotifyApi Instance;

        private static string SPOTIFY_API_URL = "https://api.spotify.com";
        private static string RYDETUNES_PLAYLIST_NAME = "Ryde Tunes Collaborative Playlist"

        private HttpClient spotifyClient;


        public SpotifyApi()
        {
            Instance = this;
        }
        public void UpdateToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                throw new Exception("This Value can't be empty");
            }
            spotifyClient = new HttpClient();

            spotifyClient.BaseAddress = new Uri(SPOTIFY_API_URL);
            spotifyClient.DefaultRequestHeaders.Accept.Clear();
            spotifyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public event EventHandler SessionInvalidated;

        /* Driver methods */

        public string GetRydeTunesPlaylist()
        {
            // Get playlist if it exists
            Playlist p = SearchForPlaylist(RYDETUNES_PLAYLIST_NAME);
            // Clear playlist
            ClearPlaylist(p.id);
            // Return id
            return p.id;
        }
        // Searches for playlists of the current user with the given name
        // Returns null if playlist was not found
        public async Playlist SearchForPlaylist(string playlistName)
        {
            foreach (Playlist p in GetPlaylists().items)
            {
                if (p.name.Contains(playlistName)) {
                  return p;
                }
            }
            return null;

        }
        public async List<Playlist> GetPlayists() {
            HttpResponseMessage response = await spotifyClient.GetAsync("v1/me/playlists");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GetPlaylistsResponse>(await response.Content.ReadAsStringAsync()).items;
        }
        public async Playlist GetPlaylist(string userId, string playlistId) {
            HttpResponseMessage response = await spotifyClient.getAsync("v1/users/"+ userId + "/playlists/" + playlistId);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Playlist>(await response.Content.ReadAsStringAsync());
        }
        public async List<Track> GetPlaylistTracks(string userId, string playlistId) {
              HttpResponseMessage response = await spotifyClient.getAsync("v1/users/"+ userId + "/playlists/" + playlistId + "/tracks");
              return Newtonsoft.Json.JsonConvert.DeserializeObject<Playlist>(await response.Content.ReadAsStringAsync());
        }

        public bool PlaylistIsEmpty(string playlistId)
        {
            return true; //TODO: Implement
        }

        public void ClearPlaylist(string playlistId)
        {
            //TODO: Implement
        }
        public void ClearPlaylistTrack(string trackId, string playlistId) {
            //TODO implement
        }

        /* Passenger methods */

        public List<String> SearchForSong(string searchTerms)
        {
            // TODO
            return new List<string>();
        }


    }
}
