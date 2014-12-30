using System;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using GifWin.Properties;
using Application = System.Windows.Application;

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

            SetupTrayIcon();
        }

        private NotifyIcon tray;
        private MainWindow window;

        private void SetupTrayIcon()
        {
            var contextMenu = new ContextMenu (new[] {
                new MenuItem ("Exit"),
            });
            contextMenu.MenuItems[0].Click += (sender, args) => Shutdown();

            this.tray = new NotifyIcon {
                ContextMenu = contextMenu,
                Visible = true,
                Icon = GifWin.Properties.Resources.TrayIcon
            };

            this.tray.Click += OnTrayClicked;
        }

        private void OnTrayClicked (object sender, EventArgs eventArgs)
        {
            if (this.window == null || !this.window.IsLoaded)
                this.window = new MainWindow();

            this.window.Show();
        }
    }
}
