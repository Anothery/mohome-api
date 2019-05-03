using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Models;


namespace DBRepository
{
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
    }
}
