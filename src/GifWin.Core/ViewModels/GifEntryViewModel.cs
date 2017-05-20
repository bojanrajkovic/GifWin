using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GifWin.Core.Models;

namespace GifWin.Core.ViewModels
{
    public class GifEntryViewModel
    {
        // Figure out how to populate this.
        Task<string> cachedUri;

        public GifEntryViewModel (GifEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException (nameof (entry));

            Id = entry.Id;
            Url = entry.Url;
            FirstFrame = entry.FirstFrame;
            Keywords = entry.Tags.Select (t => t.Tag).ToArray ();
        }

        public byte[] FirstFrame { get; }
        public int Id { get; }
        public string Url { get; }

        public string KeywordString => string.Join (" ", Keywords);

        public IEnumerable<string> Keywords { get; }
    }
}