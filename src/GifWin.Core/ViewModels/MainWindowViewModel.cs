using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GifWin.Core.Data;

namespace GifWin.Core.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        GifWinDatabase db;

        string newEntryTags;
        string filterText;
        string imageSource;

        IReadOnlyList<string> tags;
        IReadOnlyList<GifEntryViewModel> images;

        readonly HashSet<string> filterKeywords = new HashSet<string> {
            "*"
        };

        public MainWindowViewModel()
        {
            db = new GifWinDatabase("GifWin.sqlite");
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

        public string ImageSource {
            get { return imageSource; }
            set {
                if (imageSource == value)
                    return;

                var uri = new Uri(value, UriKind.Absolute);
                imageSource = value;
                RaisePropertyChanged();
            }
        }

        public string FilterText {
            get { return filterText; }
            set {
                if (filterText == value)
                    return;

                filterText = value;

                filterKeywords.Clear();
                filterKeywords.UnionWith(value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                if (Uri.IsWellFormedUriString(filterText, UriKind.Absolute))
                    ImageSource = value;

                RefreshImageCollection();
                RaisePropertyChanged();
            }
        }

        public string NewEntryTags {
            get { return newEntryTags; }
            set {
                newEntryTags = value;
                RaisePropertyChanged();
            }
        }
    }
}