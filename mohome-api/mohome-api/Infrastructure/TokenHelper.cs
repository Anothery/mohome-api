using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.Infrastructure
{
    public static class TokenHelper
    {
        public static int GetUserIdFromRefreshToken(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userId = jwt.Claims.First(c => c.Type == claimTypes.Id.ToString()).Value;
            int id = 0;
            int.TryParse(userId, out id);
            return id;
        }


    }
}
