using System.Windows;
using GifWin.Properties;

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
    }
}
