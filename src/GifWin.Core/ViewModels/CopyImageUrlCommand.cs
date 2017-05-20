using System;
using System.Windows.Input;
using GifWin.Core.Services;

namespace GifWin.Core.ViewModels
{
    internal class CopyImageUrlCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var clipService = ServiceContainer.Instance.GetRequiredService<IClipboardService>();
            var gifEntry = (GifEntryViewModel)parameter;
            clipService.PutTextOnClipboard(gifEntry.Url);
        }
    }
}