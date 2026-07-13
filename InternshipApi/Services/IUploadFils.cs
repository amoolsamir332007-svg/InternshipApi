
namespace InternshipApi.Services
{
    public interface IUploadFils
    {
        string? UploadFile(IFormFile upload, string subFolder = "ProuctsImg");
    }
}