using JetBrains.Annotations;

using GifWin.Core.Models;

namespace GifWin.Core.Messages
{
    [PublicAPI]
    public sealed class GifAdded
    {
        public GifEntry NewGif { get; set; }
    }
}
