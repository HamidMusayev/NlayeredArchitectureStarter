using API.Attributes;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Localization;
using DTO.Responses;
using DTO.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using IResult = DTO.Responses.IResult;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class UserController(IUserService userService, IUtilService utilService, IAuthService authService)
    : Controller
{
    [SwaggerOperation(Summary = "get users as paginated list")]
    [Produces(typeof(IDataResult<List<UserToListDto>>))]
    [HttpGet("paginate")]
    public async Task<IActionResult> GetAsPaginated()
    {
        var response = await userService.GetAsPaginatedListAsync();
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get users")]
    [Produces(typeof(IDataResult<List<UserToListDto>>))]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var response = await userService.GetAsync();
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get profile info")]
    [Produces(typeof(IDataResult<List<UserToListDto>>))]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileInfo()
    {
        var userId = utilService.GetUserIdFromToken();
        if (userId is null)
            return Unauthorized(new ErrorResult(Messages.CanNotFoundUserIdInYourAccessToken.Translate()));

        var response = await userService.GetAsync(userId.Value);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "get user")]
    [Produces(typeof(IDataResult<UserToListDto>))]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var response = await userService.GetAsync(id);
        return Ok(response);
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "create user")]
    [Produces(typeof(IResult))]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] UserToAddDto dto)
    {
        var response = await userService.AddAsync(dto);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "update user")]
    [Produces(typeof(IResult))]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UserToUpdateDto dto)
    {
        var response = await userService.UpdateAsync(id, dto);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "delete user")]
    [Produces(typeof(IResult))]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var response = await userService.SoftDeleteAsync(id);
        await authService.LogoutRemovedUserAsync(id);
        return Ok(response);
    }
}