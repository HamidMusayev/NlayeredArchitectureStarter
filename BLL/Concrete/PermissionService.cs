using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DAL.EntityFramework.Utility;
using DTO.Permission;
using DTO.Responses;
using ENTITIES.Entities;

namespace BLL.Concrete;

public class PermissionService(
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPaginationContext paginationContext)
    : IPermissionService
{
    public async Task<IResult> AddAsync(PermissionToAddDto addDto)
    {
        var data = mapper.Map<Permission>(addDto);

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

    public async Task<IDataResult<PaginatedList<PermissionToListDto>>> GetAsPaginatedListAsync()
    {
        var datas = permissionRepository.GetList();
        var paginationDto = paginationContext.GetPagination();
        var response = await PaginatedList<Permission>.CreateAsync(datas.OrderBy(m => m.Id), paginationDto.PageIndex,
            paginationDto.PageSize);

        var responseDto = new PaginatedList<PermissionToListDto>(mapper.Map<List<PermissionToListDto>>(response.Items),
            response.TotalRecordCount, response.PageIndex, response.TotalPageCount);

        return new SuccessDataResult<PaginatedList<PermissionToListDto>>(responseDto, Messages.Success.Translate());
    }

    public async Task<IDataResult<List<PermissionToListDto>>> GetAsync()
    {
        var datas = mapper.Map<List<PermissionToListDto>>(await permissionRepository.GetListAsync());

        return new SuccessDataResult<List<PermissionToListDto>>(datas, Messages.Success.Translate());
    }

    public async Task<IDataResult<PermissionToListDto>> GetAsync(Guid id)
    {
        var datas = mapper.Map<PermissionToListDto>(await permissionRepository.GetAsync(m => m.Id == id));

        return new SuccessDataResult<PermissionToListDto>(datas, Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid permissionId, PermissionToUpdateDto updateDto)
    {
        var data = mapper.Map<Permission>(updateDto);
        data.Id = permissionId;

        permissionRepository.Update(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}