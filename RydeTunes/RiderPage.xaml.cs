using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;

namespace RydeTunes
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RiderPage : ContentPage
	{
        private string _qrCodeText;

		public RiderPage ()
		{
			InitializeComponent ();
		}

        async void ScanQRCode(object sender, System.EventArgs e) {
            var scanner = new MobileBarcodeScanner()
            {
                TopText = "Test1",
                BottomText = "Test2"
            };
            var scanResult = await scanner.Scan(new MobileBarcodeScanningOptions());
            _qrCodeText = scanResult.Text;
            qrCodeButton.Text = _qrCodeText;
        }
	}
}