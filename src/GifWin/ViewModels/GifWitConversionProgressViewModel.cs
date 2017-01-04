using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GifWin.Data;

namespace GifWin.ViewModels
{
    class GifWitConversionProgressViewModel : ViewModelBase, IProgress<int>
    {
        GifWitLibrary libraryToConvert;
        GifWitConversionProgressWindow window;

        public GifWitConversionProgressViewModel (GifWitLibrary libraryToConvert, GifWitConversionProgressWindow window)
        {
            this.libraryToConvert = libraryToConvert;
            this.window = window;
        }

        int converted;
        public int Converted
        {
            get { return converted; }
            set { converted = value; RaisePropertyChanged (); }
        }

        internal void ConvertLibrary ()
        {
            Task.Run (async () => {
                using (var db = new GifWinDatabaseHelper ()) {
                    int converted;

                    try {
                        converted = await db.ConvertGifWitLibraryAsync (libraryToConvert, this);

                        window.Dispatcher.Invoke (() => {
                            MessageBox.Show ($"Converted {TotalItemsToConvert} entries successfully!", "Conversion succeeded!");
                            var delete = MessageBox.Show (
                                $"Do you want to delete the GifWit library file?",
                                "Delete GifWit library?",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question
                            );

                            if (delete == MessageBoxResult.Yes) {
                                try {
                                    File.Delete (libraryToConvert.LibrarySourcePath);
                                } catch (Exception e) {
                                    MessageBox.Show (
                                        $"Could not delete {Path.GetFileName (libraryToConvert.LibrarySourcePath)} ({e.Message}), you may have to delete it manually.",
                                        "Delete failed."
                                    );
                                }
                            }

                            window.Close ();
                        });
                    } catch (Exception e) {
                        window.Dispatcher.Invoke (() => MessageBox.Show ($"Something went wrong while converting GifWit library: {e.Message}.", "Conversion failed."));
                    }
                }
            });
        }

        double progress;
        public double Progress
        {
            get { return progress; }
            set { progress = value; RaisePropertyChanged (); }
        }

        public int TotalItemsToConvert => libraryToConvert.Count;

        public void Report (int value)
        {
            window.Dispatcher.Invoke (() => {
                Converted = value;
                Progress = Math.Round ((double)Converted / TotalItemsToConvert * 100, 2);
            });
        }
    }
}
