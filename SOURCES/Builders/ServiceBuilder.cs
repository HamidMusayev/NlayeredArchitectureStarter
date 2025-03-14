﻿using SOURCES.Builders.Abstract;
using SOURCES.Models;
using SOURCES.Workers;

namespace SOURCES.Builders;

// ReSharper disable once UnusedType.Global
public class ServiceBuilder : ISourceBuilder
{
    public void BuildSourceFile(List<Entity> entities)
    {
        entities.ForEach(model =>
            SourceBuilder.Instance.AddSourceFile(Constants.ServicePath, $"{model.Name}Service.cs",
                BuildSourceText(model, null)));
    }

    public string BuildSourceText(Entity? entity, List<Entity>? entities)
    {
        var text = """
                   using AutoMapper;
                   using BLL.Abstract;
                   using CORE.Abstract;
                   using CORE.Localization;
                   using DTO.Responses;
                   using DTO.{entityName};
                   using ENTITIES.Entities{entityPath};
                   using DAL.EntityFramework.Utility;
                   using DAL.EntityFramework.UnitOfWork;

                   namespace BLL.Concrete;

                   public class {entityName}Service : I{entityName}Service
                   {
                       private readonly IMapper _mapper;
                       private readonly IUnitOfWork _unitOfWork;
                       private readonly IUtilService _utilService;
                       public {entityName}Service(IMapper mapper, IUnitOfWork unitOfWork, IUtilService utilService)
                       {
                           _mapper = mapper;
                           _unitOfWork = unitOfWork;
                           _utilService = utilService;
                       }
                   
                       public async Task<IResult> AddAsync({entityName}ToAddDto dto)
                       {
                           var data = _mapper.Map<{entityName}>(dto);
                   
                           await _unitOfWork.{entityName}Repository.AddAsync(data);
                           await _unitOfWork.CommitAsync();
                   
                           return new SuccessResult(Messages.Success.Translate());
                       }
                   
                       public async Task<IResult> SoftDeleteAsync(Guid id)
                       {
                           var data = await _unitOfWork.{entityName}Repository.GetAsync(m => m.Id == id);
                   
                           _unitOfWork.{entityName}Repository.SoftDelete(data);
                           await _unitOfWork.CommitAsync();
                   
                           return new SuccessResult(Messages.Success.Translate());
                       }
                   
                       public async Task<IDataResult<PaginatedList<{entityName}ToListDto>>> GetAsPaginatedListAsync()
                       {
                           var datas = _unitOfWork.{entityName}Repository.GetList();
                           var paginationDto = _utilService.GetPagination();
                   
                           var response = await PaginatedList<{entityName}>.CreateAsync(datas.OrderBy(m => m.Id), paginationDto.PageIndex, paginationDto.PageSize);
                   
                           var responseDto = new PaginatedList<{entityName}ToListDto>(_mapper.Map<List<{entityName}ToListDto>>(response.Datas), response.TotalRecordCount, response.PageIndex, paginationDto.PageSize);
                   
                           return new SuccessDataResult<PaginatedList<{entityName}ToListDto>>(responseDto, Messages.Success.Translate());
                       }
                   
                       public async Task<IDataResult<List<{entityName}ToListDto>>> GetAsync()
                       {
                           var datas = _mapper.Map<List<{entityName}ToListDto>>(await _unitOfWork.{entityName}Repository.GetListAsync());
                   
                           return new SuccessDataResult<List<{entityName}ToListDto>>(datas, Messages.Success.Translate());
                       }
                   
                       public async Task<IDataResult<{entityName}ToListDto>> GetAsync(Guid id)
                       {
                           var data = _mapper.Map<{entityName}ToListDto>(await _unitOfWork.{entityName}Repository.GetAsync(m => m.Id == id));
                   
                           return new SuccessDataResult<{entityName}ToListDto>(data, Messages.Success.Translate());
                       }
                   
                       public async Task<IResult> UpdateAsync(Guid id, {entityName}ToUpdateDto dto)
                       {
                           var data = _mapper.Map<{entityName}>(dto);
                           data.Id = id;
                   
                           _unitOfWork.{entityName}Repository.Update(data);
                           await _unitOfWork.CommitAsync();
                   
                           return new SuccessResult(Messages.Success.Translate());
                       }
                   }

                   """;

        text = text.Replace("{entityName}", entity!.Name);
        text = text.Replace("{entityPath}", !string.IsNullOrEmpty(entity!.Path) ? $".{entity.Path}" : string.Empty);
        return text;
    }
}