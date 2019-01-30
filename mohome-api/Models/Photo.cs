using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public int AlbumId { get; set; }
        public DateTime Created { get; set; }
        public string PhotoUrl { get; set; }
        public string Caption { get; set; }
    }
}
