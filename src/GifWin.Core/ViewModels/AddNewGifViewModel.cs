using System.Linq;
using System.Windows.Input;

using JetBrains.Annotations;

using GifWin.Core.Commands;

namespace GifWin.Core.ViewModels
{
    [PublicAPI]
    public sealed class AddNewGifViewModel : ViewModelBase
    {
        string url, tagString;
        string[] tags;

        public ICommand AddGif => new AddNewGifCommand(this);

        public string Url {
            get => url;
            set {
                if (url == value)
                    return;

                url = value;
                RaisePropertyChanged();
            }
        }

        public string TagString {
            get => tagString;
            set {
                if (tagString == value)
                    return;

                tagString = value;
                Tags = tagString.Split(',');
                RaisePropertyChanged();
            }
        }

        public string[] Tags {
            get => tags;
            set {
                if (tags?.SequenceEqual(value) ?? false)
                    return;

                tags = value;
                RaisePropertyChanged();
            }
        }
    }
}
