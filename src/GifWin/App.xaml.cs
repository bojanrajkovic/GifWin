﻿using System;
using System.Windows;
using GifWin.Properties;
using Application = System.Windows.Application;
using System.Windows.Input;
using GifWin.Data;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using WMessageBox = System.Windows.MessageBox;
using Microsoft.Data.Entity;

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

            if (this.window == null) {
                this.window = new MainWindow();

                hotkey = new HotKey(ModifierKeys.Windows | ModifierKeys.Shift, Key.G, window);
                hotkey.HotKeyPressed += HotKeyPressed;
            }

            SetupTrayIcon();
        }

        private NotifyIcon tray;
        private MainWindow window;
        HotKey hotkey;

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
            this.window.Show();
            this.window.Activate();
        }

        private void HotKeyPressed(HotKey obj)
        {
            window.Show();
            window.Activate();
        }
    }
}
