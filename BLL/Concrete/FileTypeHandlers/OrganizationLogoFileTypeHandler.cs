using BLL.Abstract;
using DTO.File;
using ENTITIES.Enums;

namespace BLL.Concrete.FileTypeHandlers;

/// <summary>
///     <see cref="IFileTypeHandler" /> for <c>FileType.OrganizationLogo</c> uploads.
///     Currently a no-op placeholder — wire to an organization update command when the
///     Organization CQRS slice exposes an <c>AddLogo</c> command.
/// </summary>
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