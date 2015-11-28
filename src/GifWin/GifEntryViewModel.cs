using GifWin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GifWin
{
    class GifEntryViewModel : ViewModelBase
    {
        private Task<string> cachedUri;

        public GifEntryViewModel (GifEntry entry)
        {
            if (entry == null) {
                throw new ArgumentNullException (nameof (entry));
            }

            Id = entry.Id;
            Url = entry.Url;
            Keywords = entry.Tags.Select (t => t.Tag).ToArray ();
        }

        public int Id { get; }
        public string Url { get; }

        public Uri CachedUri
        {
            get
            {
                if (this.cachedUri == null) {
                    this.cachedUri = GifHelper.GetOrMakeSavedAsync (Id, Url);
                    this.cachedUri.ContinueWith (t => {
                        OnPropertyChanged ();
                    }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext ());

                    return null;
                }

                if (this.cachedUri.Status == TaskStatus.Running)
                    return null;

                return new Uri (this.cachedUri.Result);
            }
        }

        public string KeywordString => string.Join (" ", Keywords);

        public IEnumerable<string> Keywords { get; }
    }
}