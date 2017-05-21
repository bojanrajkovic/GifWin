using System;
using System.Windows.Input;

using GifWin.Core.Services;
using GifWin.Core.ViewModels;

namespace GifWin.Core.Commands
{
    internal class CopyImageUrlCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            parameter is GifEntryViewModel;

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var gifEntry = (GifEntryViewModel)parameter;
            clipService.PutTextOnClipboard(gifEntry.Url);
        }
    }
}