using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

using GifWin.Core.Models;
using GifWin.Core.Services;

namespace GifWin.UWP
{
    class UWPClipboardService : IClipboardService
    {
        public void PutImageOnClipboard(GifEntry entry)
        {
            GifHelper.GetOrMakeSavedAsync(entry, entry.FirstFrame)
                     .ContinueWith(async t => {
                         var path = await t;
                         if (path != null) {
                             var pack = new DataPackage();
                             var storageFile = await StorageFile.GetFileFromPathAsync(path);
                             pack.SetBitmap(RandomAccessStreamReference.CreateFromFile(storageFile));
                             Clipboard.SetContent(pack);
                         }
                     });
        }

        public void PutTextOnClipboard(string text)
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }
    }
}
