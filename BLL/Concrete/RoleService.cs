using AutoMapper;
using BLL.Abstract;
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
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRoleService
{
    public async Task<IResult> AddAsync(RoleToAddDto addDto)
    {
        var data = mapper.Map<Role>(addDto);

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

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<List<RoleToListDto>>> GetAsync()
    {
        var datas = mapper.Map<List<RoleToListDto>>(await roleRepository.GetListAsync());
        return new SuccessDataResult<List<RoleToListDto>>(datas, Messages.Success.Translate());
    }

    public async Task<IDataResult<RoleToListDto>> GetAsync(Guid id)
    {
        var data = mapper.Map<RoleToListDto>(await roleRepository.GetAsync(m => m.Id == id));

        return new SuccessDataResult<RoleToListDto>(data, Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid id, RoleToUpdateDto updateDto)
    {
        var data = mapper.Map<Role>(updateDto);
        data.Id = id;

        await roleRepository.ClearRolePermissionsAync(id);

        if (updateDto.PermissionIds is { Count: > 0 })
        {
            var permissions = await permissionRepository.GetListAsync(m => updateDto.PermissionIds.Contains(m.Id));
            data.Permissions = permissions;
        }

        roleRepository.UpdateRole(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<List<PermissionToListDto>>> GetPermissionsAsync(Guid id)
    {
        var role = await roleRepository.GetAsync(m => m.Id == id);
        if (role is null)
            return new ErrorDataResult<List<PermissionToListDto>>(Messages.DataNotFound.Translate());

        var datas = mapper.Map<List<PermissionToListDto>>(role.Permissions);

        return new SuccessDataResult<List<PermissionToListDto>>(datas, Messages.Success.Translate());
    }
}