using JetBrains.Annotations;

namespace GifWin.Core.Services
{
    [PublicAPI]
    public interface IClipboardService
    {
        void PutTextOnClipboard(string uri);
        void PutImageOnClipboard(string imagePath);
    }
}
