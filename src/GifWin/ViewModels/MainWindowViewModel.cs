using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using GalaSoft.MvvmLight;

using GifWin.Properties;
using GifWin.Data;
using GifWin.Utility;

namespace GifWin
{
    sealed class MainWindowViewModel : ViewModelBase
    {
        GifWinDatabaseHelper helper;
        string newEntryTags;
        string filterText;
        string imageSource;
        readonly HashSet<string> filterKeywords = new HashSet<string> ();
        ICollectionView images;

        public MainWindowViewModel ()
        {
            helper = new GifWinDatabaseHelper ();
            RefreshImageCollection ();
        }

        void RefreshImageCollection ()
        {
            Task<IEnumerable<GifEntry>> gifs;

            var filterArray = filterKeywords.ToArray ();
            if (filterArray.Length == 1 && filterArray[0] == "*")
                gifs = Task.FromResult(helper.QueryGifs (e => true, e => e).AsEnumerable ());
            else
                gifs = helper.GetGifsbyTagAsync (filterArray);

            gifs.ContinueWith (t => {
                var filterResults = t.Result.Select (ge => new GifEntryViewModel (ge));
                Images = CollectionViewSource.GetDefaultView (filterResults);
            });
        }

        public double Zoom
        {
            get { return Settings.Default.Zoom; }
            set
            {
                Settings.Default.Zoom = value;
                RaisePropertyChanged ();
            }
        }

        public ICollectionView Images
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

                if (uri.Scheme == Uri.UriSchemeHttps)
                    finalValue = ImageForwardingListener.Instance.BuildForwardingUrl (value);

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
    }
}