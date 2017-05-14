using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using GifWin.Data;

namespace GifWin.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel ()
        {
            helper = new GifWinDatabaseHelper ();
            RefreshImageCollection ();
        }

        public IReadOnlyList<GifEntryViewModel> Images
        {
            get { return images; }
            private set
            {
                if (images == value)
                    return;

                images = value;
                RaisePropertyChanged ();
            }
        }

        public string ImageSource
        {
            get { return imageSource; }
            set
            {
                if (imageSource == value)
                    return;

                var uri = new Uri (value, UriKind.Absolute);
                var finalValue = value;

                imageSource = finalValue;

                RaisePropertyChanged ();
            }
        }

        public string FilterText
        {
            get { return filterText; }
            set
            {
                if (filterText == value)
                    return;

                filterText = value;

                filterKeywords.Clear ();
                filterKeywords.UnionWith (value.Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                if (Uri.IsWellFormedUriString (filterText, UriKind.Absolute))
                    ImageSource = value;

                RefreshImageCollection ();
                RaisePropertyChanged ();
            }
        }

        public string NewEntryTags
        {
            get { return newEntryTags; }
            set
            {
                newEntryTags = value;
                RaisePropertyChanged ();
            }
        }

        GifWinDatabaseHelper helper;
        string newEntryTags;
        string filterText;
        string imageSource;
        IReadOnlyList<GifEntryViewModel> images;
        readonly HashSet<string> filterKeywords = new HashSet<string> ();

        private void RefreshImageCollection ()
        {
            Task<IEnumerable<GifEntry>> gifs;

            var filterArray = filterKeywords.ToArray ();
            if (filterArray.Length == 1 && filterArray[0] == "*")
                gifs = Task.FromResult (helper.QueryGifs (e => true, e => e).AsEnumerable ());
            else
                gifs = helper.GetGifsbyTagAsync (filterArray);

            gifs.ContinueWith (t => {
                var filterResults = t.Result.Select (ge => new GifEntryViewModel (ge));
                Images = filterResults.ToArray();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}