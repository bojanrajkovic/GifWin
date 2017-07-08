using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

using GifWin.Core.Messages;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddNewGifPage : Page
    {
        IDisposable subscription;

        public AddNewGifPage()
        {
            this.InitializeComponent();

            DataContext = new AddNewGifViewModel();
            subscription = MessagingService.Subscribe<GifAdded>(OnNewGifAdded);

            var title = CoreApplication.GetCurrentView().TitleBar;
            if (title != null) {
                title.ExtendViewIntoTitleBar = false;
            }
        }

        bool OnNewGifAdded(GifAdded newGif)
        {
#pragma warning disable 4014
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                subscription.Dispose();
                Frame.GoBack();
            });
#pragma warning restore 4014

            return true;
        }
    }
}
