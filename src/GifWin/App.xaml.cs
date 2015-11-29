using System;
using System.Windows;
using GifWin.Properties;
using Application = System.Windows.Application;
using System.Windows.Input;
using System.Windows.Forms;
using Squirrel;
using Windows.UI.Notifications;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit (ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit (e);
        }

        protected override void OnStartup (StartupEventArgs e)
        {
            base.OnStartup (e);

            GifHelper.ConvertGifWitLibraryToGifWinDatabaseAsync().GetAwaiter().GetResult();
            GifHelper.StartPreCachingDatabase ();

            if (window == null) {
                window = new MainWindow();

                hotkey = new HotKey(ModifierKeys.Windows | ModifierKeys.Shift, Key.G, window);
                hotkey.HotKeyPressed += HotKeyPressed;
            }

            SetupTrayIcon();
            SetupSquirrel ();
        }

        void SetupSquirrel ()
        {
#if !DEBUG
            using (var um = new UpdateManager (GifWin.Properties.Resources.UpdatePath)) {
                SquirrelAwareApp.HandleEvents (onAppUpdate: ShowUpdateNotification);
            }
#endif
        }

        public static void ShowUpdateNotification (Version obj)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier ("GifWin");
            var toastXml = ToastNotificationManager.GetTemplateContent (ToastTemplateType.ToastText02);

            var textElements = toastXml.GetElementsByTagName ("text");
            var titleNode = textElements[0];
            var textNode = textElements[1];

            titleNode.AppendChild (toastXml.CreateTextNode ("A new GifWin version is available!"));
            textNode.AppendChild (toastXml.CreateTextNode ($"Version {obj} is now available. Restart GifWin to update."));

            var toast = new ToastNotification (toastXml);

            toastNotifier.Show (toast);
        }

        NotifyIcon tray;
        MainWindow window;
        HotKey hotkey;

        void SetupTrayIcon ()
        {
            var contextMenu = new ContextMenu (new[] {
                new MenuItem ("Exit"),
            });
            contextMenu.MenuItems[0].Click += (sender, args) => Shutdown ();

            tray = new NotifyIcon {
                ContextMenu = contextMenu,
                Visible = true,
                Icon = GifWin.Properties.Resources.TrayIcon
            };

            tray.Click += OnTrayClicked;
        }

        void OnTrayClicked (object sender, EventArgs eventArgs)
        {
            window.Show ();
            window.Activate ();
        }

        void HotKeyPressed (HotKey obj)
        {
            window.Show ();
            window.Activate ();
        }
    }
}
