using System;

namespace GifWin.Core.Models
{
    public sealed class GifUsage
    {
        public int Id { get; set; }
        public int GifId { get; set; }
        public string UsedAt { get; set; }
        public string SearchTerm { get; set; }
    }
}
