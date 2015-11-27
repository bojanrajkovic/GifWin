using System;
using System.Diagnostics;
using System.Windows;

namespace GifWin
{
    internal class GlobalHelper
    {
        [Conditional("DEBUG")]
        internal static void PromptForDebuggerLaunch(Exception exception)
        {
            var result = MessageBox.Show("Do you want to launch the debugger?", "Launch debugger?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) {
                Debugger.Launch();
            }
        }
    }
}