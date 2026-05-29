using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Filters;

/// <summary>
///     Hangfire dashboard authorization filter that grants access only to requests carrying a
///     valid JWT bearer identity. Prevents anonymous browsing of the job queue in production.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.Identity.AuthenticationType == JwtBearerDefaults.AuthenticationScheme;
    }
}