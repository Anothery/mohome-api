using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    }
}