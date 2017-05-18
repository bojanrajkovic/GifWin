using System;
using System.Collections.Generic;

namespace GifWin.Core.Models
{
    public class GifEntry
    {
        public GifEntry()
        {
            Tags = new List<GifTag>();
            Usages = new List<GifUsage>();
        }

        public int Id { get; set; }
        public string Url { get; set; }
        public string AddedAt { get; set; }
        public string LastUsed { get; set; }
        public int UsedCount { get; set; } = 0;
        public byte[] FirstFrame { get; set; }
        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;

        public IEnumerable<GifTag> Tags { get; set; }
        public IEnumerable<GifUsage> Usages { get; set; }
    }
}
