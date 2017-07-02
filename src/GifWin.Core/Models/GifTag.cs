using JetBrains.Annotations;

namespace GifWin.Core.Models
{
    [PublicAPI]
    public sealed class GifTag
    {
        public int Id { get; set; }
        public int GifId { get; set; }
        public string Tag { get; set; }
    }
}
