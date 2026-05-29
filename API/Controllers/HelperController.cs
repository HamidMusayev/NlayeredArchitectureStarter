using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nummy.CodeLogger.Data.Services;
using REFITS.ToDo;

namespace API.Controllers;

/// <summary>
///     Miscellaneous helper/demo endpoints: Refit ToDo client smoke-test and a Nummy
///     CodeLogger demo route. Not for production use — kept to show integration patterns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class HelperController(IToDoClient toDoClient, INummyCodeLoggerService nummyCodeLoggerService) : ControllerBase
{
    [HttpGet("refit/test/todo/get")]
    [AllowAnonymous]
    public async Task<IActionResult> RefitTestTodoGet()
    {
        var response = await toDoClient.Get();
        return Ok(response);
    }

    [HttpGet("nummy/test/code/log")]
    [AllowAnonymous]
    public async Task<IActionResult> NummyTestCodeLog()
    {
        await nummyCodeLoggerService.LogInfoAsync("Title of info");

        return Ok();
    }

    [HttpGet("nummy/test/exception/handle")]
    [AllowAnonymous]
    public async Task<IActionResult> NummyTestExceptionHandle()
    {
        throw new ArgumentNullException();
    }
}