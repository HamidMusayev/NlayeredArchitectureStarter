using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Helpers;
using CORE.Localization;
using DAL.EntityFramework.UnitOfWork;
using DAL.EntityFramework.Utility;
using DTO.Responses;
using DTO.User;
using ENTITIES.Entities;
using ENTITIES.Enums;

namespace BLL.Concrete;

public class UserService(IUnitOfWork unitOfWork, IMapper mapper, IUtilService utilService)
    : IUserService
{
    public async Task<IResult> AddAsync(UserToAddDto dto)
    {
        if (await unitOfWork.UserRepository.IsUserExistAsync(dto.Email, null))
            return new ErrorResult(Messages.UserIsExist.Translate());

        dto = dto with
        {
            RoleId = !dto.RoleId.HasValue
                ? (await unitOfWork.RoleRepository.GetAsync(m => m.Key == UserType.Guest.ToString()))?.Id
                : dto.RoleId
        };
        var data = mapper.Map<User>(dto);

        data.Salt = SecurityHelper.GenerateSalt();
        data.Password = SecurityHelper.HashPassword(data.Password, data.Salt);

        await unitOfWork.UserRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await unitOfWork.UserRepository.GetAsync(m => m.Id == id);

        unitOfWork.UserRepository.SoftDelete(data!);

        var tokens = await unitOfWork.TokenRepository.GetListAsync(m => m.UserId == id);
        tokens.ForEach(m => m.IsDeleted = true);

        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> AddProfileAsync(Guid userId, Guid? fileId)
    {
        var user = await unitOfWork.UserRepository.GetAsNoTrackingAsync(u => u.Id == userId);
        user!.ProfileFileId = fileId;

        await unitOfWork.UserRepository.UpdateUserAsync(user);
        await unitOfWork.CommitAsync();

        return new SuccessResult();
    }

    public async Task<IDataResult<List<UserToListDto>>> GetAsync()
    {
        var datas = await unitOfWork.UserRepository.GetListAsync();

        return new SuccessDataResult<List<UserToListDto>>(mapper.Map<List<UserToListDto>>(datas),
            Messages.Success.Translate());
    }

    public async Task<IDataResult<UserToListDto>> GetAsync(Guid id)
    {
        var data = mapper.Map<UserToListDto>(await unitOfWork.UserRepository.GetAsync(m => m.Id == id));

        return new SuccessDataResult<UserToListDto>(data, Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid id, UserToUpdateDto dto)
    {
        if (await unitOfWork.UserRepository.IsUserExistAsync(dto.Email, id))
            return new ErrorResult(Messages.UserIsExist.Translate());

        dto = dto with
        {
            RoleId = dto.RoleId is null
                ? (await unitOfWork.RoleRepository.GetAsync(m => m.Key == UserType.Guest.ToString()))?.Id
                : dto.RoleId
        };

        var old = await unitOfWork.UserRepository.GetAsNoTrackingAsync(u => u.Id == id);
        if (old is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        var data = mapper.Map<User>(dto);

        data.Id = id;
        data.ProfileFileId = old.ProfileFileId;

        await unitOfWork.UserRepository.UpdateUserAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<PaginatedList<UserToListDto>>> GetAsPaginatedListAsync()
    {
        var datas = unitOfWork.UserRepository.GetList();
        var paginationDto = utilService.GetPagination();

        var response = await PaginatedList<User>.CreateAsync(datas.OrderBy(m => m.Id), paginationDto.PageIndex,
            paginationDto.PageSize);

        var responseDto = new PaginatedList<UserToListDto>(
            mapper.Map<List<UserToListDto>>(response.Datas),
            response.TotalRecordCount, response.PageIndex, response.TotalPageCount);

        return new SuccessDataResult<PaginatedList<UserToListDto>>(responseDto,
            Messages.Success.Translate());
    }
}