using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBRepository;
using static mohome_api.Options;
using Microsoft.AspNetCore.Mvc;
using mohome_api.Filters;
using Microsoft.AspNetCore.Authorization;
using mohome_api.ViewModels.Playlist;
using mohome_api.ViewModels;
using mohome_api.API_Errors;

namespace mohome_api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private IMusicRepository db;
        private string storage = LOCAL_STORAGE + MUSIC_PATH;

        public PlaylistsController(IMusicRepository db)
        {
            this.db = db;
        }

        /// <summary>
        /// Creates a new music playlist
        /// </summary>
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="500">Internal server error</response> 

        [UserActionFilter]
        [HttpPost, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult CreatePlaylist([FromBody] CreatePlaylistModel model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var newPlaylistId = db.CreatePlaylist(model.Name, model.Description, userId);

            if (newPlaylistId <= 0)
            {
                return StatusCode(500, new ErrorDetails() { errorId = ErrorList.UnknownError.Id, errorMessage = ErrorList.UnknownError.Description });
            }

            return Ok(new { response = new { albumId = newPlaylistId } });
        }


        /// <summary>
        /// Returns music playlists for current user
        /// </summary>
        /// <response code="500">Internal server error</response> 

        [ProducesResponseType(500)]
        [HttpGet, Authorize]
        [UserActionFilter]
        public IActionResult GetPlayLists()
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var playlists = db.GetPlaylists(userId);

            List<PlaylistModel> playlistsVM = new List<PlaylistModel>();

            foreach (var list in playlists)
            {
                var model = new PlaylistModel();
                model.PlaylistId = list.PlaylistId;
                model.Created = list.Created;
                model.Description = list.Description;
                model.Name = list.Name;
                model.CoverPhotoURL = null;   
                playlistsVM.Add(model);
            }

            return Ok(new { response = playlistsVM });
        }

    }
}