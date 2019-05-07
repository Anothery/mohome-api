using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels
{
    public class PhotosViewModel
    {
        public int PhotoId { get; set; }
        public DateTime Created { get; set; }
        public string Caption { get; set; }
        public string Extension { get; set; }
        public string Name { get; set; }
        public int? AlbumId { get; set; }
    }
}
