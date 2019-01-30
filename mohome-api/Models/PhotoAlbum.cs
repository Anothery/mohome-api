using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class PhotoAlbum
    {
        public int AlbumId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }

        public int UserId { get; set; }
        public int CoverPhotoId { get; set; }
    }
}
