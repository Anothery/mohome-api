using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBRepository
{
    public class MusicRepository : IMusicRepository
    {
        private MohomeContext db;

        public MusicRepository(MohomeContext db)
        {
            this.db = db;
        }

        public int CreatePlaylist(string name, string description, int userId)
        {

            var newPlaylist = new Playlist
            {
                Name = name,
                Description = description,
                UserId = userId
            };
            db.Playlist.Add(newPlaylist);
            db.SaveChanges();
            return newPlaylist.PlaylistId;
        }

        public IEnumerable<Playlist> GetPlaylists(int userId)
        {
            return db.Playlist.Where(pa => pa.UserId == userId);
        }
    }
}
