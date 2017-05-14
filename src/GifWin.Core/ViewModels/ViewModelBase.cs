using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GifWin.ViewModels
{
    public abstract class ViewModelBase
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged ([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (property));
        }
    }
}
