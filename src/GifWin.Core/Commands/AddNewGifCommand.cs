using System;
using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using GifWin.Core.Data;
using GifWin.Core.Messages;
using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    sealed class AddNewGifCommand : ICommand
    {
        bool isAdding;

        public event EventHandler CanExecuteChanged;

        public AddNewGifCommand(AddNewGifViewModel vm)
        {
            vm.PropertyChanged += ViewModelPropertyChanged;
        }

        void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) => 
            CanExecuteChanged?.Invoke(this, null);

        public bool CanExecute(object parameter)
        {
            var model = parameter as AddNewGifViewModel;

            if (model == null || isAdding)
                return false;

            return model.Tags?.Length > 0 && !string.IsNullOrWhiteSpace(model.Url);
        }

        public void Execute(object parameter)
        {
            // Disable the button.
            isAdding = true;
            CanExecuteChanged?.Invoke(this, null);

            var vm = (parameter as AddNewGifViewModel);

            if (vm == null)
                return;

            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            var mt = ServiceContainer.Instance.GetRequiredService<IMainThread>();

            db.AddGifEntryAsync(vm.Url, vm.Tags).ContinueOrFault(
                @continue: async t => {
                    var gifEntry = await t;
                    GifHelper.GetOrMakeSavedAsync(gifEntry, gifEntry.FirstFrame).FireAndForget();
                    MessagingService.Send(new GifAdded {
                        NewGif = t.Result
                    });
                },
                fault: t => {
                    var logger = ServiceContainer.Instance.GetLogger<AddNewGifCommand>();
                    logger?.LogWarning(new EventId(), t.Exception, "Could not add GIF.");
                    var prompter = ServiceContainer.Instance.GetRequiredService<IPrompter>();
                    mt.RunAsync(async () => await prompter.ShowMessageAsync("Could not add GIF", "An error occurred."));
                }
            );
        }
    }
}
