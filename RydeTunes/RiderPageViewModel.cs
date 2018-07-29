using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using RydeTunes.Network;
using RydeTunes.Network.DTO;
using Xamarin.Forms;

namespace RydeTunes
{
    public class RiderPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Song> SongList { get; private set; }

        public string SearchText { get; set; }

        public ICommand CommitSearchCommand => new Command(CommitSearch);

        private async void CommitSearch()
        {
            var songs = await SpotifyApi.Instance.SearchForSong(SearchText);
            SongList = new ObservableCollection<Song>();

            if (songs != null)
            {
                foreach (var song in songs)
                {
                    SongList.Add(song);
                }
            }
        }
    }
}
