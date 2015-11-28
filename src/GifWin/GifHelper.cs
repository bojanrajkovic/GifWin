using GifWin.Data;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GifWin
{
    class GifHelper
    {
        /// <returns>Path to the local file</returns>
        internal static Task<string> GetOrMakeSavedAsync(int gifId, string entryUrl)
        {
            return Task.Run(async () => {
                SHA1 sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(entryUrl));
                string readableHash = GetReadableHash(hash);

                DirectoryInfo storage = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GifWin", "Cache"));
                storage.Create();

                FileInfo file = new FileInfo(Path.Combine(storage.FullName, readableHash + ".gif"));
                if (!file.Exists) {
                    var client = new WebClient();
                    byte[] contents = await client.DownloadDataTaskAsync(entryUrl).ConfigureAwait(false);
                    using (FileStream stream = file.OpenWrite()) {
                        await stream.WriteAsync(contents, 0, contents.Length).ConfigureAwait(false);
                    }

                    // If we're redownloading the GIF for any reason, also update frame data. Do this
                    // off-thread, because we don't want to hold _this_ thread up any longer.
                    Task.Run(async () => {
                        using (var helper = new GifWinDatabaseHelper()) {
                            var frameData = GetFrameData(file.FullName, frameNumber: 0);
                            await helper.UpdateSavedFirstFrameDataAsync(gifId, frameData).ConfigureAwait(false);
                        }
                    });
                }

                return file.FullName;
            });
        }

        internal static byte[] GetFrameData(string gifPath, int frameNumber)
        {
            var fs = File.OpenRead(gifPath);
            var decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (frameNumber > decoder.Frames.Count) {
                throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Frame number {frameNumber} is greater than frame count {decoder.Frames.Count}");
            }

            var frame = decoder.Frames[frameNumber];

            // Now convert it to PNG
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);

            byte[] pngData;
            using (var ms = new MemoryStream()) {
                encoder.Save(ms);
                pngData = ms.ToArray();
                return pngData;
            }
        }

        private static string GetReadableHash(byte[] hash)
        {
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                builder.Append(hash[i].ToString("X2"));

            return builder.ToString();
        }
    }
}
