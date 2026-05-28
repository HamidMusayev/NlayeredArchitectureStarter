using DTO.Permission;
using DTO.Responses;
using DTO.Role;

namespace BLL.Abstract;

public interface IRoleService
{
    Task<IDataResult<List<RoleToListDto>>> GetAsync();

    Task<IDataResult<List<PermissionToListDto>>> GetPermissionsAsync(Guid id);

    Task<IDataResult<RoleToListDto>> GetAsync(Guid id);

    Task<IResult> AddAsync(RoleToAddDto addDto);

    Task<IResult> UpdateAsync(Guid id, RoleToUpdateDto updateDto);

    Task<IResult> SoftDeleteAsync(Guid id);
}