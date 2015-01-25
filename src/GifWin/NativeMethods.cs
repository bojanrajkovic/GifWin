using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace GifWin
{
    class NativeMethods
    {
        // This is the message ID
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys modifiers, int key);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
