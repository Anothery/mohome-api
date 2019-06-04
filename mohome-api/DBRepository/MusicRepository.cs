using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

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
            return db.Playlist.Include(t => t.Music).Where(pa => pa.UserId == userId);
        }


        public Playlist GetPlaylist(int playlistId, int userId)
        {
            return db.Playlist.Include(r => r.Music).FirstOrDefault(pa => pa.UserId == userId && pa.PlaylistId == playlistId);
        }

        public int DeletePlaylist(int playlistId, int userId)
        {
            int result = -1;
            var playlist = db.Playlist.Where(a => a.PlaylistId == playlistId && a.UserId == userId).FirstOrDefault();
            //User tries to remove another album
            if (playlist is null) return -1;

            var playlistMusic = db.Music.Where(r => r.PlaylistId == playlistId);
            foreach (var entity in playlistMusic)
            {
                entity.PlaylistId = null;
            };

            db.SaveChanges();

            db.Playlist.Remove(playlist);

            result = db.SaveChanges();

            return result;
        }

        public bool CheckPlaylistExists(int playlistId, int userId)
        {
            var playlist = db.Playlist.FirstOrDefault(r => r.PlaylistId == playlistId && r.UserId == userId);
            return playlist != null;
        }

        public int AddPlaylistCover(int playlistId, int userId, string coverPath)
        {
            var result = -1;
            var playlist = db.Playlist.Where(r => r.PlaylistId == playlistId && r.UserId == userId).FirstOrDefault();
            playlist.CoverPhotoPath = coverPath;
            result = db.SaveChanges();
            return result;
        }

        public int DeletePlaylistCover(int playlistId, int userId)
        {
            var result = -1;
            var playlist = db.Playlist.Where(r => r.PlaylistId == playlistId && r.UserId == userId).FirstOrDefault();
            playlist.CoverPhotoPath = null;
            result = db.SaveChanges();
            return result;
        }

    }
}
