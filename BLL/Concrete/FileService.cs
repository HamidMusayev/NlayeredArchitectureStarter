using AutoMapper;
using BLL.Abstract;
using CORE.Localization;
using DAL.EntityFramework.UnitOfWork;
using DTO.File;
using DTO.Responses;
using ENTITIES.Enums;
using File = ENTITIES.Entities.File;

namespace BLL.Concrete;

public class FileService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
    : IFileService
{
    public async Task<IResult> AddAsync(FileToAddDto dto, FileUploadRequestDto requestDto)
    {
        var fileId = await AddAsync(dto);

        switch (dto.Type)
        {
            case FileType.UserProfile:
                await userService.AddProfileAsync(requestDto.UserId!.Value, fileId.Data);
                break;
            case FileType.OrganizationLogo:
                // because of organization services are in mediatr
                // we don't need to inject this to here and add mediatr package to BLL just for this line.
                // that is example line and shows logic
                //await _organizationService.AddLogoAsync(requestDto.OrganizationId!.Value, fileId.Data);
                break;
        }

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> RemoveAsync(FileRemoveRequestDto dto)
    {
        await SoftDeleteAsync(dto.HashName);

        switch (dto.Type)
        {
            case FileType.UserProfile:
                await userService.AddProfileAsync(dto.UserId!.Value, null);
                break;
            case FileType.OrganizationLogo:
                //await _organizationService.AddProfileAsync(userId!.Value, null);
                break;
        }

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<FileToListDto>> GetAsync(string hashName)
    {
        var data = await unitOfWork.FileRepository.GetAsync(m => m.HashName == hashName);
        if (data is null) return new ErrorDataResult<FileToListDto>(Messages.DataNotFound.Translate());

        var mapped = mapper.Map<FileToListDto>(data);

        return new SuccessDataResult<FileToListDto>(mapped, Messages.Success.Translate());
    }

    private async Task<IDataResult<Guid>> AddAsync(FileToAddDto dto)
    {
        var data = mapper.Map<File>(dto);

        var added = await unitOfWork.FileRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessDataResult<Guid>(added.Id, Messages.Success.Translate());
    }

    private async Task<IResult> SoftDeleteAsync(string hashName)
    {
        var data = await unitOfWork.FileRepository.GetAsync(m => m.HashName == hashName);

        unitOfWork.FileRepository.SoftDelete(data!);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}