using GifWin.Properties;
using System;
using System.Linq;
using System.Windows.Input;

namespace GifWin
{
    static class HotkeyParser
    {
        public static bool ParseHotkeySetting (string hotkeySetting, out Tuple<ModifierKeys, Key> hotkeyRegistration, out string error)
        {
            hotkeySetting = hotkeySetting.OrIfBlank (Settings.Default.Hotkey).OrIfBlank ("Win-Shift-G");
            return ParseHotkeySettingImpl (hotkeySetting, out hotkeyRegistration, out error);
        }

        public static bool ParseHotkeySettingImpl (string hotkeySetting, out Tuple<ModifierKeys, Key> hotkeyRegistration, out string error)
        {
            hotkeyRegistration = null;
            error = string.Empty;

            // Silently drop empty hotkey parts.
            var hotkeyPieces = hotkeySetting.Split (new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            var keyString = hotkeyPieces.Last ();
            var modifierStrings = hotkeyPieces.TakeWhile (hkp => hkp != keyString).Distinct ().ToArray ();

            // Parse the hotkey itself.
            Key key;
            if (!Enum.TryParse (keyString, ignoreCase: true, result: out key)) {
                error = $"Invalid hotkey setting: the value {keyString} is not a valid hotkey key.";
                return false;
            }

            // Parse each modifier part to build the set of modifiers.
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (var modifierString in modifierStrings) {
                ModifierKeys tempModifier;

                if (!Enum.TryParse (ExpandModifierString (modifierString), ignoreCase: true, result: out tempModifier)) {
                    error = $"Invalid hotkey setting: the value {modifierString} is not a valid hotkey modifier.";
                    return false;
                }

                modifiers |= tempModifier;
            }

            // Don't allow modifier-less hotkeys
            if (modifiers == ModifierKeys.None) {
                var modifierString = string.Join ("-", modifierStrings);
                error = $"Invalid hotkey setting: the modifier string {modifierString} is not valid.";

                return false;
            }

            hotkeyRegistration = Tuple.Create (modifiers, key);
            return true;
        }

        static string ExpandModifierString (string input)
        {
            switch (input.ToLower()) {
                case "win":
                    return "Windows";
                case "ctrl":
                    return "Control";
                default:
                    return input;
            }
        }
    }
}
