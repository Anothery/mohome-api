using Models;
using System.Collections.Generic;

namespace DBRepository
{
    public interface IRepository
    {
        IEnumerable<Profile> GetProfiles();
    }
}
