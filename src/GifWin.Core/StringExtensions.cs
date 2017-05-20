namespace GifWin.Core
{
    static class StringExtensions
    {
        public static string OrIfBlank(this string self, string other) =>
            string.IsNullOrWhiteSpace (self) ? other : self;
    }
}
