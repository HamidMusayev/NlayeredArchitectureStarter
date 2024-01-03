using AutoMapper;
using BLL.Abstract;
using CORE.Localization;
using DAL.EntityFramework.UnitOfWork;
using DTO.Permission;
using DTO.Responses;
using DTO.Role;
using ENTITIES.Entities;

namespace BLL.Concrete;

public class RoleService(IUnitOfWork unitOfWork, IMapper mapper) : IRoleService
{
    public async Task<IResult> AddAsync(RoleToAddDto dto)
    {
        var data = mapper.Map<Role>(dto);

        var permissions = await unitOfWork.PermissionRepository.GetListAsync(m => dto.PermissionIds!.Contains(m.Id));
        data.Permissions = permissions;

        await unitOfWork.RoleRepository.AddRoleAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await unitOfWork.RoleRepository.GetAsync(m => m.Id == id);

        unitOfWork.RoleRepository.SoftDelete(data!);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<List<RoleToListDto>>> GetAsync()
    {
        var datas = mapper.Map<List<RoleToListDto>>(await unitOfWork.RoleRepository.GetListAsync());
        return new SuccessDataResult<List<RoleToListDto>>(datas, Messages.Success.Translate());
    }

    public Task<IDataResult<IQueryable<Role>>> GraphQlGetAsync()
    {
        return Task.FromResult<IDataResult<IQueryable<Role>>>(new SuccessDataResult<IQueryable<Role>>(
            unitOfWork.RoleRepository.GetList()!,
            Messages.Success.Translate()));
    }

    public async Task<IDataResult<RoleToListDto>> GetAsync(Guid id)
    {
        var data = mapper.Map<RoleToListDto>(await unitOfWork.RoleRepository.GetAsync(m => m.Id == id));

        return new SuccessDataResult<RoleToListDto>(data, Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid id, RoleToUpdateDto dto)
    {
        var data = mapper.Map<Role>(dto);
        data.Id = id;

        await unitOfWork.RoleRepository.ClearRolePermissionsAync(id);

        var permissions = await unitOfWork.PermissionRepository.GetListAsync(m => dto.PermissionIds!.Contains(m.Id));
        data.Permissions = permissions;
        unitOfWork.RoleRepository.UpdateRole(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<List<PermissionToListDto>>> GetPermissionsAsync(Guid id)
    {
        var datas = mapper.Map<List<PermissionToListDto>>(
            (await unitOfWork.RoleRepository.GetAsync(m => m.Id == id))!.Permissions);

        return new SuccessDataResult<List<PermissionToListDto>>(datas,
            Messages.Success.Translate());
    }
}