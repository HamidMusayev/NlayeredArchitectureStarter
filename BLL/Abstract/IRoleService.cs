using DTO.Permission;
using DTO.Responses;
using DTO.Role;

namespace BLL.Abstract;

/// <summary>
///     Application service for role management. Supports CRUD with permission assignment
///     (create/update accept a list of permission IDs that are resolved and associated atomically),
///     listing, soft-delete, and querying the permissions attached to a specific role.
/// </summary>
public interface IRoleService
{
    Task<IDataResult<List<RoleToListDto>>> GetAsync();

    Task<IDataResult<List<PermissionToListDto>>> GetPermissionsAsync(Guid id);

    Task<IDataResult<RoleToListDto>> GetAsync(Guid id);

    Task<IResult> AddAsync(RoleToAddDto addDto);

    Task<IResult> UpdateAsync(Guid id, RoleToUpdateDto updateDto);

    Task<IResult> SoftDeleteAsync(Guid id);
}