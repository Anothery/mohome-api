using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mohome_api.API_Errors;
using mohome_api.Filters;
using mohome_api.ViewModels;
using static mohome_api.Options;

namespace mohome_api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PhotoAlbumsController : ControllerBase    
    {
        private IPhotoRepository db;
        private string storage = LOCAL_STORAGE + PHOTO_PATH;

        public PhotoAlbumsController(IPhotoRepository db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns an album 
        /// </summary>
        /// <response code="500">Internal server error</response> 
        /// <response code="404">Album not found</response>
        /// <response code="401">UserId is undefined</response>

        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [Route("{albumId}")]
        [HttpGet, Authorize]
        [UserActionFilter]
        public IActionResult GetAlbum(int albumId)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var album = db.GetPhotoAlbum(albumId, userId);

            if (album is null)
            {
                return NotFound(new { error = 404 });
            }

            string coverPhotoName = null;
            if (album.CoverPhotoId is null)
            {
                var photoName = db.GetLastAlbumPhotoName(album.AlbumId, userId);
                if (photoName != null)
                {
                    coverPhotoName = photoName;
                }
            }
            else { coverPhotoName = db.GetPhotoName(userId, (int)album.CoverPhotoId); };

            PhotoAlbumModel albumView = new PhotoAlbumModel()
            {
                AlbumId = album.AlbumId,
                CoverPhotoName = coverPhotoName,
                Created = album.Created,
                PhotoCount = album.Photos.Count,
                Description = album.Description,
                Name = album.Name
            };

            return Ok(new { response = albumView });
        }

        /// <summary>
        /// Returns album list for current user
        /// </summary>
        /// <response code="500">Internal server error</response> 

        [ProducesResponseType(500)]
        [HttpGet, Authorize]
        [UserActionFilter]
        public IActionResult GetAlbums()
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var albums = db.GetPhotoAlbums(userId);

            List<PhotoAlbumModel> albumsvm = new List<PhotoAlbumModel>();

            foreach (var album in albums)
            {
                var model = new PhotoAlbumModel();
                model.AlbumId = album.AlbumId;
                model.Created = album.Created;
                model.Description = album.Description;
                model.Name = album.Name;
                model.PhotoCount = album.Photos.Count;
                if (album.CoverPhotoId is null)
                {
                    var photoName = db.GetLastAlbumPhotoName(album.AlbumId, userId);
                    if (photoName != null)
                    {
                        model.CoverPhotoName = photoName;
                    }

                }
                else { model.CoverPhotoName = db.GetPhotoName(userId, (int)album.CoverPhotoId); };
                albumsvm.Add(model);
            }

            return Ok(new { response = albumsvm });
        }


        /// <summary>
        /// Creates a new album
        /// </summary>
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="500">Internal server error</response> 

        [UserActionFilter]
        [HttpPost, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult CreateAlbum([FromBody] CreateAlbumModel model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var newAlbumId = db.CreateAlbum(model.AlbumName, model.Description, userId);

            if (newAlbumId <= 0)
            {
                return StatusCode(500, new ErrorDetails() { errorId = ErrorList.UnknownError.Id, errorMessage = ErrorList.UnknownError.Description });
            }

            return Ok(new { response = new { albumId = newAlbumId } });
        }

        /// <summary>
        /// Updates an album
        /// </summary>
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="500">Internal server error</response> 
        [Route("{albumId}")]
        [UserActionFilter]
        [HttpPut, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult ChangeAlbum(int albumId, [FromBody] ChangeAlbumModel model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var result = db.ChangeAlbum(albumId, model.AlbumName, model.Description, model.CoverPhotoName, userId);

            if (result < 0)
            {
                return StatusCode(500, new ErrorDetails() { errorId = ErrorList.UnknownError.Id, errorMessage = ErrorList.UnknownError.Description });
            }
            else if (result == 0)
            {
                return Ok(new { response = 0 });
            }

            return Ok(new { response = 1});
        }

        /// <summary>
        /// Deletes a specific album
        /// </summary>
        ///  <response code="500">Failed to delete album. Try again</response>  
        ///  <response code="403">Unauthorized action</response>  
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="500">Internal server error</response> 

        [Route("{albumId}")]
        [HttpDelete, Authorize]
        [ModelActionFilter]
        [UserActionFilter]
        [ProducesResponseType(500)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        public IActionResult DeleteAlbum(int albumId)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);



            /* Delete photos from storage             
            var photosToDelete = db.GetPhotosByAlbum(userId, albumId);
            foreach (var photo in photosToDelete)
            {
                if (System.IO.File.Exists(storage + photo.Path))
                {
                    System.IO.File.Delete(storage + photo.Path);
                }
            }
            */

            var result = db.DeleteAlbum(albumId, userId);

            switch (result)
            {
                case -1:
                    return StatusCode(403, new ErrorDetails()
                    {
                        errorId = ErrorList.UnauthorizedAction.Id,
                        errorMessage = ErrorList.UnauthorizedAction.Description
                    });
                case 0: return Ok(new { response = 0 });
            }

            return Ok(new { response = 1 });
        }



    }
}