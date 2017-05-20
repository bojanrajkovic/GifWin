using GifWin.Core.Models;

namespace GifWin.Core.Services
{
    public interface IClipboardService
    {
        void PutTextOnClipboard(string uri);
        void PutImageOnClipboard(GifEntry entry);
    }
}
