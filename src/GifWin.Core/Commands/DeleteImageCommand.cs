using System;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using GifWin.Core.Data;
using GifWin.Core.Messages;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    sealed class DeleteImageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var prompter = ServiceContainer.Instance.GetRequiredService<IPrompter>();

            prompter.PromptYesNoAsync($"Delete this image?", "Are you sure? This is permanent.")
                    .ContinueWith(async t => {
                        var result = await t;

                        if (!result)
                            return;

                        var model = (GifEntryViewModel)parameter;
                        var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
                        var logger = ServiceContainer.Instance.GetLogger<DeleteImageCommand>();

                        try {
                            await db.DeleteGifAsync(model.Id);
                        } catch (Exception e) {
                            logger?.LogWarning(new EventId(), e, "Could not delete GIF from database.");
                        }

                        MessagingService.Send(new GifDeleted {
                            DeletedGifId = model.Id
                        });
                    });
        }
    }
}
