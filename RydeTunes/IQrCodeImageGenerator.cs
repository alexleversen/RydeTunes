using System;
using Xamarin.Forms;

namespace RydeTunes
{
    public interface IQrCodeImageGenerator
    {
        ImageSource GetImageSource(string text);
    }
}
