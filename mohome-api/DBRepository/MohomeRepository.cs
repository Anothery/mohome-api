using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Models;


namespace DBRepository
{
    // TODO: Divide repositories by section
    public class MohomeRepository : IRepository
    {
        private MohomeContext db;

        public MohomeRepository(MohomeContext db)
        {
            this.db = db;
        }

        public IEnumerable<Profile> GetProfiles()
        {
            return db.Profile;
        }

        public Profile GetProfile(int userId)
        {
            return db.Profile.Include(r => r.Role).Where(r => r.UserId == userId).FirstOrDefault();
        }

        public Profile GetProfile(string email)
        {
            return db.Profile.Include(r => r.Role).Where(r => r.Email == email).FirstOrDefault();
        }

        public Profile GetProfile(string email, string password)
        {
            return db.Profile.Include(r => r.Role).Where(r => r.Email == email && r.Password == password).FirstOrDefault();
        }


        public bool CheckProfileExists(string email)
        {
            return !(db.Profile.FirstOrDefault(r => r.Email == email) is null);
        }

        public bool AddNewUser(string email, string password, string name)
        {
            var newUser = new Profile()
            {
                Email = email,
                Name = name,
                Password = password
            };

            db.Profile.Add(newUser);

            var result = db.SaveChanges();

            if (result > 0) return true;
            return false;
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
                foreach(var album in albums)
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


        public async void AddRefreshToken(string token, int userId, DateTime creationDate, DateTime expirationDate)
        {
            var model = new RefreshToken
            {
                UserId = userId,
                Token = token,
                CreationDate = creationDate,
                ExpirationDate = expirationDate
            };

            await db.RefreshToken.AddAsync(model);
            await db.SaveChangesAsync();
        }

        public bool CheckRefreshToken(string token, int userId)
        {
            var dbToken = db.RefreshToken.Where(t => t.Token == token && t.UserId == userId).FirstOrDefault();
            if (dbToken is null) { return false; }
            int checkExpiration = DateTime.Compare(DateTime.Now, dbToken.ExpirationDate);
            if (checkExpiration > 0)
            {
                db.RefreshToken.Remove(dbToken);
                db.SaveChanges();
                return false;
            }
            //if valid
            return true;
        }

        public void DeleteRefreshToken(string token, int userId)
        {
            var dbToken = db.RefreshToken.Where(t => t.Token == token && t.UserId == userId).FirstOrDefault();
            db.RefreshToken.Remove(dbToken);
            db.SaveChanges();
        }

        public bool CheckAlbumExists(int albumId, int userId)
        {
            var album = db.PhotoAlbum.FirstOrDefault(r => r.AlbumId == albumId && r.UserId == userId);
            return album != null;
        }
        public void AddPhoto(string name, int userId, int? albumid, string path)
        {
            var model = new Photo
            {
                Created = DateTime.Now,
                AlbumId = albumid == 0 ? null : albumid,
                UserId = userId,
                Path = path,
                Name = name
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



        public IEnumerable<Photo> GetPhotosByAlbum(int userId, int albumId)
        {
            var photo = db.Photo.Where(r => r.UserId == userId && r.AlbumId == albumId).OrderBy(r => r.Created);
            return photo;
        }

        public IEnumerable<Photo> GetAllPhotos(int userId)
        {
            var photo = db.Photo.Where(r => r.UserId == userId).OrderBy(r => r.Created);
            return photo;
        }

        public int ChangePhotoDescription(string photoName, string description, int userId)
        {
            var photo = db.Photo.SingleOrDefault(r => r.UserId == userId && r.Name == photoName);

            photo.Caption = description;

            return db.SaveChanges();
        }

    }
}
