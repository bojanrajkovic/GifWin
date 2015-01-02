using System;
using System.Threading;
using System.Threading.Tasks;

namespace GifWin
{
    internal class GifEntryViewModel
        : ViewModelBase
    {
        public GifEntryViewModel (GifWitLibraryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException ("entry");

            this.entry = entry;
            
        }

        public Uri Url
        {
            get { return this.entry.Url; }
        }

        public Uri CachedUri
        {
            get
            {
                if (this.cachedUri == null) {
                    this.cachedUri = GifWitLibrary.GetOrMakeSavedAsync (this.entry);
                    this.cachedUri.ContinueWith (t => {
                        OnPropertyChanged ();
                    }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

                    return null;
                }

                if (this.cachedUri.Status == TaskStatus.Running)
                    return null;

                return new Uri (this.cachedUri.Result);
            }
        }

        public string KeywordString
        {
            get { return this.entry.KeywordString; }
        }

        public string[] Keywords
        {
            get
            {
                if (this.keywords == null)
                    this.keywords = this.entry.KeywordString.Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return this.keywords;
            }

            set
            {
                if (this.keywords == value)
                    return;

                this.keywords = value;
                this.entry.KeywordString = String.Join (" ", value);
                OnPropertyChanged();
            }
        }

        private Task<string> cachedUri;
        private string[] keywords;
        private readonly GifWitLibraryEntry entry;
    }
}