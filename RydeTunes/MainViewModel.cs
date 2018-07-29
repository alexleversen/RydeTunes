using System;
using System.Linq;
using System.Windows.Input;
using RydeTunes.Network;
using Xamarin.Forms;

namespace RydeTunes
{
    public class MainViewModel
    {
        public ICommand RiderTapped => new Command(ScanQRCode);

        public event EventHandler ReadyToNavigate;

        private string _qrCodeText;

        async void ScanQRCode()
        {
            var scanner = DependencyService.Get<IQRCodeScanner>();
            _qrCodeText = await scanner.ScanAsync();
            var ids = _qrCodeText?.Split(':');
            if(ids == null || !ids.Any())
            {
                return;
            }
            SpotifyApi.Instance.ActivePlaylistId = ids[0];
            SpotifyApi.Instance.UserId = ids[1];
            SpotifyApi.Instance.Token = ids[2];
            ReadyToNavigate?.Invoke(this,EventArgs.Empty);
        }
    }
}
