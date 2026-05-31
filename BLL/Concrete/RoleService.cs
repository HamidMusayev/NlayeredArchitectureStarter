using BLL.Abstract;
using BLL.Mappers;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Permission;
using DTO.Responses;
using DTO.Role;
using ENTITIES.Entities;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IRoleService" /> implementation. Manages role lifecycle including
///     permission assignment (resolved and attached atomically on add/update) and soft-delete.
///     Permission-clearing on update calls the repository helper then commits once via
///     <c>IUnitOfWork</c>.
/// </summary>
public class RoleService(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUserRepository userRepository,
    IUserPermissionsCache userPermissionsCache,
    IUnitOfWork unitOfWork,
    RoleMapper roleMapper,
    PermissionMapper permissionMapper) : IRoleService
{
    public async Task<IResult> AddAsync(RoleToAddDto addDto)
    {
        var data = new Role { Name = string.Empty, Key = string.Empty };
        roleMapper.UpdateEntity(addDto, data);

        if (addDto.PermissionIds is { Count: > 0 })
        {
            var permissions = await permissionRepository.GetListAsync(m => addDto.PermissionIds.Contains(m.Id));
            data.Permissions = permissions;
        }

        await roleRepository.AddRoleAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await roleRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        roleRepository.SoftDelete(data);
        await unitOfWork.CommitAsync();

        await InvalidateUsersOfRoleAsync(id);

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<List<RoleToListDto>>> GetAsync()
    {
        var datas = roleMapper.ToListDtos(await roleRepository.GetListAsync());
        return new SuccessDataResult<List<RoleToListDto>>(datas, Messages.Success.Translate());
    }

    public async Task<IDataResult<RoleToListDto>> GetAsync(Guid id)
    {
        var role = await roleRepository.GetAsync(m => m.Id == id);
        if (role is null)
            return new ErrorDataResult<RoleToListDto>(Messages.DataNotFound.Translate());

        return new SuccessDataResult<RoleToListDto>(roleMapper.ToListDto(role), Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid id, RoleToUpdateDto updateDto)
    {
        // Load the tracked entity and layer the DTO over it so columns the DTO doesn't carry
        // (audit fields, etc.) aren't zeroed by the EF change tracker.
        var existing = await roleRepository.GetAsync(m => m.Id == id);
        if (existing is null) return new ErrorResult(Messages.DataNotFound.Translate());

        roleMapper.UpdateEntity(updateDto, existing);

        await roleRepository.ClearRolePermissionsAync(id);
        if (updateDto.PermissionIds is { Count: > 0 })
        {
            var permissions = await permissionRepository.GetListAsync(m => updateDto.PermissionIds.Contains(m.Id));
            existing.Permissions = permissions;
        }

        await unitOfWork.CommitAsync();

        await InvalidateUsersOfRoleAsync(id);

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<List<PermissionToListDto>>> GetPermissionsAsync(Guid id)
    {
        var role = await roleRepository.GetAsync(m => m.Id == id);
        if (role is null)
            return new ErrorDataResult<List<PermissionToListDto>>(Messages.DataNotFound.Translate());

        var datas = permissionMapper.ToListDtos(role.Permissions);

        return new SuccessDataResult<List<PermissionToListDto>>(datas, Messages.Success.Translate());
    }

    /// <summary>
    ///     Drops the cached permission set for every user holding <paramref name="roleId" />.
    ///     Called from update / soft-delete so role changes surface immediately rather than
    ///     waiting on the cache TTL.
    /// </summary>
    private async Task InvalidateUsersOfRoleAsync(Guid roleId)
    {
        var userIds = await userRepository.GetUserIdsByRoleAsync(roleId);
        foreach (var userId in userIds)
            await userPermissionsCache.InvalidateAsync(userId);
    }
}