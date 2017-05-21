﻿using System.Linq;
using Windows.ApplicationModel.Core;
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

        private void Image_Loaded(object sender, RoutedEventArgs e) =>
            ApiHelper.RunIfPropertyIsPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay", () => {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.AutoPlay = false;
                imageSource.Stop();
            });

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e) =>
            ApiHelper.RunIfPropertyIsPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay", () => {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.Play();
            });

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e) =>
            ApiHelper.RunIfPropertyIsPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay", () => {
                var imageSource = ((BitmapImage)((Image)sender).Source);
                imageSource.Stop();
            });

        void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var gifEntry = (GifEntryViewModel)((Image)sender).DataContext;
            gifEntry.CopyImageUrlCommand.Execute(gifEntry);
        }

        private void TagSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ((MainWindowViewModel)DataContext).SelectedTag =
                ((ListView)sender).SelectedItems.Cast<string>().ToArray();
    }
}
