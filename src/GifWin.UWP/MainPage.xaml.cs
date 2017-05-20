using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

using GifWin.Core.ViewModels;

namespace GifWin.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            var title = CoreApplication.GetCurrentView().TitleBar;
            if (title != null) {
                title.ExtendViewIntoTitleBar = true;
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay") == true) {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.AutoPlay = false;
                imageSource.Stop();
            }
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "IsPlaying")) {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.Play();
            }
        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "IsPlaying")) {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.Stop();
            }
        }

        private async void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var imageSource = ((BitmapImage)((Image)sender).Source);
            Debug.WriteLine($"Could not load image from {imageSource.UriSource}, UWP says: {e.ErrorMessage}.");

            var gifEntry = (GifEntryViewModel)((Image)sender).DataContext;
            if (gifEntry.FirstFrame != null)
                await imageSource.SetSourceAsync(new MemoryStream(gifEntry.FirstFrame).AsRandomAccessStream());
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var gifEntry = (GifEntryViewModel)((Image)sender).DataContext;

            var clipboardPackage = new DataPackage();
            clipboardPackage.SetText(gifEntry.Url);

            Clipboard.SetContent(clipboardPackage);
        }
    }
}
