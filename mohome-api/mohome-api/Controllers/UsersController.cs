using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static mohome_api.Controllers.TokenController;

namespace mohome_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        IRepository db;

        public UsersController(IRepository rep)
        {
            this.db = rep;
        }

        [HttpGet, Authorize]
        public string Get()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(db.GetProfiles());
        }

        [AllowAnonymous]
        [Route("sign-up")]
        [HttpPost]
        public IActionResult SignUp([FromBody] RegisterModel model)
        {
            IActionResult response;
            if (!db.CheckProfileExists(model.Email))
            {
                response = Conflict();
                return response;
            }

            if (db.AddNewUser(model.Email, model.Password, model.Username))
            {
                var controller = (TokenController) HttpContext.RequestServices.GetService(typeof(TokenController));
                return controller.CreateToken(new LoginModel { Email = model.Email, Password = model.Password });
            }
            return StatusCode(500);
        }

        public class RegisterModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
        }

    }
}