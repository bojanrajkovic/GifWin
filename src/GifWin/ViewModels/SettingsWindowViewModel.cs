using GifWin.Commands;
using GifWin.Properties;
using System;
using System.Windows;
using System.Windows.Input;

namespace GifWin
{
    class SettingsWindowViewModel : ViewModelBase
    {
        string hotkey;
        readonly RelayCommand saveCommand;

        public SettingsWindowViewModel ()
        {
            saveCommand = new RelayCommand (DoSave);
            hotkey = Settings.Default.Hotkey;
        }

        void DoSave (object obj)
        {
            string error;
            var window = (SettingsWindow)obj;
            var didSave = Save (out error);

            if (didSave) {
                MessageBox.Show ("Saved settings!", "Saved!");
                window.Close ();
            } else {
                MessageBox.Show ("Error saving settings: " + error, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string Hotkey
        {
            get
            {
                return hotkey;
            }
            set
            {
                if (hotkey == value) {
                    return;
                }

                hotkey = value;
                OnPropertyChanged ();
            }
        }

        public RelayCommand SaveCommand
        {
            get { return saveCommand; }
        }

        public bool Save (out string error)
        {
            bool anyChanged = false;
            error = null;

            if (hotkey != Settings.Default.Hotkey) {
                if (!HotKeyIsValid (hotkey, out error)) {
                    return false;
                }

                anyChanged = true;
                Settings.Default.Hotkey = hotkey;
                ((App)Application.Current).RegisterHotkey ();
            }

            if (anyChanged) {
                Settings.Default.Save ();
            }

            return true;
        }

        bool HotKeyIsValid (string hotkey, out string error)
        {
            Tuple<ModifierKeys, Key> keys;
            var parses = HotkeyParser.ParseHotkeySetting (hotkey, out keys, out error);
            bool registrable = false;

            HotKey hk = null;
            try {
                hk = new HotKey (keys.Item1, keys.Item2, Application.Current.MainWindow);
                registrable = true;
            } catch {
                registrable = false;
                error = "Could not register hotkey, it is already in use.";
            } finally {
                if (hk != null) {
                    hk.Unregister ();
                    hk.Dispose ();
                }
            }

            return parses && registrable;
        }
    }
}
