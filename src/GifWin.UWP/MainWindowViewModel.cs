using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GifWin.Data;
using System.Windows.Input;
using Windows.Storage;
using System.Text;
using Windows.Graphics.Imaging;

namespace GifWin.ViewModels
{
    public sealed class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel ()
        {
            Task.Run(async () => {
                StorageFile file;
                try {
                    file = await ApplicationData.Current.LocalFolder.GetFileAsync("GifWin.sqlite");
                } catch {
                    var importedFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/GifWin.sqlite"));
                    file = await importedFile.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder);
                }
                return file.Path;
            }).ContinueWith(t => {
                helper = new GifWinDatabaseHelper();
                RefreshImageCollection();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        internal static async Task<FrameData> GetFrameData(IStorageFile gifFile, uint frameNumber)
        {
            try
            {
                var decoder = await BitmapDecoder.CreateAsync(await gifFile.OpenReadAsync());

                if (frameNumber > decoder.FrameCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Frame number {frameNumber} is greater than frame count {decoder.FrameCount}");
                }

                var frame = await decoder.GetFrameAsync(frameNumber);

                // Now convert it to PNG
                var ms = new MemoryStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ms.AsRandomAccessStream());
                var pixelData = await frame.GetPixelDataAsync();
                var pixelBytes = pixelData.DetachPixelData();

                encoder.SetPixelData(frame.BitmapPixelFormat, frame.BitmapAlphaMode, frame.PixelWidth, frame.PixelHeight, frame.DpiX, frame.DpiY, pixelBytes);

                await encoder.FlushAsync().AsTask().ConfigureAwait(false);

                byte[] pngData = ms.ToArray();

                return new FrameData
                {
                    PngImage = pngData,
                    Width = (int)frame.PixelWidth,
                    Height = (int)frame.PixelHeight,
                };
            } catch (Exception e)
            {
                return null;
            }
        }

        static string GetReadableHash(byte[] hash)
        {
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                builder.Append(hash[i].ToString("X2"));

            return builder.ToString();
        }

        public ICommand AddNewGif => new AddNewGifCommand();

        public IReadOnlyList<GifEntryViewModel> Images
        {
            get { return images; }
            private set
            {
                if (images == value)
                    return;

                images = value;
                Tags = images.SelectMany(i => i.Keywords).Distinct().ToArray();
                RaisePropertyChanged ();
            }
        }

        public IReadOnlyList<string> Tags
        {
            get { return tags; }
            private set 
            {
                if (tags == value)
                    return;

                tags = value;
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
        IReadOnlyList<string> tags;
        IReadOnlyList<GifEntryViewModel> images;
        readonly HashSet<string> filterKeywords = new HashSet<string> {
            "*"
        };

        private void RefreshImageCollection ()
        {
            Task<IEnumerable<GifEntry>> gifs;

            var filterArray = filterKeywords.ToArray ();
            if (filterArray.Length == 1 && filterArray[0] == "*")
                gifs = Task.FromResult (helper.QueryGifs (e => true, e => e, new[] { "Tags" }).AsEnumerable ());
            else
                gifs = helper.GetGifsbyTagAsync (filterArray);

            gifs.ContinueWith (t => {
                var filterResults = t.Result.Select (ge => new GifEntryViewModel (ge));
                Images = filterResults.ToArray();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}