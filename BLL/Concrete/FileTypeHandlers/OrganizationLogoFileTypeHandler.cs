using BLL.Abstract;
using DTO.File;
using ENTITIES.Enums;

namespace BLL.Concrete.FileTypeHandlers;

public class OrganizationLogoFileTypeHandler : IFileTypeHandler
{
    public FileType Type => FileType.OrganizationLogo;

    public Task OnFileAddedAsync(Guid fileId, FileUploadRequestDto request)
    {
        return Task.CompletedTask;
    }

    public Task OnFileRemovedAsync(FileRemoveRequestDto request)
    {
        return Task.CompletedTask;
    }
    // TODO: when OrganizationCQRS exposes AddLogo command, dispatch via IMediator
}