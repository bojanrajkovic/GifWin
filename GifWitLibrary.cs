using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
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

    internal class GifWitLibraryEntry
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        // Ugh this is dumb.
        [JsonProperty("keywords")]
        public string KeywordString { get; set; }

        [JsonIgnore]
        public string[] Keywords
        {
            get { return KeywordString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries); }
            set { KeywordString = string.Join(" ", value); }
        }
    }
}
