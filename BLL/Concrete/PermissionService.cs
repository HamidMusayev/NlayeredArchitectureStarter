using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.UnitOfWork;
using DAL.EntityFramework.Utility;
using DTO.Permission;
using DTO.Responses;
using ENTITIES.Entities;

namespace BLL.Concrete;

public class PermissionService(IUnitOfWork unitOfWork, IMapper mapper, IUtilService utilService)
    : IPermissionService
{
    public async Task<IResult> AddAsync(PermissionToAddDto dto)
    {
        var data = mapper.Map<Permission>(dto);

        await unitOfWork.PermissionRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await unitOfWork.PermissionRepository.GetAsync(m => m.Id == id);
        unitOfWork.PermissionRepository.SoftDelete(data!);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<PaginatedList<PermissionToListDto>>> GetAsPaginatedListAsync()
    {
        var datas = unitOfWork.PermissionRepository.GetList();
        var paginationDto = utilService.GetPagination();
        var response = await PaginatedList<Permission>.CreateAsync(datas.OrderBy(m => m.Id), paginationDto.PageIndex,
            paginationDto.PageSize);

        var responseDto = new PaginatedList<PermissionToListDto>(mapper.Map<List<PermissionToListDto>>(response.Datas),
            response.TotalRecordCount, response.PageIndex, response.TotalPageCount);

        return new SuccessDataResult<PaginatedList<PermissionToListDto>>(responseDto, Messages.Success.Translate());
    }

    public async Task<IDataResult<List<PermissionToListDto>>> GetAsync()
    {
        var datas = mapper.Map<List<PermissionToListDto>>(await unitOfWork.PermissionRepository.GetListAsync());

        return new SuccessDataResult<List<PermissionToListDto>>(datas, Messages.Success.Translate());
    }

    public async Task<IDataResult<PermissionToListDto>> GetAsync(Guid id)
    {
        var datas = mapper.Map<PermissionToListDto>(await unitOfWork.PermissionRepository.GetAsync(m => m.Id == id));

        return new SuccessDataResult<PermissionToListDto>(datas, Messages.Success.Translate());
    }

    public async Task<IResult> UpdateAsync(Guid permissionId, PermissionToUpdateDto dto)
    {
        var data = mapper.Map<Permission>(dto);
        data.Id = permissionId;

        unitOfWork.PermissionRepository.Update(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}