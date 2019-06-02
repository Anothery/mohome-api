using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Music
    {
        public int MusicId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string MusicPath { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; }

        public int UserId { get; set; }
        public virtual Profile Profile { get; set; }

        public int? PlaylistId { get; set; }
        public virtual Playlist Playlist { get; set; }
    }
}
