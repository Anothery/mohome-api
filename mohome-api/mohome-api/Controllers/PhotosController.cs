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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace mohome_api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private IRepository db;
        private string storage = LOCAL_STORAGE + PHOTO_PATH;

        public PhotosController(IRepository db)
        {
            this.db = db;
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

            if (albumId > 0)
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

            var imageName = Guid.NewGuid().ToString().Replace("-", "");
            var newFilename = $"{imageName}.{extension}";
            var thumbName = $"{imageName}_thumb.{extension}";

            var additionalPath = $"/{userId}";
            var newImagePath = $"{additionalPath}/{newFilename}";
            var newThumbPath = $"{additionalPath}/{thumbName}";

            db.AddPhoto(newFilename, userId, albumId, newImagePath, newThumbPath, thumbName);

            var path = storage + newImagePath;
            var thumbPath = storage + newThumbPath;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }

            var resizedThumb = ResizeImage(Image.FromStream(photo.OpenReadStream()));

            if (!Directory.Exists(Path.GetDirectoryName(thumbPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(thumbPath));


            resizedThumb.Save(thumbPath);


            return Ok(new { response = new { fileName = newFilename } });
        }

        /// <summary>
        /// Returns base64 photo by photo name. If thumb=true, returns base64 thumbnail
        /// </summary>
        ///  <response code="500">Uploading error</response>  
        ///  <response code="500">Internal server error</response> 
        ///  <response code="404">File not found</response>  
        ///  <response code="401">Your user id is undefined</response>  

        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]

        [Route("{photoName}")]
        [HttpGet, Authorize]
        public IActionResult GetPhoto(string photoName, [FromQuery(Name = "thumb")] bool? thumb)
        {
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var extension = Path.GetExtension(photoName).Replace(".", ""); ;

            var photo = db.GetPhoto(userId, photoName);

            if (photo is null)
            {
                return StatusCode(404, new ErrorDetails() { errorId = ErrorList.FileNotFound.Id, errorMessage = ErrorList.FileNotFound.Description });
            }
            string imgPath = storage + photo.Path;

            if (thumb != null) { imgPath = storage + ((bool)thumb ? photo.ThumbPath : photo.Path); }

            return Ok(new
            {
                response = new
                {
                    image = Convert.ToBase64String(System.IO.File.ReadAllBytes(imgPath)),
                    imageType = $"image/{extension}",
                    description = photo.Caption,
                    created = photo.Created,
                    albumId = photo.AlbumId
                }
            });
        }


        /// <summary>
        /// Deletes a photo by photo name
        /// </summary>
        ///  <response code="500">Uploading error</response>  
        ///  <response code="500">Internal server error</response> 
        ///  <response code="403">Unauthorized action</response>  
        ///  <response code="401">Your user id is undefined</response>  

        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ResponseCache(Duration = 10)]
        [Route("{photoName}")]
        [HttpDelete, Authorize]
        public IActionResult DeletePhoto(string photoName)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var extension = Path.GetExtension(photoName).Replace(".", ""); ;

            var result = db.DeletePhoto(photoName, userId);

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
                List<PhotosViewModel> list = new List<PhotosViewModel>();
                foreach (var photo in photos)
                {
                    var model = new PhotosViewModel
                    {
                        Name = photo.Name,
                        Caption = photo.Caption,
                        Created = photo.Created,
                        AlbumId = photo.AlbumId,
                        URL = $"{PHOTO_STORAGE_PATH}/{photo.Name}"
                    };
                    list.Add(model);
                }
                return Ok(new { response = list });
            }
            else
            {
                photos = db.GetPhotosByAlbum(userId, (int)albumId);
                List<PhotosWithAlbumModel> list = new List<PhotosWithAlbumModel>();
                foreach (var photo in photos)
                {
                    var model = new PhotosWithAlbumModel
                    {
                        Name = photo.Name,
                        Caption = photo.Caption,
                        Created = photo.Created,
                        URL = $"{PHOTO_STORAGE_PATH}/{photo.Name}"
                    };
                    list.Add(model);
                }
                return Ok(new { response = list });
            }
        }


        /// <summary>
        /// Updates photo description
        /// </summary>
        ///  <response code="500">Internal server error</response>  
        ///  <response code="401">Your user id is undefined</response>  

        [Route("{photoName}")]
        [UserActionFilter]
        [ModelActionFilter]
        [HttpPatch, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult ChangePhotoDescription(string photoName, [FromBody] ChangePhotoDescription model)
        {
            int userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

            var result = db.ChangePhotoDescription(photoName, model.Description, userId);

            if (result > 0) return Ok(new { response = 1 });
            if (result == 0) return Ok(new { response = 0 });
            throw new Exception("Unknown error");
        }

        private Bitmap ResizeImage(Image image)
        {
            int width = 250, height = (int) (image.Height / ((double) image.Width / width));
            using (image)
            {
                Bitmap cpy = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.DrawImage(image,
                        new Rectangle(0, 0, width, height),
                        new Rectangle(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }


    }
}