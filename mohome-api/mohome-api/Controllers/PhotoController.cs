using System;
using System.Collections.Generic;
using System.Linq;
using mohome_api.ViewModels;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mohome_api.Infrastructure;
using mohome_api.API_Errors;

namespace mohome_api.Controllers
{
    [Route("Api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private IRepository db;
        private FileUploader uploader;


        public PhotoController(IRepository db, FileUploader uploader)
        {
            this.db = db;
            this.uploader = uploader;
        }

        
        /// <summary>
        /// [TEST] Generates unique name for photo
        /// </summary>
        [Route("Generate-name")]
        [HttpGet]
        public IActionResult GeneratePhotoName()
        {
            string key = Guid.NewGuid().ToString("N").Substring(0, 10);
            return Ok(new { response = Newtonsoft.Json.JsonConvert.SerializeObject(key) });
        }

        /// <summary>
        /// Returns album list for current user
        /// </summary>
        [Route("Album")]
        [HttpGet, Authorize]
        public IActionResult GetAlbums()
        {
            try
            {
                var currentUser = HttpContext.User;
                if (!currentUser.HasClaim(c => c.Type == claimTypes.Role.ToString()))
                {
                    return StatusCode(401, new { error = "Your user id is undefined" });
                }

                var userId = Convert.ToInt32(currentUser.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value);

                var albums = db.GetPhotoAlbums(userId);

                List<PhotoAlbumModel> albumsvm = new List<PhotoAlbumModel>();

                foreach(var album in albums)
                {
                    var model = new PhotoAlbumModel();
                    model.AlbumId = album.AlbumId;
                    model.Created = album.Created;
                    model.Description = album.Description;
                    model.Name = album.Name;
                    if (album.CoverPhotoId is null) { model.CoverPhotoPath = null; }
                    albumsvm.Add(model);
                }

                return Ok(new { response = albumsvm });
            }
            catch (Exception ex)
            {
                 return StatusCode(500, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ex.Message }});
            }
            
        }

        /// <summary>
        /// Creates a new album
        /// </summary>
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="520">Unknown error</response>  
        [Route("Album")]
        [HttpPost, Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        public IActionResult CreateAlbum([FromBody] CreateAlbumModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Your input data is incorrect");
                };
                var currentUser = HttpContext.User;
                if (!currentUser.HasClaim(c => c.Type == claimTypes.Role.ToString()))
                {
                    return StatusCode(401, new { error = "Your user id is undefined" });
                }
                var userId = currentUser.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value;

                var newAlbumId = db.CreateAlbum(model.AlbumName, model.Description, Convert.ToInt32(userId));
                if(newAlbumId <= 0)
                {
                    // TODO: describe the error
                    return StatusCode(500);
                }        
                string userAlbumPath = "/" + userId + "/" + newAlbumId;
                uploader.CreateAlbum(userAlbumPath);
                return Ok(new { response = "album has been created" });
            }
            catch(Exception ex)
            {
                    return StatusCode(500, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ex.Message }});
            }
        }

        /// <summary>
        /// Deletes a specific album
        /// </summary>
        ///  <response code="500">Failed to delete album. Try again</response>  
        ///  <response code="400">Your input data is incorrect</response>  
        ///  <response code="401">Your user id is undefined</response>  
        ///  <response code="520">Unknown error</response>  

        [Route("Album")]
        [HttpDelete, Authorize]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(520)]
        public IActionResult DeleteAlbum([FromBody] DeleteAlbumModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Your input data is incorrect");
                };

                var currentUser = HttpContext.User;
                if (!currentUser.HasClaim(c => c.Type == claimTypes.Role.ToString()))
                {
                    return StatusCode(401, new { error = new { errorCode = ErrorList.InvalidRefreshToken.Id,
                                                               errorMessage = ErrorList.InvalidRefreshToken.Description}});
                }

                int userId;
                int.TryParse(currentUser.Claims.FirstOrDefault(c => c.Type == claimTypes.Id.ToString()).Value,
                             out userId);

                var result =  db.DeleteAlbum(model.AlbumId, userId);

                switch (result)
                {
                    case -1: return StatusCode(403, new { error = new { errorCode = ErrorList.UnauthorizedAction.Id,
                                                                        errorMessage = ErrorList.UnauthorizedAction.Description } });
                    case 0:  return Ok(new { response = 0 });
               }

                return Ok(new { response = 1});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ex.Message }});
            }
        }
    }
}