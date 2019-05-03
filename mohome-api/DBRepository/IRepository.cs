using Models;
using System.Collections.Generic;


namespace DBRepository
{
    public interface IRepository
    {
        IEnumerable<Profile> GetProfiles();
        Profile GetProfile(string email);

        bool CheckProfileExists(string email);

        bool AddNewUser(string email, string password, string name);
    }
}
