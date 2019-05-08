using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels.Photo
{
    public class ChangePhotoDescription
    {
        [Required]
        public string photoName { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
    }
}
