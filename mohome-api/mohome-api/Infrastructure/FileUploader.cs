using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace mohome_api.Infrastructure
{
    public class FileUploader
    {
        private IHostingEnvironment appEnvironment;
        private string STORAGE_PATH;
        private string PHOTO_PATH = "/photo";

        public FileUploader(IHostingEnvironment appEnvironment)
        {
            this.appEnvironment = appEnvironment;
            STORAGE_PATH = appEnvironment.WebRootPath + "/storage";
        }
        public async void UploadPhoto(string additionalPath, IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                string path = STORAGE_PATH + PHOTO_PATH + additionalPath + uploadedFile.FileName;

                using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }  
            }
        }

        public void CreateAlbum(string additionalPath)
        {
            var path = STORAGE_PATH + additionalPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public void DeleteAlbum(string additionalPath)
        {
            var path = STORAGE_PATH + additionalPath;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
