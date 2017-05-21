using System;
using System.Windows.Input;

namespace GifWin.Core.Commands
{
    sealed class DeleteImageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => false;

        public void Execute(object parameter) =>
            throw new NotImplementedException();
    }
}
