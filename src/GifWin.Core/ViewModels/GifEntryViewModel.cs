using GifWin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GifWin.ViewModels
{
    public class GifEntryViewModel : ViewModelBase
    {
        Task<string> cachedUri;

        public GifEntryViewModel (GifEntry entry)
        {
            if (entry == null) {
                throw new ArgumentNullException (nameof (entry));
            }

            Id = entry.Id;
            Url = entry.Url;
            FirstFrame = entry.FirstFrame;
            Keywords = entry.Tags.Select (t => t.Tag).ToArray ();
            Width = entry.Width;
            Height = entry.Height;
        }

        public byte[] FirstFrame { get; }
        public int Id { get; }
        public string Url { get; }
        public int Width { get; }
        public int Height { get; }

        public string KeywordString => string.Join (" ", Keywords);

        public IEnumerable<string> Keywords { get; }
    }
}