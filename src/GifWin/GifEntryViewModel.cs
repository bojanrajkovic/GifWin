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
                throw new ArgumentNullException(nameof(entry));
            }

            Id = entry.Id;
            Url = entry.Url;
            Keywords = entry.Tags.Select(t => t.Tag).ToArray();
            FirstFrameData = entry.FirstFrame;
        }

        public int Id { get; set; }
        public string Url { get; }

        public Uri CachedUri
        {
            get
            {
                if (this.cachedUri == null) {
                    this.cachedUri = GifHelper.GetOrMakeSavedAsync (Url);
                    this.cachedUri.ContinueWith (t => {
                        OnPropertyChanged ();

                        if (FirstFrameData == null) {
                            Task.Run(async () => {
                                using (var helper = new GifWinDatabaseHelper()) {
                                    var frameData = GifHelper.GetFrameData(t.Result, frameNumber: 0);
                                    await helper.UpdateSavedFirstFrameDataAsync(Id, frameData);
                                }
                            });
                        }
                    }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

                    return null;
                }

                if (this.cachedUri.Status == TaskStatus.Running)
                    return null;

                return new Uri (this.cachedUri.Result);
            }
        }

        public string KeywordString => string.Join(" ", Keywords);

        public IEnumerable<string> Keywords { get; }
        public byte[] FirstFrameData { get; }
    }
}