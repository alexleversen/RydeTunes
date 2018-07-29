using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RydeTunes.Network.DTO;

namespace RydeTunes.Network
{
    class SpotifyApi
    {
        public static SpotifyApi Instance;

        private static string SPOTIFY_API_URL = "https://api.spotify.com";
        private static string RYDETUNES_PLAYLIST_NAME = "RydeTunes Collaborative Playlist";

        private HttpClient spotifyClient;

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

        public async Task<Playlist> GetRydeTunesPlaylist(string userId)
        {
            // Get playlist if it exists
            Playlist p = await SearchForPlaylist(RYDETUNES_PLAYLIST_NAME);
            if (p == null) {
                return await CreatePlaylist(RYDETUNES_PLAYLIST_NAME, userId);
            } else {
              // Clear playlist
              await ClearPlaylist(userId, p.id);
              // Return id
              return p;
            }

        }
        // Searches for playlists of the current user with the given name
        // Returns null if playlist was not found
        public async Task<Playlist> SearchForPlaylist(string playlistName)
        {
            foreach (Playlist p in await GetPlaylists())
            {
                if (p.name.Contains(playlistName)) {
                  return p;
                }
            }
            return null;

        }

        public async Task<bool> PlaylistIsEmpty(string userId, string playlistId)
        {
            return (await GetPlaylistTracks(userId, playlistId)).Count > 0;
        }

        public async Task ClearPlaylist(string userId, string playlistId)
        {
            List<string> songsToKill = new List<string>();
            List<PlaylistTrack> tracks = await GetPlaylistTracks(userId, playlistId);
            foreach (PlaylistTrack track in tracks)
            {
                songsToKill.Add(track.track.name);
            }
            await ClearPlaylistTracks(songsToKill, userId, playlistId);
        }

        public async Task<List<Playlist>> GetPlaylists() {
            HttpResponseMessage response = await spotifyClient.GetAsync("v1/me/playlists");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GetPlaylistsResponse>(await response.Content.ReadAsStringAsync()).items;
        }
        public async Task<Playlist> GetPlaylist(string userId, string playlistId) {
            HttpResponseMessage response = await spotifyClient.GetAsync("v1/users/"+ userId + "/playlists/" + playlistId);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Playlist>(await response.Content.ReadAsStringAsync());
        }
        public async Task<List<PlaylistTrack>> GetPlaylistTracks(string userId, string playlistId) {
              HttpResponseMessage response = await spotifyClient.GetAsync("v1/users/"+ userId + "/playlists/" + playlistId + "/tracks");
              return Newtonsoft.Json.JsonConvert.DeserializeObject<PlaylistTrackResponse>(await response.Content.ReadAsStringAsync()).items;
        }
        public async Task ClearPlaylistTracks(List<string> trackIds, string userId, string playlistId) {
            List<TrackToDelete> tracksToDelete = new List<TrackToDelete>();
            foreach (string s in trackIds)
            {
                tracksToDelete.Add(new TrackToDelete()
                {
                    uri = "spotify:track:" + s,
                });
            }
            DeletePlaylistTrackRequest trackRequest = new DeletePlaylistTrackRequest(){
                tracks = tracksToDelete
            };
            var requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(trackRequest);
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                Content = new StringContent(requestBody, Encoding.UTF8),
                RequestUri = new Uri("v1/users/" + userId + "/playlists/" + playlistId + "/tracks"),
            };
            HttpResponseMessage response = await spotifyClient.SendAsync(request);

        }
        public async Task<Playlist> CreatePlaylist(string playlistName, string userId) {
            // TODO implement
            //HttpResponseMessage response = await spotifyClient.PostAsync("v1/users/" + userId + "/playlists", content);
            return new Playlist();
        }

        /* Passenger methods */

        public async Task<List<Song>> SearchForSong(string searchTerms)
        {
            HttpResponseMessage response = await spotifyClient.GetAsync(searchTerms.Replace(" ", "%20"));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Tracks>(await response.Content.ReadAsStringAsync()).items;
        }
    }
}
