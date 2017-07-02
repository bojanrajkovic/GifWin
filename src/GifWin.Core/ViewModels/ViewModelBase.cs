using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace GifWin.Core.ViewModels
{
    [PublicAPI]
    public abstract class ViewModelBase
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
