using System.Net.Http.Headers;

namespace InternshipApi.Services
{
    public class UploadImageFile : IUploadFils
    {
        private readonly IWebHostEnvironment _environment;

        public string[] allowExtend = { ".jpg", ".png", ".jpeg", ".tif", ".gif", ".jfif", ".JPG" };
        public int fileSize = 4 * 1024 * 1024;//4MG
        public UploadImageFile(IWebHostEnvironment environment)
        {
            this._environment = environment;
        }

        public static string accept = "image/*";

        public string basePath = "Images";



        public String UploadFile(IFormFile upload, string subFolder = "ProuctsImg")
        {
            string fileUrl = string.Empty;

            if (upload.Length > fileSize)
            {
                throw new Exception($"Allowed File Size is {fileSize / 1024 / 1024} Mg");
            }

            if (!allowExtend.Contains(Path.GetExtension(upload.FileName)))
            {
                throw new Exception($"Allowed File type is {String.Join(",", allowExtend)} ");

            }

            // photo ===>> ({" Health.png "})

            var FileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);

            var FilePath = Path.Combine(_environment.WebRootPath, basePath, subFolder, FileName);

            using (var fileStream = System.IO.File.Create(FilePath))
            {
                upload.CopyTo(fileStream);
            }


            fileUrl = $"/{basePath}/{subFolder}/{FileName}";

            return fileUrl;


        }
    }
}
