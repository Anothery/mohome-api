using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mohome_api.ViewModels;
using static mohome_api.Controllers.TokenController;

namespace mohome_api.Controllers
{
    // TODO: trycatch and describing errors
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        IRepository db;

        public UsersController(IRepository rep)
        {
            this.db = rep;
        }

        /// <summary>
        /// [TEST] returns profile list
        /// </summary>
        [HttpGet, Authorize]
        public IActionResult Get()
        {
            return Ok(new { response = Newtonsoft.Json.JsonConvert.SerializeObject(db.GetProfiles()) });
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <response code="400">Your input data is incorrect</response>  
        /// <response code="520">Unknown error</response>  
        [AllowAnonymous]
        [Route("sign-up")]
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(520)]
        public IActionResult SignUp([FromBody] RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Your input data is incorrect");
                };

                IActionResult response;
                if (!db.CheckProfileExists(model.Email))
                {
                    response = Conflict(new { error = "Profile already exists" });
                    return response;
                }

                if (db.AddNewUser(model.Email, model.Password, model.Username))
                {
                    var controller = (TokenController)HttpContext.RequestServices.GetService(typeof(TokenController));
                    return controller.CreateToken(new LoginModel { Email = model.Email, Password = model.Password });
                }
                return StatusCode(520, new { error = "Unknown error" });
            }
            catch(Exception ex)
            {
                return StatusCode(520, new { error = "Unknown error" });
            }      
        }

    }
}