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
        Profile GetProfile(int userId);
        bool CheckProfileExists(string email);

        //user
        bool AddNewUser(string email, string password, string name);

        //photo
        int CreateAlbum(string name, string description, int userId);
        int DeleteAlbum(int albumId, int userId);
        IEnumerable<PhotoAlbum> GetPhotoAlbums(int userId);
        void AddPhoto(string name, int userId, int? albumid, string path);
        string GetPhotoPath(int userId, string photoName);
        IEnumerable<Photo> GetPhotosByAlbum(int userId, int albumId);

        //refreshToken
        void AddRefreshToken(string token, int userId, DateTime creationDate, DateTime expirationDate);
        bool CheckRefreshToken(string token, int userId);
        void DeleteRefreshToken(string token, int userId);

    }
}
