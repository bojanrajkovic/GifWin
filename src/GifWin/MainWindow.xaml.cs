using GifWin.Data;
using GifWin.Properties;
using Squirrel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;

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
            updateCheckTimer = new Timer (CheckForUpdatesAsync, null, TimeSpan.FromMilliseconds (0), TimeSpan.FromHours (24));
        }

        async void CheckForUpdatesAsync (object sender)
        {
#if !DEBUG
            try {
                using (var updateMgr = new UpdateManager (Properties.Resources.UpdatePath)) {
                    var re = await updateMgr.UpdateApp ();
                }
            } catch (Exception ex) {
                DirectoryInfo storage = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "GifWin", "Logs"));
                storage.Create ();
                var logPath = Path.Combine (storage.ToString (), $"Exception-Squirrel-{DateTimeOffset.UtcNow.ToUnixTimeSeconds ()}.log");
                using (var sw = new StreamWriter (logPath)) {
                    sw.WriteLine (ex.ToString ());
                }
            }
#endif
        }

        public new void Show ()
        {
            base.Show ();
            search.Focus ();
        }

        public new void Hide ()
        {
            search.Clear ();
            tag.Clear ();
            base.Hide ();
        }

        void AddNewGif (string gifUrl, string tags)
        {
            var helper = new GifWinDatabaseHelper ();
            var tagsArray = tags.Split (',').Select (tag => tag.Trim ()).ToArray ();
            helper.AddNewGifAsync (gifUrl, tagsArray).ContinueWith (t => {
                if (t.IsFaulted) {
                    MessageBox.Show ("Could not save GIF to database.", "Could not save GIF", MessageBoxButton.OK, MessageBoxImage.Error);
                    GlobalHelper.PromptForDebuggerLaunch (t.Exception);
                } else {
                    GifHelper.GetOrMakeSavedAsync (t.Result.Id, t.Result.Url, t.Result.FirstFrame);
                    Dispatcher.Invoke (Hide);
                }
                helper.Dispose ();
            });
        }

        void CopyImage (GifEntryViewModel entry = null)
        {
            entry = entry ?? (GifEntryViewModel)imageList.SelectedItem;

            if (entry == null && imageList.Items.Count == 1)
                entry = (GifEntryViewModel)imageList.Items[0];

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

        void DeleteImage ()
        {
            var entry = (GifEntryViewModel)imageList.SelectedItem;
            if (entry == null) {
                return;
            }

            Task.Run (async () => {
                using (var helper = new GifWinDatabaseHelper ()) {
                    try {
                        await helper.DeleteGifAsync (entry.Id);
                    } catch (Exception e) {
                        MessageBox.Show ($"Couldn't delete GIF: {e.InnerException.Message}.", "Failed");
                        GlobalHelper.PromptForDebuggerLaunch (e);
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

        void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            // We listen to this instead of the input text so we can be sure the VM has had a chance to filter before we expand.
            if (e.PropertyName == "FilterText") {
                if (search.Text == String.Empty)
                    VisualStateManager.GoToElementState (this, "NotSearching", useTransitions: true);
                else {
                    if (Uri.IsWellFormedUriString (search.Text, UriKind.Absolute)) {
                        VisualStateManager.GoToElementState (this, "Adding", useTransitions: true);
                    } else {
                        VisualStateManager.GoToElementState (this, "Searching", useTransitions: true);
                    }
                }
            }
        }

        void GifEntryClicked (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            var element = InputHitTest (e.GetPosition (this)) as FrameworkElement;
            var entry = element?.DataContext as GifEntryViewModel;
            if (entry == null)
                return;

            CopyImage (entry);
        }

        void GifEntryKeyPressed (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                CopyImage ();

            if (e.Key == Key.Delete || e.Key == Key.Back) {
                DeleteImage ();
            }
        }

        void OnWindowKeyUp (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide ();
        }

        void SearchBoxKeyPressed (object sender, KeyEventArgs e)
        {
            if (SearchStates.CurrentState == null || SearchStates.CurrentState.Name != "Searching")
                return;

            if (e.Key == Key.Return || e.Key == Key.Enter)
                CopyImage ();

            if (e.Key == Key.Down) {
                imageList.Focus ();
            }
        }

        void TagEntryKeyPressed (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                AddNewGif (search.Text, tag.Text);
        }
    }
}
