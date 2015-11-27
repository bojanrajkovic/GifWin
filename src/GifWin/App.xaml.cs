using System;
using System.Windows;
using GifWin.Properties;
using Application = System.Windows.Application;
using System.Windows.Input;
using GifWin.Data;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using WMessageBox = System.Windows.MessageBox;

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

            SetupDatabase();
            ConvertGifWitLibraryToGifWinDatabaseAsync().GetAwaiter().GetResult();

            if (this.window == null) {
                this.window = new MainWindow();

                hotkey = new HotKey(ModifierKeys.Windows | ModifierKeys.Shift, Key.G, window);
                hotkey.HotKeyPressed += HotKeyPressed;
            }

            SetupTrayIcon();
        }

        async Task ConvertGifWitLibraryToGifWinDatabaseAsync()
        {
            if (!File.Exists("library.gifwit")) {
                return;
            }

            const string question = "You have a library.gifwit file available. Do you want to convert it to the GifWin database format?";
            const string caption = "Convert GifWit Library?";

            var mboxResult = WMessageBox.Show(question, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (mboxResult == MessageBoxResult.Yes) {
                using (var db = new GifWinDatabaseHelper()) {
                    try {
                        var lib = await GifWitLibrary.LoadFromFileAsync("library.gifwit").ConfigureAwait(false);
                        var converted = await db.ConvertGifWitLibraryAsync(lib).ConfigureAwait(false);
                        WMessageBox.Show($"Converted {converted} items successfully!", "Conversion succeeded!");
                    } catch (Exception e) {
                        WMessageBox.Show($"Conversion failed: {e.Message}.", "Conversion failed.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private NotifyIcon tray;
        private MainWindow window;
        HotKey hotkey;

        void SetupDatabase()
        {
            using (var db = new GifWinContext()) {
                db.Database.EnsureCreated();
            }
        }

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
