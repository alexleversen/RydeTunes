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
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions{
                    Height = 1000,
                    Width = 1000
                }
            };
            var image = writer.Write(text);
            return ImageSource.FromStream(() => image.AsPNG().AsStream());
        }
    }
}
