using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Input;

namespace GifWin.Core.Commands
{
    class RelayCommand : ICommand
    {
        Action<object> command;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> command) =>
            this.command = command;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => command(parameter);
    }
}
