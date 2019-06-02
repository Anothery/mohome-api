using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBRepository
{
    public interface IMusicRepository
    {
        int CreatePlaylist(string name, string description, int userId);
       // int DeletePlaylist(int playlistId, int userId);
        IEnumerable<Playlist> GetPlaylists(int userId);
       // Playlist GetPlaylist(int playlistId, int userId);
    }
}
