using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Microsoft.Data.Sqlite.Internal;
using Microsoft.Extensions.Logging;

using GifWin.Core;
using GifWin.Core.Data;
using GifWin.Core.Services;

namespace GifWin.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            CacheHelper.Init(ApplicationData.Current.LocalCacheFolder.Path);
            SqliteEngine.UseWinSqlite3();

            var lf = new LoggerFactory();
            lf.AddDebug();

            ServiceContainer.Instance.RegisterService<ILoggerFactory>(lf);
            ServiceContainer.Instance.RegisterService<IClipboardService>(new UWPClipboardService());
            ServiceContainer.Instance.RegisterService<IPrompter>(new UWPPrompter());
            ServiceContainer.Instance.RegisterService<IMainThread>(new UWPMainThread());

            var dbPath = SetUpDatabaseAsync().GetAwaiter().GetResult();
            var migrated = MigrateDatabaseAsync(dbPath).GetAwaiter().GetResult();

            if (!migrated)
                Current.Exit();

            StartCachingAsync();
        }

        async Task<string> SetUpDatabaseAsync()
        {
            // Copy the database into place.
            StorageFile file;
            try {
                file = await ApplicationData.Current.LocalFolder
                                            .GetFileAsync("GifWin.sqlite")
                                            .AsTask()
                                            .ConfigureAwait(false);
            } catch {
                var importedFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/GifWin.sqlite"))
                                                    .AsTask()
                                                    .ConfigureAwait(false);
                file = await importedFile.CopyAsync(ApplicationData.Current.LocalFolder)
                                         .AsTask()
                                         .ConfigureAwait(false);
            }

            return file.Path;
        }

        async Task<bool> MigrateDatabaseAsync(string dbPath)
        {
            var db = new GifWinDatabase(dbPath);
            ServiceContainer.Instance.RegisterService(db);
            return await db.ExecuteMigrationsAsync().ConfigureAwait(false);
        }

        Task StartCachingAsync()
        {
#pragma warning disable CS4014
            return Task.Run(async () => {
                var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
                var allGifs = await db.GetAllGifsAsync();

                // Build cache.
                await allGifs.ForEach(async g => await GifHelper.GetOrMakeSavedAsync(g, g.FirstFrame));
            });
#pragma warning restore CS4014
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null) {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false) {
                if (rootFrame.Content == null) {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            //TODO: Save application state and stop any background activity
            var db = ServiceContainer.Instance.GetRequiredService<GifWinDatabase>();
            db.Optimize();
            db.ForceFlush();

            deferral.Complete();
        }
    }
}
