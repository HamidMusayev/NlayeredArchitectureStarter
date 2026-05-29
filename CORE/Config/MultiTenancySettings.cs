namespace CORE.Config;

/// <summary>
///     Multi-tenancy toggle + resolver selection. Default is <c>IsEnabled = false</c> — derived
///     single-tenant projects see no behavioural change. When enabled, every <c>Auditable</c>
///     row is filtered by the resolved tenant and new rows are stamped with the current tenant id.
/// </summary>
public record MultiTenancySettings
{
    public bool IsEnabled { get; set; }

    public TenantResolverProvider Provider { get; set; } = TenantResolverProvider.Header;

    /// <summary>HTTP header name read by the Header resolver.</summary>
    public string HeaderName { get; set; } = "X-Tenant-Id";

    /// <summary>JWT claim type read by the Claims resolver.</summary>
    public string ClaimName { get; set; } = "tenant_id";
}

public enum TenantResolverProvider
{
    Header = 0,
    Claims = 1
}