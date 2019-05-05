using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Profile
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PictureUrl { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public List<RefreshToken> Tokens { get; set; }
    }
}
