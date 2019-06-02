using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels.Playlist
{
    public class PlaylistModel
    {
        public int PlaylistId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverPhotoURL { get; set; }
    }
}
