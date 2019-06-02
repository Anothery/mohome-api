using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBRepository
{
    public class IdentityRepository : IIdentityRepository
    {
        private MohomeContext db;

        public IdentityRepository(MohomeContext db)
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
    }
}
