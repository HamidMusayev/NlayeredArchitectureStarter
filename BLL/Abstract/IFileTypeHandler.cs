using DTO.File;
using ENTITIES.Enums;

namespace BLL.Abstract;

public interface IFileTypeHandler
{
    FileType Type { get; }
    Task OnFileAddedAsync(Guid fileId, FileUploadRequestDto request);
    Task OnFileRemovedAsync(FileRemoveRequestDto request);
}