using System;
using System.Threading.Tasks;
using System.Windows.Input;

using GifWin.Core.Data;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    internal class CopyImageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            var gifEntry = (GifEntryViewModel)parameter;
            db.GetGifByIdAsync(gifEntry.Id).ContinueWith(
                t => clipService.PutImageOnClipboard(t.Result),
                TaskContinuationOptions.OnlyOnRanToCompletion
            );
        }
    }
}