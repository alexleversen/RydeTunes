using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using RydeTunes.Network.DTO;
using System.Net;

namespace RydeTunes.Network
{
    class SpotifyApi
    {
        public static SpotifyApi Instance;
        private static string SPOTIFY_API_URL = "https://api.spotify.com";
        public static readonly string RYDETUNES_PLAYLIST_NAME = "RydeTunes Collaborative Playlist";

        private HttpClient spotifyClient;
        private string userId;

        public string UserId
        {
            get => userId;
            private set => userId = value;
        }

        public SpotifyApi()
        {
            Instance = this;
        }

        public async Task UpdateToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                throw new Exception("This Value can't be empty");
            }
            spotifyClient = new HttpClient();

            spotifyClient.BaseAddress = new Uri(SPOTIFY_API_URL);
            spotifyClient.DefaultRequestHeaders.Accept.Clear();
            spotifyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            HttpResponseMessage response = await spotifyClient.GetAsync("v1/me");
            userId = (await NetworkCallWrapper.ParseResponse<GetMeResponse>(response, HttpStatusCode.OK)).id;
           
        }
        public async Task<bool> IsTokenValid()
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }
            try
            {
                HttpResponseMessage response = await spotifyClient.GetAsync("v1/me");
                await NetworkCallWrapper.ParseResponse<GetMeResponse>(response, HttpStatusCode.OK);
                return true;
            } catch (Exception e)
            {
                // It was not an ok
                return false;
            }
        }
        

        /* Driver methods */
        // Gets the playlist if it exists on the provided users account
        // Otherwise, create the playlist 
        public async Task<Playlist> GetRydeTunesPlaylist()
        {
            // Get playlist if it exists
            Playlist p = await SearchForMyPlaylist(RYDETUNES_PLAYLIST_NAME);
            if (p == null)
            {
                return await CreatePlaylist(RYDETUNES_PLAYLIST_NAME);
            }
            else
            {
                await ClearPlaylist(p.id);
                return p;
            }
        }

        // Searches for playlists of the current user (determined by token) with the given name
        // Returns null if playlist was not found
        public async Task<Playlist> SearchForMyPlaylist(string playlistName)
        {
            foreach (Playlist p in await GetMyPlaylists())
            {
                if (p.name.Contains(playlistName)) {
                  return p;
                }
            }
            return null;
        }

        public async Task<bool> PlaylistIsEmpty(string playlistId)
        {
            return (await GetPlaylistTracks(playlistId)).Count <= 0;
        }

        // Remove every song in the playlist
        public async Task ClearPlaylist(string playlistId)
        {
            List<string> songsToKill = new List<string>();
            List<PlaylistTrack> tracks = await GetPlaylistTracks(playlistId);
            foreach (PlaylistTrack track in tracks)
            {
                songsToKill.Add(track.track.name);
            }
            await ClearPlaylistTracks(songsToKill, playlistId);
        }

        public async Task<List<Playlist>> GetMyPlaylists() {
            HttpResponseMessage response = await spotifyClient.GetAsync("v1/me/playlists");
            return (await NetworkCallWrapper.ParseResponse<GetPlaylistsResponse>(response, HttpStatusCode.OK)).items;
        }
        public async Task<Playlist> GetPlaylist( string playlistId) {
            HttpResponseMessage response = await spotifyClient.GetAsync("v1/users/"+ userId + "/playlists/" + playlistId);
            return await NetworkCallWrapper.ParseResponse<Playlist>(response, HttpStatusCode.OK);
        }
        public async Task<List<PlaylistTrack>> GetPlaylistTracks( string playlistId) {
              HttpResponseMessage response = await spotifyClient.GetAsync("v1/users/"+ userId + "/playlists/" + playlistId + "/tracks");
              return (await NetworkCallWrapper.ParseResponse<PlaylistTrackResponse>(response, HttpStatusCode.OK)).items;
        }
        public async Task ClearPlaylistTracks(List<string> trackIds, string playlistId) {
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
                RequestUri = new Uri(spotifyClient.BaseAddress.OriginalString + "v1/users/" + userId + "/playlists/" + playlistId + "/tracks"),
            };
            HttpResponseMessage response = await spotifyClient.SendAsync(request);

        }
        public async Task<Playlist> CreatePlaylist(string playlistName) {
            var createPlaylistRequest = new CreatePlaylistRequest(){
                name = playlistName,
                @public = true,
            };   
            var requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(createPlaylistRequest);
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = new StringContent(requestBody, Encoding.UTF8),
                RequestUri = new Uri(spotifyClient.BaseAddress.OriginalString + "v1/users/" + userId + "/playlists"),
            };
            HttpResponseMessage response = await spotifyClient.SendAsync(request);
            return await NetworkCallWrapper.ParseResponse<Playlist>(response, HttpStatusCode.Created);
        }

        /* Passenger methods */

        public async Task<List<Song>> GetSongs(string searchTerms)
        {
            HttpResponseMessage response = await spotifyClient.GetAsync(searchTerms.Replace(" ", "%20"));
            return (await NetworkCallWrapper.ParseResponse<Tracks>(response, HttpStatusCode.OK)).items;
        }

        public async Task<List<Song>> SearchForSong(string searchTerms)
        {
            List<Song> results = null;

            try
            {
                results = await GetSongs(searchTerms);

                results.Sort(delegate (Song x, Song y)
                {
                    return x.popularity.CompareTo(y.popularity);
                });
            }
            catch (Exception e)
            {
                //deal with issue
                System.Diagnostics.Debug.WriteLine("Api call failed: {0}", e.Message);
            }

            return results;
        }
        


        public async Task AddSongToPlaylist(string songId, string ownerUserId, string playlistId)
        {
            var arguments = "?uris=spotify:track:" + songId;
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(spotifyClient.BaseAddress.OriginalString + "v1/users/" + ownerUserId + "/playlists/" + playlistId + "/tracks" + arguments),
            };
            HttpResponseMessage response = await spotifyClient.SendAsync(request);
        }
    }
}
