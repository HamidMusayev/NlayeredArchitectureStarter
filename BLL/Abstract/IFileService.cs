using DTO.File;
using DTO.Responses;

namespace BLL.Abstract;

public interface IFileService
{
    Task<IDataResult<FileToListDto>> GetAsync(string hashName);
    Task<IResult> AddAsync(FileToAddDto dto, FileUploadRequestDto requestDto);
    Task<IResult> RemoveAsync(FileRemoveRequestDto dto);
}