using System.Collections.Generic;
using Models;

namespace DBRepository
{
    public class MohomeRepository : IRepository
    {
        private MohomeContext db;

        public MohomeRepository(MohomeContext db)
        {
            this.db = db;
        }

        public IEnumerable<Profile> GetProfiles()
        {
            return  db.Profile;
        }
    }
}
