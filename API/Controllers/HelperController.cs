using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REFITS.Clients;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class HelperController(IToDoClient toDoClient) : Controller
{
    [HttpGet("refit/test/todo/get")]
    [AllowAnonymous]
    public async Task<IActionResult> RefitTestTodoGet()
    {
        var response = await toDoClient.Get();
        return Ok(response);
    }
}