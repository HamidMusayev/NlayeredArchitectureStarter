using DTO.File;
using DTO.Responses;

namespace BLL.Abstract;

/// <summary>
///     Application service for file operations. Saves/removes the metadata row in the database
///     and delegates actual byte storage to <c>IBlobStorage</c>. Post-save and post-remove
///     side-effects (e.g. linking the file to a user profile) are dispatched via
///     <c>IFileTypeHandler</c> strategies.
/// </summary>
public interface IFileService
{
    Task<IDataResult<FileToListDto>> GetAsync(string hashName);
    Task<IResult> AddAsync(FileToAddDto addDto, FileUploadRequestDto requestDto);
    Task<IResult> RemoveAsync(FileRemoveRequestDto requestDto);
}