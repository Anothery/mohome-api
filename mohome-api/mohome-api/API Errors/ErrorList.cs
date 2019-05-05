using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.API_Errors
{
    public class Error
    {
        public int Id { get; set; }
        public string Description { get; }
        public string Tip { get; }

        public Error(int Id, string Description, string Tip)
        {
            this.Id = Id;
            this.Description = Description;
            this.Tip = Tip;
        }
    }
    public static class ErrorList
    {
        public static Error UnknownError = new Error(1, "Unknown error occured", "Try Again Later");
        public static Error InputDataError = new Error(2, "Your input data is incorrect", "Make sure your input data is correct");
        public static Error UndefinedUser = new Error(3, "Your user id is undefined", "Try to renew your access token");
        public static Error InvalidRefreshToken = new Error(10, "Invalid refresh token", "Try to renew your refresh token");
        public static Error UnauthorizedAction = new Error(50, "That action is not allowed to you.", "You have not access to that action");
    }
}
