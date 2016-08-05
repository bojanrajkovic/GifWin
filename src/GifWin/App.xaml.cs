using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Squirrel;
using Windows.UI.Notifications;
using GifWin.Properties;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
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
            Settings.Default.Save ();
            base.OnExit (e);
        }

        protected override void OnStartup (StartupEventArgs e)
        {
            base.OnStartup (e);

            GifHelper.StartPreCachingDatabase ();

            SetupTheme();

            SystemEvents.UserPreferenceChanged += OnUserSystemPreferenceChanged;
            Settings.Default.PropertyChanged += OnSettingChanged;
			
            if (window == null) {
                window = new MainWindow ();
                RegisterHotkey ();
            }

            SetupTrayIcon ();
            SetupSquirrel ();
            ShowLaunchedNotification ();
        }

        internal void RegisterHotkey ()
        {
            Tuple<ModifierKeys, Key> oldRegistration = null;

            if (hotkey != null) {
                oldRegistration = Tuple.Create (hotkey.Modifiers, hotkey.Key);

                // Unregister
                hotkey.HotKeyPressed -= HotKeyPressed;
                hotkey.Unregister ();
                hotkey.Dispose ();
                hotkey = null;
            }

            Tuple<ModifierKeys, Key> newRegistration;
            string error;

            var isHotkeyValid = HotkeyParser.ParseHotkeySetting (Settings.Default.Hotkey, out newRegistration, out error);

            if (!isHotkeyValid) {
                var fallbackString = oldRegistration != null
                    ? $"{oldRegistration.Item1.ToString ("").Replace (", ", "-")}-${oldRegistration.Item2}"
                    : "Win-Shift-G";
                WMessageBox.Show (error + $" Falling back to {fallbackString}.", "Invalid hotkey!");
                Settings.Default.Hotkey = fallbackString;
                Settings.Default.Save ();
                HotkeyParser.ParseHotkeySetting (Settings.Default.Hotkey, out newRegistration, out error);
            }

            hotkey = new HotKey (newRegistration.Item1, newRegistration.Item2, window);
            hotkey.HotKeyPressed += HotKeyPressed;
        }

        void SetupSquirrel ()
        {
#if !DEBUG
            using (var um = new UpdateManager (GifWin.Properties.Resources.UpdatePath)) {
                SquirrelAwareApp.HandleEvents (onAppUpdate: ShowUpdateNotification);
            }
#endif
        }

        void ShowLaunchedNotification()
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier ("GifWin");
            var toastXml = ToastNotificationManager.GetTemplateContent (ToastTemplateType.ToastText02);

            var textElements = toastXml.GetElementsByTagName ("text");
            var titleNode = textElements[0];
            var textNode = textElements[1];

            titleNode.AppendChild (toastXml.CreateTextNode ("GifWin is now running!"));
            textNode.AppendChild (toastXml.CreateTextNode ($"Hit {Settings.Default.Hotkey} to bring up your library."));

            var toast = new ToastNotification (toastXml);
            toast.Activated += RestartGifWin;

            toastNotifier.Show (toast);
        }

        public static void ShowUpdateNotification (Version obj)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier ("GifWin");
            var toastXml = ToastNotificationManager.GetTemplateContent (ToastTemplateType.ToastText02);

            var textElements = toastXml.GetElementsByTagName ("text");
            var titleNode = textElements[0];
            var textNode = textElements[1];

            titleNode.AppendChild (toastXml.CreateTextNode ("A new GifWin version is available!"));
            textNode.AppendChild (toastXml.CreateTextNode ($"Version {obj} is now available. Restart GifWin (or click here!) to update."));

            var toast = new ToastNotification (toastXml);
            toast.Activated += RestartGifWin;

            toastNotifier.Show (toast);
        }

        static void RestartGifWin (ToastNotification sender, object args)
        {
            UpdateManager.RestartApp ();
        }

        NotifyIcon tray;
        MainWindow window;
        HotKey hotkey;

        void SetupTrayIcon ()
        {
            var menuItems = new[] {
                new MenuItem ("Convert GifWit Library"),
                new MenuItem ("Check For Updates"),
                new MenuItem ("Settings"),
                new MenuItem ("-"),
                new MenuItem ("Exit"),
            };

            menuItems[0].Click += (sender, args) => ConvertGifWitLibrary ();
            menuItems[1].Click += (sender, args) => CheckForUpdatesAsync ();
            menuItems[2].Click += (sender, args) => ShowSettingsDialog ();
            menuItems.Last ().Click += (sender, args) => Shutdown ();

            var contextMenu = new ContextMenu (menuItems);

            tray = new NotifyIcon {
                ContextMenu = contextMenu,
                Visible = true,
                Icon = GifWin.Properties.Resources.TrayIcon
            };
            this.tray.MouseClick += OnTrayClicked;
        }

        void ShowSettingsDialog ()
        {
            var sw = new SettingsWindow ();
            sw.ShowDialog ();
        }

        void ConvertGifWitLibrary ()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".gifwit",
                Filter = "GifWit Library Files (*.gifwit)|*.gifwit",
                InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
                Multiselect = false,
            };

            var result = dlg.ShowDialog ();

            if (result == true) {
                GifHelper.ConvertGifWitLibraryToGifWinDatabaseAsync (dlg.FileName).ContinueWith (t => {
                    if (!t.IsFaulted) {
                        GifHelper.StartPreCachingDatabase ();
                    }
                });
            }
        }

        async void CheckForUpdatesAsync ()
        {
            using (var um = new UpdateManager (GifWin.Properties.Resources.UpdatePath)) {
                await um.UpdateApp ();
            }
        }

        void OnTrayClicked (object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            window.Show ();
            window.Activate ();
        }

        void HotKeyPressed (HotKey obj)
        {
            window.Show ();
            window.Activate ();
        }

        void OnSettingChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof (Settings.Theme))
                SetupTheme();
        }

        const string MatchWindows = "Windows";
        void OnUserSystemPreferenceChanged (object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color) {
                if (Settings.Default.Theme == MatchWindows)
                    SetupTheme();
            }
        }

        void SetupTheme()
        {
            if (Resources.MergedDictionaries.Count > 0)
                Resources.MergedDictionaries.RemoveAt (0);

            bool light = true;
            if (Settings.Default.Theme == MatchWindows) {
                var personalTheme = Registry.CurrentUser.OpenSubKey ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                if (personalTheme != null) {
                    int useLightTheme = (int) personalTheme.GetValue ("AppsUseLightTheme", 1);
                    light = (useLightTheme == 1);
                }
            } else {
                light = (Settings.Default.Theme == "Light");
            }

            Resources.MergedDictionaries.Add (new ResourceDictionary {
                Source = new Uri ("Resources/" + ((light) ? "Light" : "Dark") + "Theme.xaml", UriKind.Relative)
            });
        }
    }
}
