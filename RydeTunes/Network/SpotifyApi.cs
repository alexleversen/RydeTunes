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

        private HttpClient spotifyClient;


        private SpotifyApi(string authToken)
        {
            UpdateToken(authToken);
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
            
            // Clear playlist
            // Return id
            return "";
        }
        // Searches for playlists of the current user with the given name
        public async string SearchForPlaylist(string playlistName)
        {
            HttpResponseMessage response = await spotifyClient.GetAsync("v1/me/playlists");
            var values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
            foreach (Dictionary<string, string> i in values["items"])
            {

            }

        }

        public bool PlaylistIsEmpty(string playlistId)
        {
            return true; //TODO: Implement
        }

        public void ClearPlaylist(string playlistId)
        {
            //TODO: Implement
        }

        /* Passenger methods */

        public List<String> SearchForSong(string searchTerms)
        {
            // TODO
            return new List<string>();
        }
        

    }
}
