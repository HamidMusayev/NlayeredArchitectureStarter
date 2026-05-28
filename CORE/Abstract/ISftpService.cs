using Microsoft.AspNetCore.Http;

namespace CORE.Abstract;

public interface ISftpService
{
    Task UploadFileAsync(string folderPath, string fileName, IFormFile formFile,
        CancellationToken ct = default);

    Task DeleteFileAsync(string folderPath, string fileName, CancellationToken ct = default);

    Task<byte[]> ReadFileAsync(string folderPath, string fileName, CancellationToken ct = default);
}