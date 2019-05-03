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
    public class PhotoController : ControllerBase
    {
        IRepository db;


        public PhotoController(IRepository rep)
        {
            this.db = rep;
        }

        [Route("album")]
        [HttpGet]
        public string GetAlbumHash()
        {
            string key = Guid.NewGuid().ToString("N").Substring(0, 10);
            return Newtonsoft.Json.JsonConvert.SerializeObject(key);
        }
    }
}