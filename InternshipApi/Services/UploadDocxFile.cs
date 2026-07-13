

namespace InternshipApi.Services
{
    public class UploadDocxFile:IUploadFils
    {

       private readonly IWebHostEnvironment _environment;

        private string[] allowExtend = { ".pdf", ".doc", ".docx" ,".rar",".zip"};
        public int fileSize = 4 * 1024 * 1024;


        public UploadDocxFile(IWebHostEnvironment environment)
        {
            this._environment = environment;
        }

        

        public string basePath = "Docx";
        

        public String UploadFile(IFormFile upload, string subFolder = "DocxFile")
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

            

            var FileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);

            var folderPath = Path.Combine(
                _environment.WebRootPath,
                basePath,
                subFolder);

            Directory.CreateDirectory(folderPath);



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
