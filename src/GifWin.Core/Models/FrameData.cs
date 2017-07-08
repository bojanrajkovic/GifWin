using JetBrains.Annotations;

namespace GifWin.Core.Models
{
    [PublicAPI]
    public sealed class FrameData
    {
        public int Height { get; set; }
        public byte[] PngImage { get; set; }
        public int Width { get; set; }
    }
}