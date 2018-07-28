using System;
using System.Threading.Tasks;
using RydeTunes.iOS;
using Xamarin.Forms;
using ZXing.Mobile;

[assembly:Dependency (typeof(QRCodeScanner))]
namespace RydeTunes.iOS
{
    public class QRCodeScanner : IQRCodeScanner
    {
        public QRCodeScanner()
        {
        }

        public async Task<string> ScanAsync() {
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan(new MobileBarcodeScanningOptions());
            return result?.Text ?? "";
        }
    }
}
