using API.Attributes;
using BLL.Abstract;
using DTO.Responses;
using DTO.Role;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using IResult = DTO.Responses.IResult;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class RoleController(IRoleService roleService) : Controller
{
    [SwaggerOperation(Summary = "get roles")]
    [Produces(typeof(IDataResult<List<RoleToListDto>>))]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var response = await roleService.GetAsync();
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get role")]
    [Produces(typeof(IDataResult<RoleToListDto>))]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var response = await roleService.GetAsync(id);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get role permissions")]
    [Produces(typeof(IDataResult<RoleToListDto>))]
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetRolePermissions([FromRoute] Guid id)
    {
        var response = await roleService.GetPermissionsAsync(id);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "create role")]
    [Produces(typeof(IResult))]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] RoleToAddDto dto)
    {
        var response = await roleService.AddAsync(dto);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "update role")]
    [Produces(typeof(IResult))]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RoleToUpdateDto dto)
    {
        var response = await roleService.UpdateAsync(id, dto);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "delete role")]
    [Produces(typeof(IResult))]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var response = await roleService.SoftDeleteAsync(id);
        return Ok(response);
    }
}