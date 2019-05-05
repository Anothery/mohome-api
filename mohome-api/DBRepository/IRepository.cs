using Models;
using System;
using System.Collections.Generic;


namespace DBRepository
{
    public interface IRepository
    {
        //profile
        IEnumerable<Profile> GetProfiles();
        Profile GetProfile(string email);
        Profile GetProfile(string email, string password);
        bool CheckProfileExists(string email);

        //user
        bool AddNewUser(string email, string password, string name);

        //photo
        int CreateAlbum(string name, string description, int userId);
        bool DeleteAlbum(int albumId);
        IEnumerable<PhotoAlbum> GetPhotoAlbums(int userId);

        //refreshToken
        void AddRefreshToken(string token, int userId, DateTime creationDate, DateTime expirationDate);
        bool CheckRefreshToken(string token, int userId);
        void DeleteRefreshToken(string token, int userId);
        Profile GetProfile(int userId);
    }
}
