using System.ComponentModel;
using System.Windows.Input;
using RydeTunes.Network;
using Xamarin.Forms;

namespace RydeTunes
{
    class DriverDashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource QrCodeImage { get; private set; }

        public bool InstructionsVisible { get; set; }
        public bool QrCodeVisible { get; set; }
        public bool IsRideStarted { get; set; }

        public ICommand StartRideCommand => new Command(_ => StartRide());
        public ICommand EndRideCommand => new Command(_ => EndRide());

        public DriverDashboardViewModel()
        {
            InstructionsVisible = true;
            QrCodeVisible = false;
        }

        private async void StartRide()
        {
            InstructionsVisible = false;
            QrCodeVisible = true;
            var playlist = await SpotifyApi.Instance.GetRydeTunesPlaylist();
            var userId = SpotifyApi.Instance.UserId;
            var authToken = SpotifyApi.Instance.Token;
            QrCodeImage = DependencyService.Get<IQrCodeImageGenerator>().GetImageSource(playlist.id+":"+userId+":"+authToken);
            IsRideStarted = true;
        }

        private async void EndRide()
        {
            await SpotifyApi.Instance.ClearPlaylist(SpotifyApi.Instance.ActivePlaylistId);
            SpotifyApi.Instance.ActivePlaylistId = null;
            IsRideStarted = false;
        }
    }
}
