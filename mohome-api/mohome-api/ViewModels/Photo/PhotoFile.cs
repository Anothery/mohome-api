using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels
{
    public class PhotoFile
    {
        [Required]
        public IFormFile Photo { get; set; }

        public int AlbumId { get; set; }
    }
}
