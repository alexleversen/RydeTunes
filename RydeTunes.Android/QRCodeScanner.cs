using System;
using System.Threading.Tasks;
using RydeTunes.Droid;
using Xamarin.Forms;
using ZXing.Mobile;

[assembly:Dependency(typeof(QRCodeScanner))]
namespace RydeTunes.Droid
{
    public class QRCodeScanner : IQRCodeScanner
    {

        public async Task<string> ScanAsync()
        {
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan(Android.App.Application.Context, new MobileBarcodeScanningOptions());
            return result?.Text ?? "";
        }
    }
}
