using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using GifWin.Core.Services;

namespace GifWin.Core
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task self)
        {
            self.ContinueWith(t => {
                var logger = ServiceContainer.Instance.GetLogger(nameof(TaskExtensions));
                logger?.LogWarning(new EventId(), t.Exception, $"FAF: Dropping exception on floor.");
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
