using System;
using RydeTunes.Network;

namespace RydeTunes
{
    class DriverLoginViewModel
    {
        public event EventHandler ReadyToNavigateToSuccess;

        public string LoginUrl =>
            @"https://accounts.spotify.com/authorize?client_id=37f783152b7d4be696ab2b901b73f99c&redirect_uri=http://RydeTunes.com/Success&scope=playlist-modify-public playlist-read-collaborative user-library-read playlist-read-private&response_type=token";

        public void HandleUrlNavigation(string url)
        {
            if (url.StartsWith("http://rydetunes.com/Success"))
            {
                //TODO: Extract validity duration and compare to UTC time?
                var tokenStartIndex = url.IndexOf('=');
                var tokenEndIndex = url.IndexOf('&');
                SpotifyApi.Instance.UpdateToken(url.Substring(tokenStartIndex + 1, tokenEndIndex - tokenStartIndex - 1));

                ReadyToNavigateToSuccess?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
