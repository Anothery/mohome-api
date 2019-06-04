using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBRepository
{
    public interface IMusicRepository
    {
        int CreatePlaylist(string name, string description, int userId);
        int DeletePlaylist(int playlistId, int userId);
        IEnumerable<Playlist> GetPlaylists(int userId);
        Playlist GetPlaylist(int playlistId, int userId);
        bool CheckPlaylistExists(int playlistId, int userId);
        int AddPlaylistCover(int playlistId, int userId, string coverPath);
        int DeletePlaylistCover(int playlistId, int userId);
    }
}
