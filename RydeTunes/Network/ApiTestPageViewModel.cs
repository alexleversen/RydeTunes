using System.ComponentModel;
using System.Windows.Input;
using Android.OS;
using Android.Print;
using Xamarin.Forms;

namespace RydeTunes.Network
{
    class ApiTestPageViewModel : INotifyPropertyChanged
    {
        private string _playlistId;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ShouldShowEmptyPlaylistText { get; private set; }
        public string EmptyPlaylistText { get; private set; }

        public ICommand GetRTPlaylistCommand => new Command(GetRTPlaylist);
        public ICommand SearchForPlaylistCommand => new Command(SearchForPlaylist);
        public ICommand CreatePlaylistCommand => new Command(CreatePlaylist);
        public ICommand CheckPlaylistEmptyCommand => new Command(CheckPlaylistEmpty);
        public ICommand ClearPlaylistCommand => new Command(ClearPlaylist);
        public ICommand AddSongcommand => new Command(AddSong);

        private async void ClearPlaylist()
        {
            if (!string.IsNullOrEmpty(_playlistId))
            {
                await SpotifyApi.Instance.ClearPlaylist(_playlistId);
            }
        }

        private async void CheckPlaylistEmpty()
        {
            if (!string.IsNullOrEmpty(_playlistId))
            {
                var playlistIsEmpty = await SpotifyApi.Instance.PlaylistIsEmpty(_playlistId);
                ShouldShowEmptyPlaylistText = true;
                EmptyPlaylistText = playlistIsEmpty ? "RT Playlist is empty!" : "RT Playlist isn't empty";
            }
        }

        private async void CreatePlaylist()
        {
            var playlist = await SpotifyApi.Instance.CreatePlaylist("Boohoohaha");
            EmptyPlaylistText = "playlist created";
        }

        private async void SearchForPlaylist()
        {
            var playlist = await SpotifyApi.Instance.SearchForMyPlaylist("Boohoohaha");
            EmptyPlaylistText = playlist == null ? "Not found playlist" : ("Playlist found: " + playlist.id);
        }

        private async void GetRTPlaylist()
        {
            var playlist = await SpotifyApi.Instance.GetRydeTunesPlaylist();
            _playlistId = playlist.id;
        }

        private async void AddSong()
        {
            string userId = "1tepoxl0zxhepvp0915g4r3av";
            string playlistId = "26RuME3IIjyAzwWFJCl5rS";
            string songId = "6YTh4DW8dg0TnHUFuxJ10Y";
            var playlist = await SpotifyApi.Instance.GetPlaylist(userId, playlistId);
            EmptyPlaylistText = playlist.name;
            await SpotifyApi.Instance.AddSongToPlaylist(songId, userId, playlistId);
            
            EmptyPlaylistText = "Song added to playlist";
        }
    }
}
