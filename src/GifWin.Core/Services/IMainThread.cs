using System;
using System.Threading.Tasks;

namespace GifWin.Core.Services
{
    public interface IMainThread
    {
        Task RunAsync(Action action);
        Task RunAsync(Func<Task> action);
    }
}
