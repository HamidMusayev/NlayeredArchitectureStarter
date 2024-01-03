using API.Attributes;
using BLL.Abstract;
using DTO.Permission;
using DTO.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class PermissionController(IPermissionService permissionService) : Controller
{
    [SwaggerOperation(Summary = "get permissions as paginated list")]
    [Produces(typeof(IDataResult<List<PermissionToListDto>>))]
    [HttpGet("paginate")]
    public async Task<IActionResult> GetAsPaginated()
    {
        var response = await permissionService.GetAsPaginatedListAsync();
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get permissions")]
    [Produces(typeof(IDataResult<List<PermissionToListDto>>))]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var response = await permissionService.GetAsync();
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get permission by id")]
    [Produces(typeof(IDataResult<PermissionToListDto>))]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var response = await permissionService.GetAsync(id);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "create permission")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] PermissionToAddDto dto)
    {
        var response = await permissionService.AddAsync(dto);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "update permission")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id,
        [FromBody] PermissionToUpdateDto dto)
    {
        var response = await permissionService.UpdateAsync(id, dto);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "delete permission")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var response = await permissionService.SoftDeleteAsync(id);
        return Ok(response);
    }
}