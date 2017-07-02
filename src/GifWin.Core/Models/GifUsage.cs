using JetBrains.Annotations;

namespace GifWin.Core.Models
{
    [PublicAPI]
    public sealed class GifUsage
    {
        public int Id { get; set; }
        public int GifId { get; set; }
        public string UsedAt { get; set; }
        public string SearchTerm { get; set; }
        public string SearchProvider { get; set; }
    }
}
