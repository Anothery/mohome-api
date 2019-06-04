using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels.Playlist
{
    public class CoverFile
    {
        [Required]
        public IFormFile Photo { get; set; }
    }
}
