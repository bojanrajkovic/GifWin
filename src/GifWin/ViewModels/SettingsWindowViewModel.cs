using GifWin.Properties;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace GifWin
{
    class SettingsWindowViewModel : ViewModelBase
    {
        bool settingsDirty;
        string theme;
        string hotkey;
        readonly RelayCommand<Window> saveCommand;

        public SettingsWindowViewModel ()
        {
            saveCommand = new RelayCommand<Window> (DoSave, w => settingsDirty);
            hotkey = Settings.Default.Hotkey;
            theme = Settings.Default.Theme;
        }

        public string Theme
        {
            get { return theme; }
            set
            {
                if (theme == value)
                    return;

                theme = value;
                OnSettingChanged ();
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
                OnSettingChanged ();
            }
        }

        public ICommand SaveCommand
        {
            get { return saveCommand; }
        }

        bool SettingsDirty
        {
            get { return settingsDirty; }
            set
            {
                if (settingsDirty == value)
                    return;

                settingsDirty = value;
                RaisePropertyChanged ();
                saveCommand.RaiseCanExecuteChanged();
            }
        }

        bool Save (out string error)
        {
            error = null;

            if (hotkey != Settings.Default.Hotkey) {
                if (!HotKeyIsValid (hotkey, out error)) {
                    return false;
                }

                Settings.Default.Hotkey = hotkey;
                ((App)Application.Current).RegisterHotkey ();
            }

            if (theme != Settings.Default.Theme) {
                Settings.Default.Theme = theme;
            }

            if (settingsDirty) {
                Settings.Default.Save ();
                settingsDirty = false;
            }

            return true;
        }

        void DoSave (Window window)
        {
            string error;
            var didSave = Save (out error);

            if (didSave) {
                MessengerInstance.Send (new SettingsSaved());
            } else {
                MessageBox.Show ("Error saving settings: " + error, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void OnSettingChanged ([CallerMemberName] string propertyName = null)
        {
            SettingsDirty = true;
            RaisePropertyChanged (propertyName);
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
