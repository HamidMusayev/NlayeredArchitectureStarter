using DTO.Common;
using DTO.Permission;
using DTO.Responses;

namespace BLL.Abstract;

/// <summary>
///     Application service for permission management — CRUD, full list, paginated list, and
///     soft-delete. Permissions are the leaf-level capabilities assigned to Roles.
/// </summary>
public interface IPermissionService
{
    Task<IDataResult<List<PermissionToListDto>>> GetAsync();

    Task<IDataResult<PagedResult<PermissionToListDto>>> GetAsPaginatedListAsync();

    Task<IDataResult<PermissionToListDto>> GetAsync(Guid id);

    Task<IResult> AddAsync(PermissionToAddDto addDto);

    Task<IResult> UpdateAsync(Guid permissionId, PermissionToUpdateDto updateDto);

    Task<IResult> SoftDeleteAsync(Guid id);
}