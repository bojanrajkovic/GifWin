using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
            lib = GifWitLibrary.LoadFromFile ("library.gifwit");
            listBox.DataContext = lib;
        }

        private void textBox_TextChanged (object sender, TextChangedEventArgs e)
        {
            if (lib != null && !string.IsNullOrWhiteSpace (textBox.Text)) {
                typingTimer?.Dispose ();

                typingTimer = new Timer (state => {
                    var newKeywords = ((string) state).Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Dispatcher.Invoke (() => {
                        if (newKeywords.Any ()) {
                            listBox.DataContext = lib.Where (x => newKeywords.Any (nk => x.Keywords.Contains (nk))).ToList ();
                        } else {
                            listBox.DataContext = lib;
                        }

                        listBox.Items.Refresh ();
                        listBox.Focus ();
                    });
                }, textBox.Text, TimeSpan.FromMilliseconds (500), TimeSpan.FromMilliseconds (-1));
            }
        }

        private void ListBox_OnMouseDoubleClick (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            var entry = (GifWitLibraryEntry) listBox.SelectedItem;
            Clipboard.SetText (entry.Url.ToString ());
        }

        private void ListBox_OnKeyDown (object sender, KeyEventArgs e)
        {
            // Because I am lazy, just reuse the same thing. This should probably be a command on a ViewModel.
            if (e.Key == Key.Enter) {
                ListBox_OnMouseDoubleClick (sender, new MouseButtonEventArgs (InputManager.Current.PrimaryMouseDevice, 5, MouseButton.Left));
            }
        }
    }
}
