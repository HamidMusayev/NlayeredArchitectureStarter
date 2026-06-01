using CORE.Abstract;
using CORE.Config;
using ENTITIES.Identifiers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CORE.Concrete.Tenancy;

/// <summary>
///     Reads the tenant id from a JWT claim (default <c>tenant_id</c>). Use this when the
///     issuer encodes tenant membership in the access token — fewer trust assumptions than
///     the header-based resolver because the claim is signed.
/// </summary>
public sealed class ClaimsTenantResolver(
    IHttpContextAccessor httpContextAccessor,
    IOptions<MultiTenancySettings> options) : ITenant
{
    private readonly MultiTenancySettings _settings = options.Value;

    public TenantId? TenantId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var raw = user?.FindFirst(_settings.ClaimName)?.Value;
            return Guid.TryParse(raw, out var id) ? new TenantId(id) : null;
        }
    }
}