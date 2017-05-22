using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using GifWin.Core.Commands;
using GifWin.Core.Data;
using GifWin.Core.Services;

namespace GifWin.Core.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        GifWinDatabase db;

        string[] selectedTag;
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
                Tags = new ObservableCollection<string>(
                    t.Result.Select(tag => tag.Tag).Distinct()
                     .OrderBy(tag => tag)
                );
            }, TaskScheduler.FromCurrentSynchronizationContext());

            db.GetAllGifsAsync().ContinueWith(t => {
                Images = new ObservableCollection<GifEntryViewModel>(
                    t.Result.Select(ge => {
                        var model = new GifEntryViewModel(ge);
                        model.EntryDeleted += Model_EntryDeleted;
                        return model;
                    })
                );
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void Model_EntryDeleted(object sender, EventArgs e)
        {
            var gifEntry = (GifEntryViewModel)sender;
            gifEntry.EntryDeleted -= Model_EntryDeleted;

            var mainThread = ServiceContainer.Instance.GetRequiredService<IMainThread>();
            mainThread.RunAsync(() => {
                Images.Remove(gifEntry);
            });
        }

        public Action<object> AddNewGifCallback { get; set; }

        public ICommand AddNewGif => new RelayCommand(AddNewGifCallback);

        public string[] SelectedTag {
            get => selectedTag;
            set {
                if (selectedTag == value)
                    return;

                selectedTag = value;

                var gifs = selectedTag.Length == 0 ? db.GetAllGifsAsync() : db.GetGifsByTagAsync(selectedTag);
                gifs.ContinueWith(t => {
                    var mainThread = ServiceContainer.Instance.GetRequiredService<IMainThread>();
                    mainThread.RunAsync(() => {
                        Images.Clear();
                        Images = new ObservableCollection<GifEntryViewModel>(t.Result.Select(ge => {
                            var model = new GifEntryViewModel(ge, searchTerm: string.Join(", ", selectedTag));
                            model.EntryDeleted += Model_EntryDeleted;
                            return model;
                        }));
                    });
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        public ObservableCollection<string> Tags {
            get => tags;
            private set {
                if (tags == value)
                    return;

                tags = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<GifEntryViewModel> Images {
            get => images;
            private set {
                if (images == value)
                    return;

                images = value;
                RaisePropertyChanged();
            }
        }
    }
}