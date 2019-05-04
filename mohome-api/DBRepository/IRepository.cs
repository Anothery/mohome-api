using Models;
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
    }
}
