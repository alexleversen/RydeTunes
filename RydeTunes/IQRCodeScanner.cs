using System;
using System.Threading.Tasks;

namespace RydeTunes
{
    public interface IQRCodeScanner
    {
        Task<string> ScanAsync();
    }
}
