using System;
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
            try
            {
                return db.Profile;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        public Profile GetProfile(string email)
        {
            try
            {
                return db.Profile.Include(r => r.Role).Where(r => r.Email == email).FirstOrDefault();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public Profile GetProfile(string email, string password)
        {
            try
            {
                return db.Profile.Include(r => r.Role).Where(r => r.Email == email && r.Password == password).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool CheckProfileExists(string email)
        {
            try
            { 
            return db.Profile.FirstOrDefault(r => r.Email == email) is null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool AddNewUser(string email, string password, string name)
        {
            try
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int CreateAlbum(string name, string description, int userId)
        {
            try
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
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool DeleteAlbum(int albumId)
        {
            try
            {
                var album = db.PhotoAlbum.Where(a => a.AlbumId == albumId).FirstOrDefault();
                db.PhotoAlbum.Remove(album);
                var result = db.SaveChanges();
                return result > 0;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<PhotoAlbum> GetPhotoAlbums(int userId)
        {
            try
            {
                return db.PhotoAlbum.Where(pa => pa.UserId == userId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
