using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nummy.CodeLogger.Data.Services;
using REFITS.Clients;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class HelperController(IToDoClient toDoClient, INummyCodeLoggerService nummyCodeLoggerService) : Controller
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