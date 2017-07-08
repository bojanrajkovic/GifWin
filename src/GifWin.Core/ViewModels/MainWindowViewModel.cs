using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using JetBrains.Annotations;

using GifWin.Core.Commands;
using GifWin.Core.Data;
using GifWin.Core.Messages;
using GifWin.Core.Services;

namespace GifWin.Core.ViewModels
{
    [PublicAPI]
    public sealed class MainWindowViewModel : ViewModelBase
    {
        readonly GifWinDatabase db;

        string[] selectedTag;
        ObservableCollection<string> tags;
        ObservableCollection<GifEntryViewModel> images;
        IDisposable imageDeletedSubscription;

        public MainWindowViewModel()
        {
            db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            imageDeletedSubscription = MessagingService.Subscribe<GifDeleted>(OnImageDeleted);
            RefreshImageCollection();
        }

        bool OnImageDeleted(GifDeleted arg)
        {
            var mt = ServiceContainer.Instance.GetRequiredService<IMainThread>();
            mt.RunAsync(() => {
                Images.Where(gevm => gevm.Id == arg.DeletedGifId)
                      .ToArray()
                      .ForEach(gevm => Images.Remove(gevm));
            });

            return true;
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
                    t.Result.Select(ge => new GifEntryViewModel(ge))
                );
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Action<object> AddNewGifCallback { get; set; }

        public ICommand AddNewGif => new RelayCommand(AddNewGifCallback, param => true);

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
                        Images = new ObservableCollection<GifEntryViewModel>(t.Result.Select(ge =>
                            new GifEntryViewModel(ge, searchTerm: string.Join(", ", selectedTag)))
                        );
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
