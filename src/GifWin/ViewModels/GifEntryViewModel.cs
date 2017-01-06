using GifWin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace GifWin
{
    class GifEntryViewModel : ViewModelBase
    {
        Task<string> cachedUri;

        public GifEntryViewModel (GifEntry entry)
        {
            if (entry == null) {
                throw new ArgumentNullException (nameof (entry));
            }

            Id = entry.Id;
            Url = entry.Url;
            FirstFrame = entry.FirstFrame;
            Keywords = entry.Tags.Select (t => t.Tag).ToArray ();
        }

        public byte[] FirstFrame { get; }
        public int Id { get; }
        public string Url { get; }

        public Uri CachedUri
        {
            get
            {
                if (cachedUri == null) {
                    cachedUri = GifHelper.GetOrMakeSavedAsync (Id, Url, FirstFrame);
                    cachedUri.ContinueWith (t => {
                        RaisePropertyChanged ();
                    }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext ());

                    return null;
                }

                if (cachedUri.Status == TaskStatus.Running)
                    return null;

                return new Uri (cachedUri.Result);
            }
        }

        public string KeywordString => string.Join (" ", Keywords);

        public IEnumerable<string> Keywords { get; }
    }
}