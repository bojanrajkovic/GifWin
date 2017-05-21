using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using GifWin.Core.Services;

namespace GifWin.UWP
{
    class UWPPrompter : IPrompter
    {
        public async Task<bool> PromptYesNoAsync(string title, string detail)
        {
            var cd = new ContentDialog {
                Title = title,
                Content = detail,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };
            var result = await cd.ShowAsync();

            switch (result) {
                case ContentDialogResult.Primary:
                    return true;
                default:
                    return false;
            }
        }
    }
}
