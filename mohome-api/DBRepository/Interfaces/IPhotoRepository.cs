using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBRepository
{
    public interface IPhotoRepository
    {
        int CreateAlbum(string name, string description, int userId);
        int DeleteAlbum(int albumId, int userId);
        IEnumerable<PhotoAlbum> GetPhotoAlbums(int userId);
        PhotoAlbum GetPhotoAlbum(int albumId, int userId);
        string GetLastAlbumPhotoName(int albumId, int userId);
        void AddPhoto(string name, int userId, int? albumid, string path, string thumbPath, string thumbName);
        Photo GetPhoto(int userId, string photoName);
        string GetPhotoName(int userId, int? photoId);
        IEnumerable<Photo> GetPhotosByAlbum(int userId, int albumId, int? offset);
        IEnumerable<Photo> GetAllPhotos(int userId, int? offset);
        int ChangeAlbum(int albumId, string name, string description, string coverPhotoName, int userId);
        int ChangePhotoDescription(string photoName, string description, int userId);
        bool CheckAlbumExists(int albumId, int userId);
        int DeletePhoto(string photoName, int userId);
    }
}
