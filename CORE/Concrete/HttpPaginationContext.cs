using CORE.Abstract;
using CORE.Config;
using DTO.Helper;
using Microsoft.AspNetCore.Http;

namespace CORE.Concrete;

public class HttpPaginationContext(ConfigSettings config, IHttpContextAccessor accessor) : IPaginationContext
{
    public PaginationDto GetPagination()
    {
        var headers = accessor.HttpContext?.Request.Headers;
        var pageIndex = Convert.ToInt32(headers?[config.RequestSettings.PageIndex]);
        var pageSize = Convert.ToInt32(headers?[config.RequestSettings.PageSize]);

        return new PaginationDto
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}