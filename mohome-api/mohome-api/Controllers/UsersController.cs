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

    }
}