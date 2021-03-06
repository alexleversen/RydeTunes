﻿using System;
using System.Windows.Input;
using RydeTunes.Network;
using Xamarin.Forms;

namespace RydeTunes
{
    public class MainViewModel
    {
        private string _qrCodeText;

        public ICommand RiderTapped => new Command(ScanQRCode);

        public event EventHandler ReadyToNavigate;

        async void ScanQRCode()
        {
            var scanner = DependencyService.Get<IQRCodeScanner>();
            _qrCodeText = await scanner.ScanAsync();
            var ids = _qrCodeText?.Split(':');
            if(ids == null || ids.Length != 3)
            {
                return;
            }
            SpotifyApi.Instance.ActivePlaylistId = ids[0];
            SpotifyApi.Instance.UserId = ids[1];
            await SpotifyApi.Instance.UpdateToken(ids[2]);
            ReadyToNavigate?.Invoke(this,EventArgs.Empty);
        }
    }
}
