using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GifWitLibrary lib;
        private Timer typingTimer;

        public MainWindow ()
        {
            InitializeComponent ();
        }

        private void GifEntryClicked (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            var entry = (GifWitLibraryEntry) this.imageList.SelectedItem;
            Clipboard.SetText (entry.Url.ToString ());
        }

        private void GifEntryKeyPressed (object sender, KeyEventArgs e)
        {
            // Because I am lazy, just reuse the same thing. This should probably be a command on a ViewModel.
            if (e.Key == Key.Enter) {
                GifEntryClicked (sender, new MouseButtonEventArgs (InputManager.Current.PrimaryMouseDevice, 5, MouseButton.Left));
            }
        }
    }
}
