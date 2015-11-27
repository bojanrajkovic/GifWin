using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GifWin.Data
{
    [Table("usages")]
    public class GifUsage
    {
        public int Id { get; set; }
        public DateTimeOffset UsedAt { get; set; }
        public virtual GifEntry Gif { get; set; }
        public string SearchTerm { get; set; }
    }
}
