using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GifWin
{
    class HotKey : IDisposable
    {
        readonly int id;
        bool isRegistered;
        IntPtr hWnd;

        public event Action<HotKey> HotKeyPressed;

        public Key Key { get; private set; }
        public ModifierKeys Modifiers { get; private set; }

        public HotKey (ModifierKeys modifiers, Key input, Window window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            Key = input;
            Modifiers = modifiers;
            id = GetHashCode();
            var helper = new WindowInteropHelper(window);
            hWnd = helper.EnsureHandle();
            Register();
            ComponentDispatcher.ThreadPreprocessMessage += HandleMessage;
        }

        ~HotKey()
        {
            Dispose();
        }

        public void Register()
        {
            if (Key == Key.None)
                return;

            if (isRegistered)
                Unregister();

            isRegistered = NativeMethods.RegisterHotKey(hWnd, id, Modifiers, KeyInterop.VirtualKeyFromKey(Key));

            // If isRegistered is false, it means we couldn't register our hotkey.
            if (!isRegistered) {
                Debug.WriteLine("Win32 error message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
                throw new ApplicationException("Hotkey already in use!");
            }
        }

        public void Unregister()
        {
            isRegistered = !NativeMethods.UnregisterHotKey(hWnd, id);
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadPreprocessMessage -= HandleMessage;
            Unregister();
        }

        void HandleMessage(ref MSG msg, ref bool handled)
        {
            if (!handled) {
                if (msg.message == NativeMethods.WmHotKey && (int)msg.wParam == id) {
                    OnHotKeyPressed();
                    handled = true;
                }
            }
        }

        void OnHotKeyPressed()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this);
        }
    }
}
