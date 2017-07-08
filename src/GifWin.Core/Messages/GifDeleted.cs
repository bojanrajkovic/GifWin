using JetBrains.Annotations;

namespace GifWin.Core.Messages
{
    [PublicAPI]
    public sealed class GifDeleted
    {
        public int DeletedGifId { get; set; }
    }
}
