using System.IO;
using Android.Graphics;
using RydeTunes.Droid;
using Xamarin.Forms;
using ZXing.Common;
using ZXing.Mobile;

[assembly: Dependency(typeof(QrCodeImageGenerator))]
namespace RydeTunes.Droid
{
    public class QrCodeImageGenerator : IQrCodeImageGenerator
    {
        public ImageSource GetImageSource(string text)
        {
            var writer = new BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new EncodingOptions { Height = 1000, Width = 1000 }
            };
            var image = writer.Write(text);

            var imgsrc = ImageSource.FromStream(() =>
            {
                MemoryStream ms = new MemoryStream();
                image.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                ms.Seek(0L, SeekOrigin.Begin);
                return ms;
            });

            return imgsrc;
        }
    }
}
