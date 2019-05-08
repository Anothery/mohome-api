using System;
using System.Collections.Generic;
using System.Linq;
using mohome_api.ViewModels;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using mohome_api.API_Errors;
using static mohome_api.Options;
using System.IO;
using mohome_api.Filters;
using Models;
using mohome_api.ViewModels.Photo;

namespace mohome_api.Controllers
{
    [Route("Api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private IRepository db;
        private string storage = LOCAL_STORAGE + PHOTO_PATH;



        public PhotoController(IRepository db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns album list for current user
        /// </summary>
        /// <response code="500">Internal server error</response> 

        [ProducesResponseType(500)]
        [Route("Album")]
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

        [Route("Album")]
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
        [Route("Album")]
        [UserActionFilter]
        [HttpPut, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult ChangeAlbum([FromBody] ChangeAlbumModel model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var newAlbumId = db.ChangeAlbum(model.albumId, model.AlbumName, model.Description, model.CoverPhotoName, userId);

            if (newAlbumId <= 0)
            {
                return StatusCode(500, new ErrorDetails() { errorId = ErrorList.UnknownError.Id, errorMessage = ErrorList.UnknownError.Description });
            }

            return Ok(new { response = new { albumId = newAlbumId } });
        }

        /// <summary>
        /// Deletes a specific album
        /// </summary>
        ///  <response code="500">Failed to delete album. Try again</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="500">Internal server error</response> 

        [Route("Album")]
        [HttpDelete, Authorize]
        [ModelActionFilter]
        [UserActionFilter]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        public IActionResult DeleteAlbum([FromBody] DeleteAlbumModel model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            // Delete photos from storage             
            var photosToDelete = db.GetPhotosByAlbum(userId, model.AlbumId);
            foreach (var photo in photosToDelete)
            {
                if (System.IO.File.Exists(storage + photo.Path))
                {
                    System.IO.File.Delete(storage + photo.Path);
                }
            }


            var result = db.DeleteAlbum(model.AlbumId, userId);

            var path = $"{storage}/{userId}/{model.AlbumId}";
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.Delete(Path.GetDirectoryName(path));

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

        /// <summary>
        /// Uploads a photo
        /// </summary>
        ///  <response code="500">Uploading error</response>  
        ///  <response code="400">Wrong size</response>  
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="500">Internal server error</response> 

        [ModelActionFilter]
        [UserActionFilter]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        [HttpPost, Authorize]
        public async Task<IActionResult> UploadPhoto([FromForm] PhotoFile file)
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
            int? albumId = file.AlbumId;

            if (!db.CheckAlbumExists((int)albumId, userId))
            {
                return StatusCode(403, new ErrorDetails()
                { errorId = ErrorList.UnauthorizedAction.Id, errorMessage = ErrorList.UnauthorizedAction.Description }.ToString());
            }

            var extension = Path.GetExtension(photo.FileName).Replace(".", "");

            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new ErrorDetails()
                { errorId = ErrorList.InputDataError.Id, errorMessage = ErrorList.InputDataError.Description }.ToString());
            };
            var newFilename = $"{DateTime.Now.ToString().GetHashCode().ToString("x")}.{extension}";
            var additionalPath = $"/{userId}/{newFilename}";

            db.AddPhoto(newFilename, userId, albumId, additionalPath);


            string path = storage + additionalPath;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }

            return Ok(new { response = new { fileName = newFilename } });
        }

        /// <summary>
        /// Returns base64 photo by photo name
        /// </summary>
        ///  <response code="500">Uploading error</response>  
        ///  <response code="500">Internal server error</response> 
        ///  <response code="404">File not found</response>  
        ///  <response code="401">Your user id is undefined</response>  

        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ResponseCache(Duration = 500)]
        [Route("{photoName}")]
        [HttpGet, Authorize]
        public IActionResult GetPhoto(string photoName)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var extension = Path.GetExtension(photoName).Replace(".", ""); ;

            var path = db.GetPhotoPath(userId, photoName);

            if (path is null || path == string.Empty)
            {
                return StatusCode(404, new ErrorDetails() { errorId = ErrorList.FileNotFound.Id, errorMessage = ErrorList.FileNotFound.Description });
            }
            return Ok(new { response = new { image = Convert.ToBase64String(System.IO.File.ReadAllBytes(storage + path)), imageType = $"image/{extension}" } });
        }



        /// <summary>
        /// Returns photos by albumId. If albumId is null, returns all photos
        /// </summary>
        ///  <response code="500">Internal server error</response>  
        ///  <response code="401">Your user id is undefined</response>  

        [HttpGet, Authorize]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [UserActionFilter]
        public IActionResult GetPhotos([FromQuery(Name = "albumId")] int? albumId)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            IEnumerable<Photo> photos;

            if (albumId is null)
            {
                photos = db.GetAllPhotos(userId);
            }
            else
            {
                photos = db.GetPhotosByAlbum(userId, (int)albumId);
            }

            List<PhotosViewModel> list = new List<PhotosViewModel>();
            foreach (var photo in photos)
            {
                var model = new PhotosViewModel
                {
                    Name = photo.Name,
                    AlbumId = photo.AlbumId,
                    Caption = photo.Caption,
                    Created = photo.Created,
                    URL = $"{PHOTO_STORAGE_PATH}/{photo.Name}"
                };
                list.Add(model);
            }
            return Ok(new { response = list });
        }

        /// <summary>
        /// Updates a photo description
        /// </summary>
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="500">Internal server error</response> 
        [UserActionFilter]
        [ModelActionFilter]
        [HttpPatch, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult ChangePhotoDescription([FromBody] ChangePhotoDescription model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var result = db.ChangePhotoDescription(model.photoName, model.Description, userId);

            if (result > 0) return Ok(new { response = 1 });

            throw new Exception("Unknown error");
        }

    }
}