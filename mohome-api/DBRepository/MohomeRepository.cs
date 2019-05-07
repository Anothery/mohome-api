﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public int DeleteAlbum(int albumId, int userId)
        {
                var album = db.PhotoAlbum.Where(a => a.AlbumId == albumId && a.UserId == userId).FirstOrDefault();
                //User tries to remove another album
                if (album is null) return -1;

                db.PhotoAlbum.Remove(album);
                var result = db.SaveChanges();
                return result;
        }

        public IEnumerable<PhotoAlbum> GetPhotoAlbums(int userId)
        {
                return db.PhotoAlbum.Where(pa => pa.UserId == userId);
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
                if (dbToken is null){return false; }
                int checkExpiration = DateTime.Compare(DateTime.Now, dbToken.ExpirationDate);
                if(checkExpiration > 0)
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

        public async void AddPhoto(string name, int userId, int? albumid,  string path)
        {
                var model = new Photo
                {
                    Created = DateTime.Now,
                    AlbumId = albumid == 0 ? null : albumid,
                    UserId = userId,
                    Path = path,
                    Name = name
                };

                await db.Photo.AddAsync(model);
                await db.SaveChangesAsync();
        }

        public string GetPhotoPath(int userId, string photoName)
        {
                var photo = db.Photo.Where(r => r.UserId == userId && r.Name == photoName).FirstOrDefault();
                if (photo is null) return null;
                return photo.Path;
        }



        public IEnumerable<Photo> GetPhotosByAlbum(int userId, int albumId)
        {
                var photo = db.Photo.Where(r => r.UserId == userId && r.AlbumId == albumId);
                return photo;
        }

    }
}
