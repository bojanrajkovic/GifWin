using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GifWin
{
    internal sealed class MainWindowViewModel
        : ViewModelBase
    {
        public MainWindowViewModel()
        {
            GifWitLibrary.LoadFromFileAsync ("library.gifwit").ContinueWith (t => {
                Images = new CollectionView (t.Result.Select (e => new GifEntryViewModel (e))) {
                    Filter = FilterPredicate
                };
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public ICollectionView Images
        {
            get { return this.images; }
            private set
            {
                if (this.images == value)
                    return;

                this.images = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
                
                this.filterKeywords.Clear();
                this.filterKeywords.UnionWith (value.Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                if (this.images != null)
                    this.images.Refresh();
            }
        }

        private string filterText;
        private readonly HashSet<string> filterKeywords = new HashSet<string>();
        private ICollectionView images;

        private bool FilterPredicate (object o)
        {
            var entry = (GifEntryViewModel) o;
            if (String.IsNullOrWhiteSpace (FilterText))
                return true;

            return this.filterKeywords.IsSubsetOf (entry.Keywords);
        }
    }
}