﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels.Photo
{
    public class PhotosWithAlbumModel
    {
        public DateTime Created { get; set; }
        public string Caption { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
    }
}
