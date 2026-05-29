using DTO.Responses;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
///     Demonstration controller that exposes the same route under API versions 1.0 and 2.0.
///     Shows how <c>[ApiVersion]</c> and <c>[MapToApiVersion]</c> work with the versioned route
///     template. Remove or replace with a real versioned endpoint in derived projects.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class VersioningController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new SuccessResult());
    }

    [MapToApiVersion("2.0")]
    [HttpGet("{id}")]
    public IActionResult Get([FromRoute] int id)
    {
        return Ok(new SuccessResult());
    }
}