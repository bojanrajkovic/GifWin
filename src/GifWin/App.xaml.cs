using System;
using System.Windows;
using GifWin.Properties;
using Application = System.Windows.Application;
using System.Windows.Input;
using System.Windows.Forms;

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
