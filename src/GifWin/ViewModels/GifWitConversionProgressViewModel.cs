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

        public string LibraryFileName => Path.GetFileName (libraryToConvert.LibrarySourcePath);

        int converted;
        public int Converted
        {
            get { return converted; }
            set { converted = value; RaisePropertyChanged (); }
        }

        internal Task ConvertLibraryAsync ()
        {
            return Task.Run (async () => {
                using (var db = new GifWinDatabaseHelper ())
                    await db.ConvertGifWitLibraryAsync (libraryToConvert, this);
                GifHelper.StartPreCachingDatabase ();
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
            Converted = value;
            Progress = Math.Round ((double)Converted / TotalItemsToConvert * 100, 2);
        }

        internal bool DeleteConvertedLibrary ()
        {
            try {
                File.Delete (libraryToConvert.LibrarySourcePath);
                return true;
            } catch {
                return false;
            }
        }
    }
}
