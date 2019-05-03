using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }

        public virtual List<Profile> Profiles { get; set; }
    }
}
