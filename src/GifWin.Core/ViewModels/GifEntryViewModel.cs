using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GifWin.Core.Commands;
using GifWin.Core.Models;
using GifWin.Core.Services;

namespace GifWin.Core.ViewModels
{
    public sealed class GifEntryViewModel : ViewModelBase
    {
        readonly Task<string> cachedUri;
        string url, searchTerm;

        public event EventHandler EntryDeleted;

        public GifEntryViewModel (GifEntry entry, string searchTerm = null)
        {
            if (entry == null)
                throw new ArgumentNullException (nameof (entry));

            this.searchTerm = searchTerm;

            Id = entry.Id;

            OriginalUrl = entry.Url;

            if (GifHelper.TryGetCachedPathIfExists(entry, out var cachedPath))
                Url = cachedPath;
            else {
                cachedUri = GifHelper.GetOrMakeSavedAsync(entry, entry.FirstFrame);
                cachedUri.ContinueWith(
                    t => {
                        if (t != null) {
                            var mt = ServiceContainer.Instance.GetRequiredService<IMainThread>();
                            mt.RunAsync(() => Url = t.Result);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion
                );
            }

            FirstFrame = entry.FirstFrame;
            Keywords = entry.Tags.Select (t => t.Tag).ToArray ();
        }

        internal void RaiseDeleted() =>
            EntryDeleted?.Invoke(this, null);

        public ICommand CopyImageUrlCommand => new CopyImageUrlCommand(searchTerm);
        public ICommand CopyImageCommand => new CopyImageCommand(searchTerm);
        public ICommand DeleteImageCommand => new DeleteImageCommand();

        public byte[] FirstFrame { get; }
        public int Id { get; }

        public string Url {
            get => url;
            private set {
                if (url != value) {
                    url = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string OriginalUrl { get; }
        public IEnumerable<string> Keywords { get; }
    }
}