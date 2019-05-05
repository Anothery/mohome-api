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
using mohome_api.API_Errors;

namespace mohome_api.Controllers
{
    // TODO: trycatch and describing errors
    [Route("Api/[controller]")]
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
        /// Creates access and refresh tokens
        /// </summary>
        /// <response code="520">Unknown error</response>  
        [AllowAnonymous]
        [Route("Sign-in")]
        [HttpPost]
        [ProducesResponseType(520)]
        public IActionResult SignIn([FromBody] LoginModel login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Your input data is incorrect");
                };

                IActionResult response = Unauthorized();
                var user = Authenticate(login);

                if (user != null)
                {
                    var accessTokenExp = DateTime.Now.AddMinutes(20);
                    var refreshTokenExp = DateTime.Now.AddYears(1);
                    var accessToken = BuildAccessToken(user, accessTokenExp);
                    var refreshToken = BuildRefreshToken(user, refreshTokenExp);
                    long unixExp = ((DateTimeOffset)accessTokenExp).ToUnixTimeSeconds();
                    db.AddRefreshToken(refreshToken, user.Id, DateTime.Now , refreshTokenExp);
                    var resp = new { response = new { accessToken, refreshToken, expiresIn = unixExp  } };
                    response = Ok(resp);
                }

                return response;
            }
            catch(Exception ex)
            {
                return StatusCode(520, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ex.Message }});
            }
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <response code="400">Your input data is incorrect</response>  
        /// <response code="520">Unknown error</response>  
        [AllowAnonymous]
        [Route("Sign-up")]
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
                    return SignIn(new LoginModel { Email = model.Email, PasswordHash = model.Password });
                }
                
                 return StatusCode(520, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ErrorList.UnknownError.Description}});
            }
            catch (Exception ex)
            {
               return StatusCode(520, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ex.Message }});
            }
        }

        /// <summary>
        /// Refreshes tokens
        /// </summary>
        /// <response code="400">Your input data is incorrect</response>  
        /// <response code="520">Unknown error</response>  
        [AllowAnonymous]
        [Route("Refresh-token")]
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(520)]
        public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = new { errorCode = ErrorList.InputDataError.Id,
                                                          errorMessage = ErrorList.InputDataError.Description} });
                };

                var userId = TokenHelper.GetUserIdFromRefreshToken(model.RefreshToken);

                if(!db.CheckRefreshToken(model.RefreshToken, userId))
                {
                    return StatusCode(401, new { error = new { errorCode = ErrorList.InputDataError.Id,
                                                          errorMessage = ErrorList.InputDataError.Description} });
                }

                db.DeleteRefreshToken(model.RefreshToken, userId);

                var profile = db.GetProfile(userId);
                var user = new UserModel
                { Name = profile.Name, Email = profile.Email, Role = profile.Role.Name, Id = profile.UserId };

                if (user != null)
                {
                    var accessTokenExp = DateTime.Now.AddMinutes(30);
                    var refreshTokenExp = DateTime.Now.AddYears(1);
                    var accessToken = BuildAccessToken(user, accessTokenExp);
                    var refreshToken = BuildRefreshToken(user, refreshTokenExp);
                    long unixExp = ((DateTimeOffset)accessTokenExp).ToUnixTimeSeconds();
                    db.AddRefreshToken(refreshToken, user.Id, DateTime.Now, refreshTokenExp);
                    var resp = new { response = new { accessToken, refreshToken, expiresIn = unixExp } };
                    return Ok(resp);
                }

                return StatusCode(520, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ErrorList.UnknownError.Description}});
            }
            catch (Exception ex)
            {
                return StatusCode(520, new { error = new { errorCode = ErrorList.UnknownError.Id,
                                                           errorMessage = ex.Message}});
            }
        }

        private string BuildAccessToken(UserModel user, DateTime expiration)
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
              expires: expiration,
              signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string BuildRefreshToken(UserModel user, DateTime expiration)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(claimTypes.Id.ToString(), user.Id.ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                                             _config["Jwt:Issuer"],
                                             claims,
                                             expires: expiration,
                                             signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        private UserModel Authenticate(LoginModel login)
        {
            UserModel user = null;
            var profile = db.GetProfile(login.Email, login.PasswordHash);

            if (profile is null) return null;
            user = new UserModel { Name = profile.Name,  Email = login.Email, Role = profile.Role.Name, Id = profile.UserId};
            return user;
        }
    }
}