using System;
using System.Windows.Input;

using GifWin.Core.Data;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    sealed class CopyImageUrlCommand : ICommand
    {
        readonly string searchTerm;

        public event EventHandler CanExecuteChanged;

        public CopyImageUrlCommand(string searchTerm)
            => this.searchTerm = searchTerm;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            var gifEntry = (GifEntryViewModel)parameter;

            db.RecordGifUsageAsync(gifEntry.Id, searchTerm).FireAndForget();

            clipService.PutTextOnClipboard(gifEntry.OriginalUrl);
        }
    }
}