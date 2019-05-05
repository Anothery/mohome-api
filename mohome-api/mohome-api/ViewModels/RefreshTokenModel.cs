using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels
{
    public class RefreshTokenModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
