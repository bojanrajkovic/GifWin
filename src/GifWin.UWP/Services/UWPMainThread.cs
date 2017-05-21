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
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () => await action()
            ).AsTask();
    }
}
