using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Squirrel;
using Windows.UI.Notifications;
using GifWin.Properties;
using Application = System.Windows.Application;
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

            GifHelper.ConvertGifWitLibraryToGifWinDatabaseAsync ().GetAwaiter ().GetResult ();
            GifHelper.StartPreCachingDatabase ();

            if (window == null) {
                window = new MainWindow ();

                var hotkeyRegistration = ParseHotkeySetting ();
                hotkey = new HotKey (hotkeyRegistration.Item1, hotkeyRegistration.Item2, window);
                hotkey.HotKeyPressed += HotKeyPressed;
            }

            SetupTrayIcon ();
            SetupSquirrel ();
            ShowLaunchedNotification ();
        }

        Tuple<ModifierKeys, Key> ParseHotkeySetting ()
        {
            var hotkeySetting = Settings.Default.Hotkey.OrIfBlank ("Win-Shift-G");
            return ParseHotkeySettingImpl (hotkeySetting);
        }

        Tuple<ModifierKeys, Key> ParseHotkeySettingImpl (string hotkeySetting)
        {
            // Silently drop empty hotkey parts.
            var hotkeyPieces = hotkeySetting.Split (new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            var keyString = hotkeyPieces.Last ();
            var modifierStrings = hotkeyPieces.TakeWhile (hkp => hkp != keyString).Distinct().ToArray ();

            // Parse the hotkey itself.
            Key key;
            if (!Enum.TryParse (keyString, out key)) {
                WMessageBox.Show ($"Invalid hotkey setting: the value {keyString} is not a valid hotkey key. Falling back to default Win-Shift-G hotkey.", "Invalid hotkey.");
                Settings.Default.Hotkey = "Win-Shift-G";
                return ParseHotkeySettingImpl (Settings.Default.Hotkey);
            }

            // Parse each modifier part to build the set of modifiers.
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (var modifierString in modifierStrings) {
                ModifierKeys tempModifier;

                if (!Enum.TryParse (ExpandModifierString (modifierString), out tempModifier)) {
                    WMessageBox.Show ($"Invalid hotkey setting: the value {modifierString} is not a valid hotkey modifier. Falling back to default Win-Shift-G hotkey.", "Invalid hotkey.");
                    Settings.Default.Hotkey = "Win-Shift-G";
                    return ParseHotkeySettingImpl (Settings.Default.Hotkey);
                }

                modifiers |= tempModifier;
            }

            // Don't allow modifier-less hotkeys
            if (modifiers == ModifierKeys.None) {
                var modifierString = string.Join ("-", modifierStrings);
                WMessageBox.Show ($"Invalid hotkey setting: the modifier string {modifierString} is not valid. Falling back to default Win-Shift-G hotkey.", "Invalid hotkey.");
                Settings.Default.Hotkey = "Win-Shift-G";
                return ParseHotkeySettingImpl (Settings.Default.Hotkey);
            }

            return Tuple.Create (modifiers, key);
        }

        string ExpandModifierString (string input)
        {
            switch (input) {
                case "Win":
                    return "Windows";
                case "Ctrl":
                    return "Control";
                default:
                    return input;
            }
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
            var contextMenu = new ContextMenu (new[] {
                new MenuItem ("Check For Updates"),
                new MenuItem ("-"),
                new MenuItem ("Exit"),

            });
            contextMenu.MenuItems[2].Click += (sender, args) => Shutdown ();
            contextMenu.MenuItems[0].Click += (sender, args) => CheckForUpdatesAsync ();

            tray = new NotifyIcon {
                ContextMenu = contextMenu,
                Visible = true,
                Icon = GifWin.Properties.Resources.TrayIcon
            };

            tray.Click += OnTrayClicked;
        }

        async void CheckForUpdatesAsync ()
        {
            using (var um = new UpdateManager (GifWin.Properties.Resources.UpdatePath)) {
                await um.UpdateApp ();
            }
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
