using System;
using RydeTunes.iOS;
using Xamarin.Forms;
using ZXing.Mobile;

[assembly:Dependency(typeof(QrCodeImageGenerator))]
namespace RydeTunes.iOS
{
    public class QrCodeImageGenerator : IQrCodeImageGenerator
    {
        public ImageSource GetImageSource(string text)
        {
            var writer = new BarcodeWriter{
                Format = ZXing.BarcodeFormat.QR_CODE
            };
            var image = writer.Write(text);
            return ImageSource.FromStream(() => image.AsPNG().AsStream());
        }
    }
}
