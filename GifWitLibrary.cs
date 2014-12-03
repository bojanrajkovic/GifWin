using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GifWin
{
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

        public int Version { get; set; }
        internal List<GifWitLibraryEntry> Images { get; set; }
    }

    internal class GifWitLibraryEntry
    {
        public string Url { get; set; }
        public string[] Keywords { get; set; }
    }
}
