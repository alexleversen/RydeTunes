using System;
using RydeTunes.Network;

namespace RydeTunes
{
    class DriverLoginViewModel
    {
        public event EventHandler ReadyToNavigateToSuccess;

        public string LoginUrl =>
            @"https://accounts.spotify.com/authorize?client_id=37f783152b7d4be696ab2b901b73f99c&redirect_uri=http:%2F%2FRydeTunes.com%2FSuccess&scope=playlist-modify-public%20playlist-read-collaborative%20user-library-read%20playlist-read-private&response_type=token";

        public void HandleUrlNavigation(string url)
        {
            if (url.StartsWith("http://rydetunes.com/Success"))
            {
                var tokenStartIndex = url.IndexOf('=');
                var tokenEndIndex = url.IndexOf('&');
                SpotifyApi.Instance.AuthToken = url.Substring(tokenStartIndex + 1, tokenEndIndex - tokenStartIndex - 1);

                ReadyToNavigateToSuccess?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
