using System;
using System.ComponentModel;
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
            ((MainWindowViewModel)DataContext).PropertyChanged += OnPropertyChanged;
        }

        public new void Show()
        {
            base.Show();
            this.search.Focus();
        }

        public new void Hide()
        {
            this.search.Clear();
            base.Hide();
        }

        protected override void OnClosing (CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            // We listen to this instead of the input text so we can be sure the VM has had a chance to filter before we expand.
            if (e.PropertyName == "FilterText") {
                if (this.search.Text == String.Empty)
                    VisualStateManager.GoToElementState (this, "NotSearching", useTransitions: true);
                 else
                    VisualStateManager.GoToElementState (this, "Searching", useTransitions: true);
            }
        }

        private void GifEntryClicked (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            var entry = (GifEntryViewModel) this.imageList.SelectedItem;
            Clipboard.SetText (entry.Url.ToString ());
        }

        private void GifEntryKeyPressed (object sender, KeyEventArgs e)
        {
            // Because I am lazy, just reuse the same thing. This should probably be a command on a ViewModel.
            if (e.Key == Key.Enter) {
                GifEntryClicked (sender, new MouseButtonEventArgs (InputManager.Current.PrimaryMouseDevice, 5, MouseButton.Left));
            }
        }

        private void OnWindowKeyUp (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();
        }
    }
}
