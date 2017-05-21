using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GifWin.Core.Data;
using GifWin.Core.Services;

namespace GifWin.Core.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        GifWinDatabase db;

        IReadOnlyList<string> tags;
        IReadOnlyList<GifEntryViewModel> images;

        public MainWindowViewModel()
        {
            db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            RefreshImageCollection();
        }

        void RefreshImageCollection()
        {
            db.GetAllTagsAsync().ContinueWith(t => {
                tags = t.Result.Select(tag => tag.Tag).ToArray();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            db.GetAllGifsAsync().ContinueWith(t => {
                images = t.Result.Select(ge => new GifEntryViewModel(ge)).ToArray();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public IReadOnlyList<string> Tags {
            get { return tags; }
            private set {
                if (tags == value)
                    return;

                tags = value;
                RaisePropertyChanged();
            }
        }

        public IReadOnlyList<GifEntryViewModel> Images {
            get { return images; }
            private set {
                if (images == value)
                    return;

                images = value;
                RaisePropertyChanged();
            }
        }
    }
}