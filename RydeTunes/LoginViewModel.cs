using System;
using RydeTunes.Network;

namespace RydeTunes
{
    class LoginViewModel
    {
        public event EventHandler ReadyToNavigateToSuccess;

        public string LoginUrl =>
            @"https://accounts.spotify.com/authorize?client_id=37f783152b7d4be696ab2b901b73f99c&redirect_uri=http://RydeTunes.com/Success&scope=playlist-modify-public playlist-read-collaborative user-library-read playlist-read-private&response_type=token";

        private bool _gotSuccess = false;

        public async void HandleUrlNavigation(string url)
        {
            if (url.StartsWith("http://rydetunes.com/Success") && !_gotSuccess)
            {
                _gotSuccess = true;
                //TODO: Extract validity duration and compare to UTC time?
                var tokenStartIndex = url.IndexOf('=');
                var tokenEndIndex = url.IndexOf('&');
                await SpotifyApi.Instance.UpdateToken(url.Substring(tokenStartIndex + 1, tokenEndIndex - tokenStartIndex - 1));

                ReadyToNavigateToSuccess?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
