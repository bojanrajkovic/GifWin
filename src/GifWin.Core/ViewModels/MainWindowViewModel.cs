using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using GifWin.Core.Data;
using GifWin.Core.Services;

namespace GifWin.Core.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        GifWinDatabase db;

        ObservableCollection<string> tags;
        ObservableCollection<GifEntryViewModel> images;

        public MainWindowViewModel()
        {
            db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            RefreshImageCollection();
        }

        void RefreshImageCollection()
        {
            db.GetAllTagsAsync().ContinueWith(t => {
                tags = new ObservableCollection<string>(t.Result.Select(tag => tag.Tag));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            db.GetAllGifsAsync().ContinueWith(t => {
                images = new ObservableCollection<GifEntryViewModel>(
                    t.Result.Select(ge => {
                        var model = new GifEntryViewModel(ge);
                        model.EntryDeleted += Model_EntryDeleted;
                        return model;
                    })
                );
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Model_EntryDeleted(object sender, EventArgs e)
        {
            var gifEntry = (GifEntryViewModel)sender;
            gifEntry.EntryDeleted -= Model_EntryDeleted;

            var mainThread = ServiceContainer.Instance.GetRequiredService<IMainThread>();
            mainThread.RunAsync(() => {
                images.Remove(gifEntry);
            });
        }

        public ObservableCollection<string> Tags {
            get { return tags; }
            private set {
                if (tags == value)
                    return;

                tags = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<GifEntryViewModel> Images {
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