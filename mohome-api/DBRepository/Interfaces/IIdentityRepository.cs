using Models;
using System;
using System.Collections.Generic;

namespace DBRepository
{
    public interface IIdentityRepository
    {
        void AddRefreshToken(string token, int userId, DateTime creationDate, DateTime expirationDate);
        bool CheckRefreshToken(string token, int userId);
        void DeleteRefreshToken(string token, int userId);

        bool AddNewUser(string email, string password, string name);

        IEnumerable<Profile> GetProfiles();
        Profile GetProfile(string email);
        Profile GetProfile(string email, string password);
        Profile GetProfile(int userId);
        bool CheckProfileExists(string email);
    }
}
