using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace GifWin.Core
{
    [JsonObject]
    sealed class GifWitLibrary : IEnumerable<GifWitLibraryEntry>, IReadOnlyList<GifWitLibraryEntry>
    {
        public IEnumerator<GifWitLibraryEntry> GetEnumerator () =>
            Images.AsReadOnly ().GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator () =>
            GetEnumerator ();

        [JsonProperty ("version")]
        public int Version { get; set; }

        [JsonProperty ("images")]
        internal List<GifWitLibraryEntry> Images { get; set; }

        public string LibrarySourcePath { get; set; }

        public int Count => Images.Count;

        public GifWitLibraryEntry this[int index] => Images[index];

        internal static async Task<GifWitLibrary> LoadFromFileAsync (string path)
        {
            using (StreamReader reader = new StreamReader (File.Open (path, FileMode.Open))) {
                string content = await reader.ReadToEndAsync ().ConfigureAwait (false);
                var lib = JsonConvert.DeserializeObject<GifWitLibrary> (content);
                lib.LibrarySourcePath = path;
                return lib;
            }
        }
    }
}
