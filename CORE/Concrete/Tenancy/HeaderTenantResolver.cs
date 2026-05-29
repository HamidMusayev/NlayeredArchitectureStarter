using CORE.Abstract;
using CORE.Config;
using Microsoft.AspNetCore.Http;

namespace CORE.Concrete.Tenancy;

/// <summary>
///     Reads the tenant id from a request header (default <c>X-Tenant-Id</c>). Useful when an
///     upstream gateway / reverse proxy decides the tenant before authentication runs (e.g. a
///     subdomain-routed multi-tenant app rewriting the host to a tenant id header).
/// </summary>
public sealed class HeaderTenantResolver(IHttpContextAccessor httpContextAccessor, ConfigSettings config) : ITenant
{
    public Guid? TenantId
    {
        get
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx is null) return null;

            var raw = ctx.Request.Headers[config.MultiTenancySettings.HeaderName].ToString();
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}