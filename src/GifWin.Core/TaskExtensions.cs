using System;
using System.Threading.Tasks;
using GifWin.Core.Services;
using Microsoft.Extensions.Logging;

namespace GifWin.Core
{
    static class TaskExtensions
    {
        public static void FireAndForget<T>(this Task<T> self)
        {
            self.ContinueWith(t => {
                var logger = ServiceContainer.Instance.GetLogger(nameof(TaskExtensions));
                logger?.LogWarning(null, t.Exception, $"FAF: Dropping exception on floor.");
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        public static Task ContinueOrFault<T>(
            this Task<T> self,
            Action<Task<T>> @continue,
            Action<Task<T>> fault)
        {
            self.ContinueWith(@continue, TaskContinuationOptions.OnlyOnRanToCompletion);
            self.ContinueWith(fault, TaskContinuationOptions.NotOnRanToCompletion);
            return self;
        }
    }
}
