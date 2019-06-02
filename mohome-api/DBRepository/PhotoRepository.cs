using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace DBRepository
{
    public class PhotoRepository : IPhotoRepository
    {
        private MohomeContext db;

        public PhotoRepository(MohomeContext db)
        {
            this.db = db;
        }

        public int CreateAlbum(string name, string description, int userId)
        {

            var newAlbum = new PhotoAlbum
            {
                Name = name,
                Description = description,
                UserId = userId
            };
            db.PhotoAlbum.Add(newAlbum);
            db.SaveChanges();
            return newAlbum.AlbumId;
        }

        public int ChangeAlbum(int albumId, string name, string description, string CoverPhotoName, int userId)
        {
            var album = db.PhotoAlbum.SingleOrDefault(r => r.AlbumId == albumId && r.UserId == userId);
            if (album is null) { return -1; }

            if (CoverPhotoName != null)
            {
                var photo = db.Photo.FirstOrDefault(r => r.Name == CoverPhotoName && r.UserId == userId);
                if (photo != null) { album.CoverPhotoId = photo.PhotoId; }
            }

            album.Description = description;
            album.Name = name;
            return db.SaveChanges();
        }


        public int DeleteAlbum(int albumId, int userId)
        {
            int result = -1;
            var album = db.PhotoAlbum.Where(a => a.AlbumId == albumId && a.UserId == userId).FirstOrDefault();
            //User tries to remove another album
            if (album is null) return -1;
            if (album.CoverPhotoId != null)
            {
                album.CoverPhotoId = null;
                db.SaveChanges();
            }

            using (TransactionScope tsTransScope = new TransactionScope())
            {

                db.Photo.RemoveRange(db.Photo.Where(r => r.AlbumId == albumId));
                db.PhotoAlbum.Remove(album);
                result = db.SaveChanges();
                tsTransScope.Complete();
            }

            return result;
        }


        public string GetLastAlbumPhotoName(int albumId, int userId)
        {
            var album = db.PhotoAlbum.SingleOrDefault(r => r.AlbumId == albumId && r.UserId == userId);

            if (album is null) { return null; }
            var lastPhoto = db.Photo.Where(r => r.AlbumId == albumId && r.UserId == userId).OrderByDescending(r => r.Created).FirstOrDefault();
            if (lastPhoto is null) { return null; };
            return lastPhoto.Name;
        }

        public int DeletePhoto(string photoName, int userId)
        {
            var photo = db.Photo.SingleOrDefault(r => r.Name == photoName && r.UserId == userId);

            if (photo is null) { return -1; }

            int result;

            using (TransactionScope tsTransScope = new TransactionScope())
            {
                var albums = db.PhotoAlbum.Where(r => r.CoverPhotoId == photo.PhotoId);
                foreach (var album in albums)
                {
                    album.CoverPhotoId = null;
                }
                db.Photo.Remove(photo);
                result = db.SaveChanges();
                tsTransScope.Complete();
            }

            return result;
        }

        public IEnumerable<PhotoAlbum> GetPhotoAlbums(int userId)
        {
            return db.PhotoAlbum.Include(r => r.Photos).Where(pa => pa.UserId == userId);
        }

        public PhotoAlbum GetPhotoAlbum(int albumId, int userId)
        {
            return db.PhotoAlbum.Include(r => r.Photos).FirstOrDefault(pa => pa.UserId == userId && pa.AlbumId == albumId);
        }




        public bool CheckAlbumExists(int albumId, int userId)
        {
            var album = db.PhotoAlbum.FirstOrDefault(r => r.AlbumId == albumId && r.UserId == userId);
            return album != null;
        }
        public void AddPhoto(string name, int userId, int? albumid, string path, string thumbPath, string thumbName)
        {
            var model = new Photo
            {
                Created = DateTime.Now,
                AlbumId = albumid == 0 ? null : albumid,
                UserId = userId,
                Path = path,
                Name = name,
                ThumbName = thumbName,
                ThumbPath = thumbPath
            };

            db.Photo.Add(model);
            db.SaveChanges();
        }

        public Photo GetPhoto(int userId, string photoName)
        {
            var photo = db.Photo.Where(r => r.UserId == userId && r.Name == photoName).FirstOrDefault();
            return photo;
        }

        public string GetPhotoName(int userId, int? photoId)
        {
            var photo = db.Photo.Where(r => r.UserId == userId && r.PhotoId == photoId).FirstOrDefault();
            if (photo is null) return null;
            return photo.Name;
        }



        public IEnumerable<Photo> GetPhotosByAlbum(int userId, int albumId, int? offset)
        {
            if (offset is null)
            {
                return db.Photo.Where(r => r.UserId == userId && r.AlbumId == albumId).OrderBy(r => r.Created);
            }
            else
            {
                var take = 20;
                return db.Photo.Where(r => r.UserId == userId && r.AlbumId == albumId).OrderBy(r => r.Created).Skip((int)offset).Take(take);
            }
        }

        public IEnumerable<Photo> GetAllPhotos(int userId, int? offset)
        {
            if (offset is null)
            {
                return db.Photo.Where(r => r.UserId == userId).OrderBy(r => r.Created);
            }
            else
            {
                var take = 20;
                return db.Photo.Where(r => r.UserId == userId).OrderBy(r => r.Created).Skip((int)offset).Take(take);
            }
        }

        public int ChangePhotoDescription(string photoName, string description, int userId)
        {
            var photo = db.Photo.SingleOrDefault(r => r.UserId == userId && r.Name == photoName);

            photo.Caption = description;

            return db.SaveChanges();
        }

    }
}
