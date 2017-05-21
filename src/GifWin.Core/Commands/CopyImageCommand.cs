using System;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using GifWin.Core.Data;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    sealed class CopyImageCommand : ICommand
    {
        string searchTerm;

        public CopyImageCommand(string searchTerm) =>
            this.searchTerm = searchTerm;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            var gifEntry = (GifEntryViewModel)parameter;

            db.RecordGifUsageAsync(gifEntry.Id, searchTerm).FireAndForget();

            db.GetGifByIdAsync(gifEntry.Id).ContinueOrFault(
                @continue: async t => {
                    try {
                        var gif = await t;
                        var savedImage = await GifHelper.GetOrMakeSavedAsync(gif, gif.FirstFrame);
                        var mt = ServiceContainer.Instance.GetRequiredService<IMainThread>();
                        await mt.RunAsync(() => clipService.PutImageOnClipboard(savedImage));
                    } catch (Exception e) {
                        FaultHandler(e);
                    }
                },
                fault: t => FaultHandler(t.Exception)
            );
        }

        void FaultHandler(Exception e)
        {
            ServiceContainer.Instance.GetLogger<CopyImageCommand>()
                                    ?.LogWarning(
                                        new EventId(),
                                        e,
                                        "Could not copy image to clipboard."
                                    );

            var mt = ServiceContainer.Instance.GetRequiredService<IMainThread>();
            mt.RunAsync(() => {
                ServiceContainer.Instance.GetOptionalService<IPrompter>()
                               ?.ShowMessageAsync(
                                   "Error",
                                   "Could not copy image to clipboard."
                               ).FireAndForget();
            });
        }
    }
}