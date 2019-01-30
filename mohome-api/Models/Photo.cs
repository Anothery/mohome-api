using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public DateTime Created { get; set; }
        public string PhotoUrl { get; set; }
        public string Caption { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual PhotoAlbum Album { get; set; }
    }
}
