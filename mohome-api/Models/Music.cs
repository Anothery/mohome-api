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
        public string MusicUrl { get; set; }

        public int UserId { get; set; }
    }
}
