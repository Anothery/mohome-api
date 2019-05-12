using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mohome_api.API_Errors;
using mohome_api.ViewModels;
using static mohome_api.Controllers.TokenController;

namespace mohome_api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        IRepository db;

        public UsersController(IRepository rep)
        {
            this.db = rep;
        }

        /// <summary>
        /// Returns Ok result if user with the email exist in the database
        /// </summary>

        [AllowAnonymous]
        [HttpGet]
        public IActionResult CheckUserExists([FromQuery(Name = "email")] string email)
        {
            if (db.CheckProfileExists(email))
            {
                return Ok(new { response = 1 });
            }
            return NotFound(new ErrorDetails() { errorId = ErrorList.UserNotFound.Id, errorMessage = ErrorList.UserNotFound.Description });
        }

    }
}