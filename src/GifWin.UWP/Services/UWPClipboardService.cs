using System;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;

using Microsoft.Extensions.Logging;

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

                             try {
                                 await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                                     Clipboard.SetContent(pack);
                                 });
                             } catch (Exception e) {
                                 ServiceContainer.Instance.GetLogger<UWPClipboardService>()
                                                ?.LogWarning(new EventId(), e, "Could not place image on clipboard.");
                             }
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
