using DTO.Common;
using DTO.Responses;
using DTO.User;

namespace BLL.Abstract;

/// <summary>
///     Application service for user management — CRUD operations, paginated listing, soft-delete,
///     and profile-picture association. Password hashing is handled internally; callers pass plain text.
/// </summary>
public interface IUserService
{
    Task<IDataResult<List<UserToListDto>>> GetAsync();

    Task<IDataResult<PagedResult<UserToListDto>>> GetAsPaginatedListAsync();

    Task<IDataResult<UserToListDto>> GetAsync(Guid id);

    Task<IResult> AddAsync(UserToAddDto addDto);

    Task<IResult> UpdateAsync(Guid id, UserToUpdateDto updateDto);

    Task<IResult> SoftDeleteAsync(Guid id);
    Task<IResult> AddProfileAsync(Guid userId, Guid? fileId);
}