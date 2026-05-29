using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REFITS.ToDo;

namespace API.Controllers;

/// <summary>
///     Miscellaneous helper/demo endpoints: Refit client
///     CodeLogger demo route. Not for production use — kept to show integration patterns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RefitDemoController(IToDoClient toDoClient) : ControllerBase
{
    [HttpGet("refit/test/todo/get")]
    [AllowAnonymous]
    public async Task<IActionResult> RefitTestTodoGet()
    {
        var response = await toDoClient.Get();
        return Ok(response);
    }
}