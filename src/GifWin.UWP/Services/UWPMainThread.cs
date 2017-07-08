using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

using GifWin.Core.Services;

namespace GifWin.UWP
{
    class UWPMainThread : IMainThread
    {
        public Task RunAsync(Action action) =>
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(action)
            ).AsTask();

        public Task RunAsync(Func<Task> action) =>
            RunTaskAsync(CoreApplication.MainView.CoreWindow.Dispatcher, action);

        // I can't believe this is what has to be done to actually _wait_ for the work.
        static async Task<T> RunTaskAsync<T>(
            CoreDispatcher dispatcher,
            Func<Task<T>> func,
            CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () => {
                try {
                    taskCompletionSource.SetResult(await func());
                } catch (Exception ex) {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        static async Task RunTaskAsync(
            CoreDispatcher dispatcher,
            Func<Task> func,
            CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) =>
            await RunTaskAsync(dispatcher, async () => { await func(); return false; }, priority);
    }
}
