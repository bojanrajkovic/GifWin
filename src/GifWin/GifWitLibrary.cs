using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        internal static GifWitLibrary LoadFromFile(string path)
        {
            return JsonConvert.DeserializeObject<GifWitLibrary>(File.ReadAllText(path));
        }
    }
}
