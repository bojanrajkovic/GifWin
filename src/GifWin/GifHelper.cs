using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GifWin
{
    class GifHelper
    {
        /// <returns>Path to the local file</returns>
        internal static Task<string> GetOrMakeSavedAsync(string entryUrl)
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
