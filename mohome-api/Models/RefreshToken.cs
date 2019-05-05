using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreationDate { get; set; }

        public virtual Profile User { get; set; }
    }

}
