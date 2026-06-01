using CORE.Abstract;
using CORE.Config;
using DTO.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CORE.Concrete;

/// <summary>
///     Default <see cref="IPaginationContext" /> — reads PageIndex / PageSize from request
///     headers whose names come from <see cref="RequestSettings" />. Allows BLL code to depend
///     on a simple pagination abstraction without touching <c>IHttpContextAccessor</c> directly.
/// </summary>
public class HttpPaginationContext(IOptions<RequestSettings> options, IHttpContextAccessor accessor)
    : IPaginationContext
{
    private readonly RequestSettings _settings = options.Value;

    public PaginationDto GetPagination()
    {
        var headers = accessor.HttpContext?.Request.Headers;
        var pageIndex = Convert.ToInt32(headers?[_settings.PageIndex]);
        var pageSize = Convert.ToInt32(headers?[_settings.PageSize]);

        return new PaginationDto
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}