using GifWin.Data;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            CopyImage();
        }

        private void CopyImage()
        {
            var entry = (GifEntryViewModel) this.imageList.SelectedItem;
            if (entry == null)
                return;

            Clipboard.SetText (entry.Url);

            var searchText = search.Text;
            Task.Run(async () => {
                using (var helper = new GifWinDatabaseHelper()) {
                    try {
                        await helper.RecordGifUsageAsync(entry.Id, searchText);
                    } catch (Exception e) {
                        await Dispatcher.InvokeAsync(() => {
                            MessageBox.Show($"Couldn't save usage record: {e.InnerException.Message}.", "Failed");
                            GlobalHelper.PromptForDebuggerLaunch(e);
                        });
                    }
                }
            });

            this.search.Clear();

            Hide();
        }

        private void GifEntryKeyPressed (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                CopyImage();
        }

        private void OnWindowKeyUp (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();
        }

        private void SearchBoxKeyPressed(object sender, KeyEventArgs e)
        {
            if (SearchStates.CurrentState == null || SearchStates.CurrentState.Name != "Searching")
                return;

            if (e.Key == Key.Down) {
                imageList.Focus();
            }
        }
    }
}
