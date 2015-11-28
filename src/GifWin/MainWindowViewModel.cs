using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using GifWin.Properties;
using GifWin.Data;

namespace GifWin
{
    internal sealed class MainWindowViewModel
        : ViewModelBase
    {
        GifWinDatabaseHelper helper;
        string newEntryTags;
        private string filterText;
        private readonly HashSet<string> filterKeywords = new HashSet<string> ();
        private ICollectionView images;

        public MainWindowViewModel ()
        {
            helper = new GifWinDatabaseHelper ();
            RefreshImageCollection ();
        }

        void RefreshImageCollection ()
        {
            helper.GetGifsbyTagAsync (filterKeywords.ToArray ()).ContinueWith (t => {
                var filterResults = t.Result.Select(ge => new GifEntryViewModel(ge));
                Images = CollectionViewSource.GetDefaultView (filterResults);
            });
        }

        public double Zoom
        {
            get { return Settings.Default.Zoom; }
            set
            {
                Settings.Default.Zoom = value;
                OnPropertyChanged ();
            }
        }

        public ICollectionView Images
        {
            get { return this.images; }
            private set
            {
                if (this.images == value)
                    return;

                this.images = value;
                OnPropertyChanged ();
            }
        }

        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (this.filterText == value)
                    return;

                this.filterText = value;

                this.filterKeywords.Clear ();
                this.filterKeywords.UnionWith (value.Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                RefreshImageCollection ();
                OnPropertyChanged ();
            }
        }

        internal void RefreshImagesFromDatabase ()
        {
            RefreshImageCollection ();
        }

        public string NewEntryTags
        {
            get { return newEntryTags; }
            set
            {
                newEntryTags = value;
                OnPropertyChanged ();
            }
        }
    }
}