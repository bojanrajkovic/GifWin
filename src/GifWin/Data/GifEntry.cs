using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GifWin.Data
{
    [Table("gifs")]
    public class GifEntry
    {
        public GifEntry()
        {
            Tags = new List<GifTag>();
            Usages = new List<GifUsage>();
        }

        public int Id { get; set; }
        [Required]
        public string Url { get; set; }
        public DateTimeOffset? AddedAt { get; set; }
        public DateTimeOffset? LastUsed { get; set; }
        public int UsedCount { get; set; } = 0;

        public virtual ICollection<GifTag> Tags { get; set; }
        public virtual ICollection<GifUsage> Usages { get; set; }
    }
}
