using DAL.EntityFramework.Utility;
using DTO.Permission;
using DTO.Responses;

namespace BLL.Abstract;

public interface IPermissionService
{
    Task<IDataResult<List<PermissionToListDto>>> GetAsync();

    Task<IDataResult<PaginatedList<PermissionToListDto>>> GetAsPaginatedListAsync();

    Task<IDataResult<PermissionToListDto>> GetAsync(Guid id);

    Task<IResult> AddAsync(PermissionToAddDto addDto);

    Task<IResult> UpdateAsync(Guid permissionId, PermissionToUpdateDto updateDto);

    Task<IResult> SoftDeleteAsync(Guid id);
}