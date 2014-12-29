using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GifWin
{
    internal class GifWitLibraryEntry
    {
        private Uri url;

        [JsonProperty ("url")]
        public Uri Url
        {
            get;
            set;
        }

        // Ugh this is dumb.
        [JsonProperty("keywords")]
        public string KeywordString { get; set; }
    }
}