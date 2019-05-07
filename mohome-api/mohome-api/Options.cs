using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api
{
    public static class Options
    {
        public static string LOCAL_STORAGE_URL = "http://213.141.130.153/api";
        public static string PHOTO_PATH = "/photo";
        public static string PHOTO_STORAGE_PATH = LOCAL_STORAGE_URL + PHOTO_PATH;
        public static string LOCAL_STORAGE = "D:/storage";

        private readonly static List<string> AllowedExtensions = new List<string>() { "png", "jpg", "jpeg", "gif" };

        public enum claimTypes { Email, Name, Role, Id }
        public readonly static int uploadPhotoLimitMB = 10;
        public readonly static int uploadVideoLimitMB = 100;
        public readonly static int uploadMusicLimitMB = 20;
    }
}
    