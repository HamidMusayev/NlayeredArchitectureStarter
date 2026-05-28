using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DAL.EntityFramework.Utility;
using DTO.Responses;
using DTO.User;
using ENTITIES.Entities;
using ENTITIES.Enums;

namespace BLL.Concrete;

public class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPaginationContext paginationContext,
    IPasswordHasher passwordHasher)
    : IUserService
{
    public async Task<IResult> AddAsync(UserToAddDto addDto)
    {
        if (await userRepository.IsUserExistAsync(addDto.Email, null))
            return new ErrorResult(Messages.UserIsExist.Translate());

        addDto = addDto with
        {
            RoleId = addDto.RoleId
                     ?? (await roleRepository.GetAsync(m => m.Key == nameof(UserType.Guest)))?.Id
        };
        var data = mapper.Map<User>(addDto);

        data.Salt = passwordHasher.GenerateSalt();
        data.Password = passwordHasher.Hash(data.Password, data.Salt);

        await userRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await userRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        userRepository.SoftDelete(data);

        var tokens = await tokenRepository.GetListAsync(m => m.UserId == id);
        tokens.ForEach(m => m.IsDeleted = true);

        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> AddProfileAsync(Guid userId, Guid? fileId)
    {
        var user = await userRepository.GetAsNoTrackingAsync(u => u.Id == userId);
        if (user is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        user.ProfileFileId = fileId;

        userRepository.UpdateUser(user);
        await unitOfWork.CommitAsync();

        return new SuccessResult();
    }

    public async Task<IDataResult<List<UserToListDto>>> GetAsync()
    {
        // loads all rows — prefer GetAsPaginatedListAsync for large datasets
        var datas = await userRepository.GetListAsync();

        return new SuccessDataResult<List<UserToListDto>>(mapper.Map<List<UserToListDto>>(datas),
            Messages.Success.Translate());
    }

    public async Task<IDataResult<UserToListDto>> GetAsync(Guid id)
    {
        var data = await userRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorDataResult<UserToListDto>(Messages.UserIsNotExist.Translate());

        return new SuccessDataResult<UserToListDto>(mapper.Map<UserToListDto>(data), Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid id, UserToUpdateDto updateDto)
    {
        if (await userRepository.IsUserExistAsync(updateDto.Email, id))
            return new ErrorResult(Messages.UserIsExist.Translate());

        updateDto = updateDto with
        {
            RoleId = updateDto.RoleId is null
                ? (await roleRepository.GetAsync(m => m.Key == UserType.Guest.ToString()))?.Id
                : updateDto.RoleId
        };

        var old = await userRepository.GetAsNoTrackingAsync(u => u.Id == id);
        if (old is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        var data = mapper.Map<User>(updateDto);

        data.Id = id;
        data.ProfileFileId = old.ProfileFileId;

        userRepository.UpdateUser(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<PaginatedList<UserToListDto>>> GetAsPaginatedListAsync()
    {
        var datas = userRepository.GetList();
        var paginationDto = paginationContext.GetPagination();

        var response = await PaginatedList<User>.CreateAsync(datas.OrderBy(m => m.Id), paginationDto.PageIndex,
            paginationDto.PageSize);

        var responseDto = new PaginatedList<UserToListDto>(
            mapper.Map<List<UserToListDto>>(response.Items),
            response.TotalRecordCount, response.PageIndex, response.TotalPageCount);

        return new SuccessDataResult<PaginatedList<UserToListDto>>(responseDto,
            Messages.Success.Translate());
    }
}