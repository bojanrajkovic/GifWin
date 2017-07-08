using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace GifWin.Core.Services
{
    [PublicAPI]
    public interface IMainThread
    {
        Task RunAsync(Action action);
        Task RunAsync(Func<Task> action);
    }
}
