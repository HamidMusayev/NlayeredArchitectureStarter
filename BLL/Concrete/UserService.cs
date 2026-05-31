using BLL.Abstract;
using BLL.Mappers;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DAL.EntityFramework.Utility;
using DTO.Common;
using DTO.Responses;
using DTO.User;
using ENTITIES.Entities;
using ENTITIES.Enums;
using ENTITIES.Identifiers;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IUserService" /> implementation. Handles user creation (hashes password,
///     assigns Guest role when none provided), profile updates (excludes credentials from the UPDATE
///     statement), soft-delete (also invalidates all tokens), and paginated/full list retrieval.
///     <para>
///         Uses the Mapperly-generated <see cref="UserMapper" /> directly — no <c>IMapper</c>
///         injection. Compile-time mappers catch missing properties at build time instead of at
///         <c>Map&lt;T&gt;</c>.
///     </para>
/// </summary>
public class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITokenRepository tokenRepository,
    IUserPermissionsCache userPermissionsCache,
    IUnitOfWork unitOfWork,
    UserMapper userMapper,
    IPaginationContext paginationContext,
    IPasswordHasher passwordHasher)
    : IUserService
{
    public async Task<IResult> AddAsync(UserToAddDto addDto)
    {
        if (await userRepository.IsUserExistAsync(addDto.Email, null))
            return new ErrorResult(Messages.UserIsExist.Translate());

        if (addDto.RoleId is null)
        {
            var guest = await roleRepository.GetAsync(m => m.Key == nameof(UserType.Guest));
            if (guest is not null) addDto = addDto with { RoleId = new RoleId(guest.Id) };
        }

        // Construct the entity explicitly — User has `required` fields the DTO doesn't carry
        // (Salt, hashed Password). Mapperly copies the DTO-supplied bits onto this skeleton.
        var salt = passwordHasher.GenerateSalt();
        var data = new User
        {
            Username = string.Empty,
            Email = string.Empty,
            ContactNumber = string.Empty,
            Password = passwordHasher.Hash(addDto.Password, salt),
            Salt = salt
        };
        userMapper.UpdateEntity(addDto, data);

        await userRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await userRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        userRepository.SoftDelete(data);

        var typedUserId = new UserId(id);
        var tokens = await tokenRepository.GetListAsync(m => m.UserId == typedUserId);
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

        return new SuccessDataResult<List<UserToListDto>>(userMapper.ToListDtos(datas),
            Messages.Success.Translate());
    }

    public async Task<IDataResult<UserToListDto>> GetAsync(Guid id)
    {
        var data = await userRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorDataResult<UserToListDto>(Messages.UserIsNotExist.Translate());

        return new SuccessDataResult<UserToListDto>(userMapper.ToListDto(data), Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid id, UserToUpdateDto updateDto)
    {
        if (await userRepository.IsUserExistAsync(updateDto.Email, id))
            return new ErrorResult(Messages.UserIsExist.Translate());

        if (updateDto.RoleId is null)
        {
            var guest = await roleRepository.GetAsync(m => m.Key == UserType.Guest.ToString());
            if (guest is not null) updateDto = updateDto with { RoleId = new RoleId(guest.Id) };
        }

        // Load the tracked entity and apply the DTO over it. This preserves every column the
        // DTO doesn't carry — including FailedLoginAttempts / LockedUntil from P1.4 — instead
        // of letting EF emit zeros for them like the old "map fresh entity + UpdateUser" dance
        // silently did.
        var existing = await userRepository.GetAsync(u => u.Id == id);
        if (existing is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        userMapper.UpdateEntity(updateDto, existing);
        await unitOfWork.CommitAsync();

        // User's role may have changed — drop the cached permission set so the next request resolves fresh.
        await userPermissionsCache.InvalidateAsync(id);

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<PagedResult<UserToListDto>>> GetAsPaginatedListAsync()
    {
        var pagination = paginationContext.GetPagination();

        // Always page off an ordered query — EF warns about pagination on unordered sources and
        // the page contents become non-deterministic across calls.
        var page = await userRepository.GetList()
            .OrderBy(u => u.Id)
            .PageAsync(pagination.PageIndex, pagination.PageSize);

        var projected = page.Map(items =>
            userMapper.ToListDtos(items));

        return new SuccessDataResult<PagedResult<UserToListDto>>(projected, Messages.Success.Translate());
    }
}