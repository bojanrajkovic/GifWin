using System;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using GifWin.Core.Data;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    sealed class CopyImageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            var gifEntry = (GifEntryViewModel)parameter;

            db.RecordGifUsageAsync(gifEntry.Id, "*").FireAndForget();

            db.GetGifByIdAsync(gifEntry.Id).ContinueOrFault(
                @continue: t => clipService.PutImageOnClipboard(t.Result),
                fault: t => ServiceContainer.Instance.GetLogger<CopyImageCommand>()
                                           ?.LogWarning(new EventId(), t.Exception, "Could not copy image to clipboard.")
            );
        }
    }
}