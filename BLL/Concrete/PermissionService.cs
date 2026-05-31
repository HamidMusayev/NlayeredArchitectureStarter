using BLL.Abstract;
using BLL.Mappers;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DAL.EntityFramework.Utility;
using DTO.Common;
using DTO.Permission;
using DTO.Responses;
using ENTITIES.Entities;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IPermissionService" /> implementation. Provides full-list, paginated-list,
///     single-item lookup, create, update, and soft-delete for <c>Permission</c> records.
/// </summary>
public class PermissionService(
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    PermissionMapper permissionMapper,
    IPaginationContext paginationContext)
    : IPermissionService
{
    public async Task<IResult> AddAsync(PermissionToAddDto addDto)
    {
        var data = new Permission { Name = string.Empty, Key = string.Empty };
        permissionMapper.UpdateEntity(addDto, data);

        await permissionRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await permissionRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        permissionRepository.SoftDelete(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<PagedResult<PermissionToListDto>>> GetAsPaginatedListAsync()
    {
        var pagination = paginationContext.GetPagination();

        // Always page off an ordered query — EF warns on unordered pagination and the page
        // contents become non-deterministic across calls.
        var page = await permissionRepository.GetList()
            .OrderBy(p => p.Id)
            .PageAsync(pagination.PageIndex, pagination.PageSize);

        var projected = page.Map(items =>
            permissionMapper.ToListDtos(items));

        return new SuccessDataResult<PagedResult<PermissionToListDto>>(projected, Messages.Success.Translate());
    }

    public async Task<IDataResult<List<PermissionToListDto>>> GetAsync()
    {
        var datas = permissionMapper.ToListDtos(await permissionRepository.GetListAsync());

        return new SuccessDataResult<List<PermissionToListDto>>(datas, Messages.Success.Translate());
    }

    public async Task<IDataResult<PermissionToListDto>> GetAsync(Guid id)
    {
        var data = await permissionRepository.GetAsync(m => m.Id == id);
        if (data is null)
            return new ErrorDataResult<PermissionToListDto>(Messages.DataNotFound.Translate());

        return new SuccessDataResult<PermissionToListDto>(permissionMapper.ToListDto(data),
            Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid permissionId, PermissionToUpdateDto updateDto)
    {
        // Load the tracked entity and layer the DTO over it so audit / nav fields the DTO
        // doesn't carry stay intact.
        var existing = await permissionRepository.GetAsync(m => m.Id == permissionId);
        if (existing is null) return new ErrorResult(Messages.DataNotFound.Translate());

        permissionMapper.UpdateEntity(updateDto, existing);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}