using GifWin.Data;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.Generic;

namespace GifWin
{
    class GifHelper
    {
        static Dictionary<int, Task<string>> CachedTasks;

        /// <returns>Path to the local file</returns>
        internal static Task<string> GetOrMakeSavedAsync (int gifId, string entryUrl)
        {
            if (CachedTasks != null && CachedTasks.ContainsKey (gifId)) {
                return CachedTasks[gifId];
            }

            return Task.Run (async () => {
                SHA1 sha1 = SHA1.Create ();
                byte[] hash = sha1.ComputeHash (Encoding.Unicode.GetBytes (entryUrl));
                string readableHash = GetReadableHash (hash);

                DirectoryInfo storage = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "GifWin", "Cache"));
                storage.Create ();

                FileInfo file = new FileInfo (Path.Combine (storage.FullName, readableHash + ".gif"));
                if (!file.Exists) {
                    var client = new WebClient ();
                    byte[] contents = await client.DownloadDataTaskAsync (entryUrl).ConfigureAwait (false);
                    using (FileStream stream = file.OpenWrite ()) {
                        await stream.WriteAsync (contents, 0, contents.Length).ConfigureAwait (false);
                    }

#pragma warning disable CS4014
                    // If we're redownloading the GIF for any reason, also update frame data. Do this
                    // off-thread, because we don't want to hold _this_ thread up any longer.
                    // The pragma warning disable is because the compiler is _really_ irritating about this,
                    // claiming we should await this.
                    Task.Run (async () => {
                        using (var helper = new GifWinDatabaseHelper ()) {
                            var frameData = GetFrameData (file.FullName, frameNumber: 0);
                            await helper.UpdateSavedFirstFrameDataAsync (gifId, frameData).ConfigureAwait (false);
                        }
                    });
#pragma warning restore CS4014
                }

                return file.FullName;
            });
        }

        internal static void StartPreCachingDatabase ()
        {
            var helper = new GifWinDatabaseHelper ();
            var gifs = helper.QueryGifs (e => true, e => new { e.Id, e.Url });
            CachedTasks = new Dictionary<int, Task<string>> ();
            foreach (var gif in gifs) {
                CachedTasks[gif.Id] = GetOrMakeSavedAsync (gif.Id, gif.Url);
            }
            Task.WhenAll (CachedTasks.Values).ContinueWith (t => {
                CachedTasks = null;
            });
        }

        internal static async Task ConvertGifWitLibraryToGifWinDatabaseAsync (string path)
        {
            using (var db = new GifWinDatabaseHelper ()) {
                try {
                    var lib = await GifWitLibrary.LoadFromFileAsync (path).ConfigureAwait (false);
                    var converted = await db.ConvertGifWitLibraryAsync (lib).ConfigureAwait (false);
                    MessageBox.Show ($"Converted {converted} items successfully!", "Conversion succeeded!");
                    var delete = MessageBox.Show ($"Do you want to delete the GifWit library file?", "Delete GifWit library?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (delete == MessageBoxResult.Yes) {
                        try {
                            File.Delete (path);
                        } catch (Exception e) {
                            MessageBox.Show ($"Could not delete library.gifwit ({e.Message}), you may have to delete it manually.", "Delete failed.");
                        }
                    }
                } catch (Exception e) {
                    MessageBox.Show ($"Conversion failed: {e.Message}.", "Conversion failed.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        internal static FrameData GetFrameData (string gifPath, int frameNumber)
        {
            var fs = File.OpenRead (gifPath);
            var decoder = BitmapDecoder.Create (fs, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (frameNumber > decoder.Frames.Count) {
                throw new ArgumentOutOfRangeException (nameof (frameNumber), $"Frame number {frameNumber} is greater than frame count {decoder.Frames.Count}");
            }

            var frame = decoder.Frames[frameNumber];

            // Now convert it to PNG
            var encoder = new PngBitmapEncoder ();
            encoder.Frames.Add (frame);

            byte[] pngData;
            using (var ms = new MemoryStream ()) {
                encoder.Save (ms);
                pngData = ms.ToArray ();
            }

            return new FrameData {
                PngImage = pngData,
                Width = frame.PixelWidth,
                Height = frame.PixelHeight,
            };
        }

        static string GetReadableHash (byte[] hash)
        {
            StringBuilder builder = new StringBuilder (hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                builder.Append (hash[i].ToString ("X2"));

            return builder.ToString ();
        }
    }
}
