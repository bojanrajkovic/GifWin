using System;
using System.Windows.Input;

namespace GifWin.Core.Commands
{
    class RelayCommand : ICommand
    {
        readonly Action<object> command;
        readonly Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> command, Func<object, bool> canExecute)
        {
            this.command = command ?? throw new ArgumentNullException(nameof(command));
            this.canExecute = canExecute ?? (obj => true);
        }

        public bool CanExecute(object parameter) => canExecute(parameter);
        public void Execute(object parameter) => command(parameter);
    }
}
