﻿using SOURCES.Builders.Abstract;
using SOURCES.Helpers;
using SOURCES.Models;
using SOURCES.Workers;

namespace SOURCES.Builders;

// ReSharper disable once UnusedType.Global
public class ControllerBuilder : ISourceBuilder
{
    public void BuildSourceFile(List<Entity> entities)
    {
        entities.ForEach(model =>
            SourceBuilder.Instance.AddSourceFile(Constants.ControllerPath, $"{model.Name}Controller.cs",
                BuildSourceText(model, null)));
    }

    public string BuildSourceText(Entity? entity, List<Entity>? entities)
    {
        var text = """
                   using API.Filters;
                   using API.Attributes;
                   using BLL.Abstract;
                   using DTO.Responses;
                   using DTO.{entityName};
                   using Microsoft.AspNetCore.Authentication.JwtBearer;
                   using Microsoft.AspNetCore.Authorization;
                   using Microsoft.AspNetCore.Mvc;
                   using Swashbuckle.AspNetCore.Annotations;
                   using IResult = DTO.Responses.IResult;

                   namespace API.Controllers;

                   [Route("api/[controller]")]
                   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
                   [ValidateToken]
                   public class {entityName}Controller : Controller
                   {
                       private readonly I{entityName}Service _{entityNameLower}Service;
                       public {entityName}Controller(I{entityName}Service {entityNameLower}Service)
                       {
                           _{entityNameLower}Service = {entityNameLower}Service;
                       }
                   
                       [SwaggerOperation(Summary = "get paginated list")]
                       [Produces(typeof(IDataResult<List<{entityName}ToListDto>>))]
                       [HttpGet("paginate")]
                       public async Task<IActionResult> GetAsPaginated()
                       {
                           var response = await _{entityNameLower}Service.GetAsPaginatedListAsync();
                           return Ok(response);
                       }
                   
                       [SwaggerOperation(Summary = "get list")]
                       [Produces(typeof(IDataResult<List<{entityName}ToListDto>>))]
                       [HttpGet]
                       public async Task<IActionResult> Get()
                       {
                           var response = await _{entityNameLower}Service.GetAsync();
                           return Ok(response);
                       }
                   
                       [SwaggerOperation(Summary = "get data")]
                       [Produces(typeof(IDataResult<{entityName}ToListDto>))]
                       [HttpGet("{id}")]
                       public async Task<IActionResult> Get([FromRoute] Guid id)
                       {
                           var response = await _{entityNameLower}Service.GetAsync(id);
                           return Ok(response);
                       }
                   
                       [SwaggerOperation(Summary = "create")]
                       [Produces(typeof(IResult))]
                       [HttpPost]
                       public async Task<IActionResult> Add([FromBody] {entityName}ToAddDto dto)
                       {
                           var response = await _{entityNameLower}Service.AddAsync(dto);
                           return Ok(response);
                       }
                   
                       [SwaggerOperation(Summary = "update")]
                       [Produces(typeof(IResult))]
                       [HttpPut("{id}")]
                       public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] {entityName}ToUpdateDto dto)
                       {
                           var response = await _{entityNameLower}Service.UpdateAsync(id, dto);
                           return Ok(response);
                       }
                   
                       [SwaggerOperation(Summary = "delete")]
                       [Produces(typeof(IResult))]
                       [HttpDelete("{id}")]
                       public async Task<IActionResult> Delete([FromRoute] Guid id)
                       {
                           var response = await _{entityNameLower}Service.SoftDeleteAsync(id);
                           return Ok(response);
                       }
                   }

                   """;

        text = text.Replace("{entityName}", entity!.Name);
        text = text.Replace("{entityNameLower}", entity.Name.FirstCharToLowerCase());
        return text;
    }
}