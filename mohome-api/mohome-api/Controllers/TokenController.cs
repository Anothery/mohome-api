using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DBRepository;
using Microsoft.AspNetCore.Authorization;
using mohome_api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mohome_api.Infrastructure;

namespace mohome_api.Controllers
{
    // TODO: trycatch and describing errors
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    { 
        private IRepository db;
        private IConfiguration _config;

        public TokenController(IConfiguration config, IRepository rep)
        {
            this.db = rep;
            _config = config;
        }

        /// <summary>
        /// Creates bearer token
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody] LoginModel login)
        {
            IActionResult response = Unauthorized();
            var user = Authenticate(login);

            if (user != null)
            {
                var tokenString = BuildToken(user);
                response = Ok(new { response = new { token = tokenString } });
            }

            return response;
        }


        private string BuildToken(UserModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(claimTypes.Role.ToString() , user.Role),
                new Claim(claimTypes.Email.ToString(), user.Email),
                new Claim(claimTypes.Name.ToString(), user.Name),
                new Claim(claimTypes.Id.ToString(), user.Id.ToString())
            };


            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(45),
              signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel Authenticate(LoginModel login)
        {
            UserModel user = null;
            var profile = db.GetProfile(login.Email, login.Password);

            if (profile is null) return null;
            user = new UserModel { Name = profile.Name,  Email = login.Email, Role = profile.Role.Name, Id = profile.UserId};
            return user;
        }
    }
}