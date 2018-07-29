using System.IO;
using Android.Graphics;
using RydeTunes.Droid;
using Xamarin.Forms;
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
                Format = ZXing.BarcodeFormat.QR_CODE
            };
            var image = writer.Write(text);
            using(var stream = new MemoryStream())
            {
                image.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return ImageSource.FromStream(() => stream);
            }

        }
    }
}
