using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    }
}
