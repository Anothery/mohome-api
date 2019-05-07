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
        public static Error InternalServerError = new Error(4, "Server can't process your request", "Try again later");
        public static Error FileNotFound = new Error(5, "File not found in the server", "");
        public static Error ProfileAlreadyExists = new Error(6, "Profile already exists", "");
        public static Error InvalidRefreshToken = new Error(10, "Invalid refresh token", "Try to renew your refresh token");
        public static Error UserNotFound = new Error(51, "User does not exist in database", "");
        public static Error UnauthorizedAction = new Error(50, "That action is not allowed to you.", "You have not access to that action");
        public static Error UploadingError = new Error(100, "Uploading error", "Try Again Later");
        public static Error WrongSize = new Error(101, "File size is greater than allowed", "Limits: photo: 10 MB, video: 100 MB, music: 20mb");
        public static Error Unauthorized = new Error(401, "You are unauthorized", "Sign in to mohome");
    }
}
