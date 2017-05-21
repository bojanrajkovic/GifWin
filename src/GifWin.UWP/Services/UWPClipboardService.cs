using System;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;

using Microsoft.Extensions.Logging;

using GifWin.Core;
using GifWin.Core.Models;
using GifWin.Core.Services;

namespace GifWin.UWP
{
    class UWPClipboardService : IClipboardService
    {
        public void PutImageOnClipboard(string imagePath)
        {
            var pack = new DataPackage();
            var storageFile = StorageFile.GetFileFromPathAsync(imagePath)
                                         .AsTask()
                                         .ConfigureAwait(false)
                                         .GetAwaiter()
                                         .GetResult();
            pack.SetBitmap(RandomAccessStreamReference.CreateFromFile(storageFile));

            Clipboard.SetContent(pack);
        }

        public void PutTextOnClipboard(string text)
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }
    }
}
