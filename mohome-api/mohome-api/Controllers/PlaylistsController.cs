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
using Models;
using System.IO;
using mohome_api.Infrastructure;
using System.Drawing;

namespace mohome_api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private IMusicRepository db;
        private PhotoHelper helper;
        private string COVER_STORAGE = LOCAL_STORAGE + PLAYLIST_COVER_PATH;
        private string MUSIC_STORAGE = LOCAL_STORAGE + MUSIC_PATH;

        public PlaylistsController(IMusicRepository db, PhotoHelper helper)
        {
            this.db = db;
            this.helper = helper;
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

            foreach (Playlist list in playlists)
            {
                var model = new PlaylistModel();
                model.PlaylistId = list.PlaylistId;
                model.Created = list.Created;
                model.Description = list.Description;
                model.Name = list.Name;
                model.CoverPhoto = list.CoverPhotoPath is null ? null : helper.GetBase64Image(COVER_STORAGE + list.CoverPhotoPath);
                model.MusicCount = list.Music.Count;
                playlistsVM.Add(model);
            }

            return Ok(new { response = playlistsVM });
        }

        /// <summary>
        /// Returns a playlist 
        /// </summary>
        /// <response code="500">Internal server error</response> 
        /// <response code="404">Playlist not found</response>
        /// <response code="401">UserId is undefined</response>

        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [Route("{playlistId}")]
        [HttpGet, Authorize]
        [UserActionFilter]
        public IActionResult GetPlaylist(int playlistId)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var playlist = db.GetPlaylist(playlistId, userId);

            if (playlist is null)
            {
                return NotFound(new { error = 404 });
            }


            PlaylistModel playlistViewModel = new PlaylistModel()
            {
                PlaylistId = playlist.PlaylistId,
                CoverPhoto = playlist.CoverPhotoPath is null ?  null : helper.GetBase64Image(COVER_STORAGE + playlist.CoverPhotoPath),
                Created = playlist.Created,
                Name = playlist.Name,
                Description = playlist.Description,
                MusicCount = playlist.Music.Count
            };

            return Ok(new { response = playlistViewModel });
        }

        /// <summary>
        /// Deletes a specific playlist
        /// </summary>
        ///  <response code="500">Failed to delete playlist. Try again</response>  
        ///  <response code="403">Unauthorized action</response>  
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="500">Internal server error</response> 

        [Route("{playlistId}")]
        [HttpDelete, Authorize]
        [ModelActionFilter]
        [UserActionFilter]
        [ProducesResponseType(500)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        public IActionResult DeletePlaylist(int playlistId)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var result = db.DeletePlaylist(playlistId, userId);


            return Ok(new { response = result });
        }



        /// <summary>
        /// Uploads a cover
        /// </summary>
        ///  <response code="500">Uploading error</response>  
        ///  <response code="400">Wrong size</response>  
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="500">Internal server error</response> 

        [Route("cover/{playlistId}")]
        [ModelActionFilter]
        [UserActionFilter]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        [HttpPut, Authorize]
        public IActionResult UploadCoverPhoto(int playlistId, [FromForm] CoverFile file)
        {
            var photo = file.Photo;
            var sizeMB = photo.Length / 1024 / 1000;
            if (sizeMB > 10)
            {
                return StatusCode(400, new ErrorDetails()
                {
                    errorId = ErrorList.WrongSize.Id,
                    errorMessage = ErrorList.WrongSize.Description
                });
            }

            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            if (!db.CheckPlaylistExists(playlistId, userId))
            {
                return StatusCode(403, new ErrorDetails()
                { errorId = ErrorList.UnauthorizedAction.Id, errorMessage = ErrorList.UnauthorizedAction.Description}.ToString());
            }

            var extension = Path.GetExtension(photo.FileName).Replace(".", "");

            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new ErrorDetails()
                { errorId = ErrorList.InputDataError.Id, errorMessage = ErrorList.InputDataError.Description }.ToString());
            };

            var imageName = Guid.NewGuid().ToString().Replace("-", "");
            var thumbName = $"{imageName}_cover.{extension}";

            var additionalPath = $"/{userId}";
            var newThumbPath = $"{additionalPath}/{thumbName}";

            db.AddPlaylistCover(playlistId, userId, newThumbPath);

            var thumbPath = COVER_STORAGE + newThumbPath;

            var resizedThumb = helper.ResizeImage(Image.FromStream(photo.OpenReadStream()));

            if (!Directory.Exists(Path.GetDirectoryName(thumbPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(thumbPath));

            helper.SaveImage(resizedThumb, thumbPath);

            var base64Photo = Convert.ToBase64String(System.IO.File.ReadAllBytes(thumbPath));
            return Ok(new { response = new { coverPhoto = base64Photo } });
        }


        /// <summary>
        /// Deletes a cover
        /// </summary>
        ///  <response code="500">Server error</response>  
        ///  <response code="403">Your action is not allowed</response>  

        [Route("cover/{playlistId}")]
        [UserActionFilter]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [HttpDelete, Authorize]
        public IActionResult DeleteCoverPhoto(int playlistId)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            if (!db.CheckPlaylistExists(playlistId, userId))
            {
                return StatusCode(403, new ErrorDetails()
                { errorId = ErrorList.UnauthorizedAction.Id, errorMessage = ErrorList.UnauthorizedAction.Description }.ToString());
            }

            var result = db.DeletePlaylistCover(playlistId, userId);

            return Ok(new { response = result });
        }
    }
}