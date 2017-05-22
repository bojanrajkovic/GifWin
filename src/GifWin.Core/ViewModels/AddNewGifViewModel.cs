using System;
using System.Linq;
using System.Windows.Input;

using GifWin.Core.Commands;
using GifWin.Core.Models;

namespace GifWin.Core.ViewModels
{
    public class AddNewGifViewModel : ViewModelBase
    {
        string url, tagString;
        string[] tags;

        public event EventHandler<GifEntry> GifEntryAdded;
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

        internal void RaiseGifEntryAdded(GifEntry newGifEntry) =>
            GifEntryAdded?.Invoke(this, newGifEntry);
    }
}
