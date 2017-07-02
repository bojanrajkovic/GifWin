using System;

using Newtonsoft.Json;

namespace GifWin.Core.GifWit
{
    sealed class GifWitLibraryEntry
    {
        [JsonProperty ("url")]
        public Uri Url { get; set; }

        // Ugh this is dumb.
        [JsonProperty ("keywords")]
        public string KeywordString { get; set; }
    }
}