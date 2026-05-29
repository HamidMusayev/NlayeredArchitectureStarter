using DTO.File;
using ENTITIES.Enums;

namespace BLL.Abstract;

/// <summary>
///     Strategy interface for per-<see cref="ENTITIES.Enums.FileType" /> post-upload and
///     post-removal behaviour. Implementations are discovered by DI and keyed on <see cref="Type" />;
///     <c>FileService</c> dispatches to the matching handler after every add or remove.
/// </summary>
public interface IFileTypeHandler
{
    FileType Type { get; }
    Task OnFileAddedAsync(Guid fileId, FileUploadRequestDto request);
    Task OnFileRemovedAsync(FileRemoveRequestDto request);
}