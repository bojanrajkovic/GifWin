namespace GifWin
{
    static class StringExtensions
    {
        public static string OrIfBlank(this string self, string other)
        {
            return string.IsNullOrWhiteSpace (self) ? other : self;
        }
    }
}
