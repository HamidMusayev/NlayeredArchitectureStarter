using Microsoft.AspNetCore.Http;
namespace CORE.Abstract;

public interface ISftpService
{
    void UploadFile(string folderPath, string fileName, IFormFile formFile);
    void DeleteFile(string folderPath, string fileName);
    byte[] ReadFile(string folderPath, string fileName);
}