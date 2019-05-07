using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels
{
    public class ChangeAlbumModel
    {
        [Required]
        public int albumId { get; set; }
        [Required]
        public string AlbumName { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
    }
}
