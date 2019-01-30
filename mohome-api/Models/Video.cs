using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Video
    {
        public int VideoId { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set;  }
        public int UserId { get; set; }
    }
}
