using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;

using GifWin.Core.ViewModels;

namespace GifWin.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddNewGifPage : Page
    {
        public AddNewGifPage()
        {
            this.InitializeComponent();

            var vm = new AddNewGifViewModel();
            vm.GifEntryAdded += GifEntryAdded;

            DataContext = vm;

            var title = CoreApplication.GetCurrentView().TitleBar;
            if (title != null) {
                title.ExtendViewIntoTitleBar = false;
            }
        }

        void GifEntryAdded(object sender, Core.Models.GifEntry e) =>
            Frame.GoBack();
    }
}
