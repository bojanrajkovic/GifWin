using GifWin.Data;
using Squirrel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace GifWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer updateCheckTimer;

        public MainWindow ()
        {
            InitializeComponent ();
            ((MainWindowViewModel)DataContext).PropertyChanged += OnPropertyChanged;

            updateCheckTimer = new Timer (TimeSpan.FromSeconds (10).TotalSeconds);
            updateCheckTimer.Elapsed += CheckForUpdatesAsync;
            updateCheckTimer.Start ();
        }

        async void CheckForUpdatesAsync (object sender, ElapsedEventArgs e)
        {
            try {
                using (var updateMgr = new UpdateManager ("https://gifwin-releases.s3.amazonaws.com/")) {
                    await updateMgr.UpdateApp ();
                }
            } catch (Exception ex) {
                Debug.WriteLine ($"Error while calling UpdateManager!UpdateApp: {ex.Message}.");
                Debug.WriteLine (ex.ToString ());
            }
        }

        public new void Show ()
        {
            base.Show ();
            this.search.Focus ();
        }

        public new void Hide ()
        {
            this.search.Clear ();
            this.tag.Clear ();
            base.Hide ();
        }

        private void AddNewGif (string gifUrl, string tags)
        {
            var helper = new GifWinDatabaseHelper ();
            var tagsArray = tags.Split (' ');
            helper.AddNewGifAsync (gifUrl, tagsArray).ContinueWith (t => {
                if (t.IsFaulted) {
                    MessageBox.Show ("Could not save GIF to database.", "Could not save GIF", MessageBoxButton.OK, MessageBoxImage.Error);
                    GlobalHelper.PromptForDebuggerLaunch (t.Exception);
                } else {
                    // In lieu of a messenger-like thing, hit up the VM directly.
                    var vm = DataContext as MainWindowViewModel;

                    if (vm != null) {
                        vm.RefreshImagesFromDatabase ();
                    }

                    GifHelper.GetOrMakeSavedAsync (t.Result.Id, t.Result.Url);
                    Dispatcher.Invoke(Hide);
                }
                helper.Dispose ();
            });
        }

        private void CopyImage ()
        {
            var entry = (GifEntryViewModel)this.imageList.SelectedItem;
            if (entry == null)
                return;

            Clipboard.SetText (entry.Url);

            var searchText = search.Text;
            Task.Run (async () => {
                using (var helper = new GifWinDatabaseHelper ()) {
                    try {
                        await helper.RecordGifUsageAsync (entry.Id, searchText);
                    } catch (Exception e) {
                        await Dispatcher.InvokeAsync (() => {
                            MessageBox.Show ($"Couldn't save usage record: {e.InnerException.Message}.", "Failed");
                            GlobalHelper.PromptForDebuggerLaunch (e);
                        });
                    }
                }
            });

            Hide ();
        }

        protected override void OnClosing (CancelEventArgs e)
        {
            e.Cancel = true;
            Hide ();
        }

        private void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            // We listen to this instead of the input text so we can be sure the VM has had a chance to filter before we expand.
            if (e.PropertyName == "FilterText") {
                if (this.search.Text == String.Empty)
                    VisualStateManager.GoToElementState (this, "NotSearching", useTransitions: true);
                else {
                    if (Uri.IsWellFormedUriString (this.search.Text, UriKind.Absolute)) {
                        VisualStateManager.GoToElementState (this, "Adding", useTransitions: true);
                    } else {
                        VisualStateManager.GoToElementState (this, "Searching", useTransitions: true);
                    }
                }
            }
        }

        private void GifEntryClicked (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            CopyImage ();
        }

        private void GifEntryKeyPressed (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                CopyImage ();
        }

        private void OnWindowKeyUp (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide ();
        }

        private void SearchBoxKeyPressed (object sender, KeyEventArgs e)
        {
            if (SearchStates.CurrentState == null || SearchStates.CurrentState.Name != "Searching")
                return;

            if (e.Key == Key.Down) {
                imageList.Focus ();
            }
        }

        private void TagEntryKeyPressed (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                AddNewGif (search.Text, tag.Text);
        }
    }
}
