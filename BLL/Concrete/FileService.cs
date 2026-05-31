using BLL.Abstract;
using BLL.Mappers;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.File;
using DTO.Responses;
using ENTITIES.Enums;
using File = ENTITIES.Entities.File;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IFileService" /> implementation. Persists file metadata to the database
///     then dispatches to the matching <see cref="IFileTypeHandler" /> strategy for side-effects
///     (e.g. linking the file to a user's profile). Actual byte storage is handled externally by
///     <c>IBlobStorage</c> before this service is called.
/// </summary>
public class FileService(
    IFileRepository fileRepository,
    IUnitOfWork unitOfWork,
    FileMapper fileMapper,
    IEnumerable<IFileTypeHandler> handlers)
    : IFileService
{
    private readonly Dictionary<FileType, IFileTypeHandler> _handlers =
        handlers.ToDictionary(h => h.Type);

    public async Task<IResult> AddAsync(FileToAddDto addDto, FileUploadRequestDto requestDto)
    {
        var fileId = await AddAsync(addDto);

        if (_handlers.TryGetValue(addDto.Type, out var handler))
            await handler.OnFileAddedAsync(fileId.Data, requestDto);

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> RemoveAsync(FileRemoveRequestDto requestDto)
    {
        await SoftDeleteAsync(requestDto.HashName);

        if (_handlers.TryGetValue(requestDto.Type, out var handler))
            await handler.OnFileRemovedAsync(requestDto);

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<FileToListDto>> GetAsync(string hashName)
    {
        var data = await fileRepository.GetAsync(m => m.HashName == hashName);
        if (data is null) return new ErrorDataResult<FileToListDto>(Messages.DataNotFound.Translate());

        return new SuccessDataResult<FileToListDto>(fileMapper.ToListDto(data), Messages.Success.Translate());
    }

    private async Task<IDataResult<Guid>> AddAsync(FileToAddDto addDto)
    {
        // Construct the entity explicitly to satisfy its `required` fields; Mapperly then layers
        // the DTO over the top. Path is non-nullable on the entity but nullable on the DTO —
        // null-coalesce to empty string after the map so the assignment doesn't NRE.
        var data = new File
        {
            OriginalName = string.Empty,
            HashName = string.Empty,
            Extension = string.Empty,
            Length = 0,
            Path = string.Empty,
            Type = addDto.Type
        };
        fileMapper.UpdateEntity(addDto, data);
        data.Path ??= string.Empty;

        var added = await fileRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessDataResult<Guid>(added.Id, Messages.Success.Translate());
    }

    private async Task<IResult> SoftDeleteAsync(string hashName)
    {
        var data = await fileRepository.GetAsync(m => m.HashName == hashName);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        fileRepository.SoftDelete(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}