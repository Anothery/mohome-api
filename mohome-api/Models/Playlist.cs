using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Playlist
    {
        public int PlaylistId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int UserId { get; set; }
        public virtual Profile Profile { get; set; }

        public string CoverPhotoPath { get; set; }

        public List<Music> Music { get; set; }
    }
}
