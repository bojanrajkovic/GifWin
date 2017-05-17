using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using GifWin.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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
            DataContext = new MainPageViewModel ();

            var title = CoreApplication.GetCurrentView().TitleBar;
            if (title != null) {
                title.ExtendViewIntoTitleBar = true;
            }
        }   

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay") == true)
            {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.AutoPlay = false;
                imageSource.Stop();
            }
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "IsPlaying"))
            {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.Play();
            }
        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "IsPlaying"))
            {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.Stop();
            }
        }

        private async void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var imageSource = ((BitmapImage)((Image)sender).Source);
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var res = await client.GetAsync(imageSource.UriSource);
                    if (!res.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Could not load image from {imageSource.UriSource}, server returned {res.StatusCode}.");
                    }
                    else
                    {
                        Debug.WriteLine($"Could not load image from {imageSource.UriSource}, UWP says: {e.ErrorMessage}.");
                    }
                } catch (Exception ex)
                {
                    Debug.WriteLine($"Could not load image from {imageSource.UriSource}, UWP says: {e.ErrorMessage}, trying to use HttpClient failed with: {ex.Message}");
                }
            }
            var gifEntry = (GifEntryViewModel)((Image)sender).DataContext;
            if (gifEntry.FirstFrame != null)
                await imageSource.SetSourceAsync(new MemoryStream(gifEntry.FirstFrame).AsRandomAccessStream());
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var gifEntry = (GifEntryViewModel)((Image)sender).DataContext;
            var dp = new DataPackage();
            dp.SetText(gifEntry.Url);
            Clipboard.SetContent(dp);
        }
    }
}
