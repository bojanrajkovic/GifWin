using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace GifWin
{
    [JsonObject]
    class GifWitLibrary : IEnumerable<GifWitLibraryEntry>
    {
        public IEnumerator<GifWitLibraryEntry> GetEnumerator()
        {
            return Images.AsReadOnly().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("images")]
        internal List<GifWitLibraryEntry> Images { get; set; }

        internal static async Task<GifWitLibrary> LoadFromFileAsync (string path)
        {
            using (StreamReader reader = new StreamReader (File.Open (path, FileMode.Open))) {
                string content = await reader.ReadToEndAsync().ConfigureAwait (false);
                return JsonConvert.DeserializeObject<GifWitLibrary> (content);
            }
        }

        /// <returns>Path to the local file</returns>
        internal static Task<string> GetOrMakeSavedAsync (GifWitLibraryEntry entry)
        {
            return Task.Run (async () => {
                SHA1 sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash (Encoding.Unicode.GetBytes (entry.Url.ToString()));
                string readableHash = GetReadableHash (hash);

                DirectoryInfo storage = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "GifWin", "Cache"));
                storage.Create();

                FileInfo file = new FileInfo (Path.Combine (storage.FullName, readableHash + ".gif"));
                if (!file.Exists) {
                    var client = new WebClient();
                    byte[] contents = await client.DownloadDataTaskAsync (entry.Url).ConfigureAwait (false);
                    using (FileStream stream = file.OpenWrite()) {
                        await stream.WriteAsync (contents, 0, contents.Length).ConfigureAwait (false);
                    }
                }

                return file.FullName;
            });
        }

        private static string GetReadableHash (byte[] hash)
        {
            StringBuilder builder = new StringBuilder (hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                builder.Append (hash[i].ToString ("X2"));

            return builder.ToString();
        }
    }
}
