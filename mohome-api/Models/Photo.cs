using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public DateTime Created { get; set; }
        public string Path { get; set; }
        public string Caption { get; set; }
        public string Name { get; set; }
        public string ThumbName { get; set; }
        public string ThumbPath { get; set; }
        public int UserId { get; set; }
        public virtual Profile Profile { get; set; }
        public int? AlbumId { get; set; }
        public virtual PhotoAlbum Album { get; set; }
    }
}
