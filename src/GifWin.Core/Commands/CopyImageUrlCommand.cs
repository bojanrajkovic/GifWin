using System;
using System.Windows.Input;

using GifWin.Core.Data;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    sealed class CopyImageUrlCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            var gifEntry = (GifEntryViewModel)parameter;

            db.RecordGifUsageAsync(gifEntry.Id, "*");

            clipService.PutTextOnClipboard(gifEntry.Url);
        }
    }
}