using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GifWin.Data
{
    [Table("tags")]
    public class GifTag
    {
        public int Id { get; set; }
        [Required]
        public string Tag { get; set; }
        
        public virtual GifEntry Gif { get; set; }
    }
}
