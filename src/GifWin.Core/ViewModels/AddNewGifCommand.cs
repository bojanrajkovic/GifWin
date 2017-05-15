using System;
using System.Windows.Input;

namespace GifWin.ViewModels
{
    public class AddNewGifCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
        {
        }
    }
}