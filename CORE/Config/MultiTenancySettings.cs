namespace CORE.Config;

/// <summary>
///     Multi-tenancy configuration. Every <c>Auditable</c> row is filtered by the resolved
///     tenant and new rows are stamped with the current tenant id. If a derived project is
///     single-tenant, delete this record, <see cref="ITenant" /> and its implementations, the
///     <c>AddMultiTenancy</c> call, the <c>TenantId</c> column on <c>Auditable</c>, and the
///     appsettings block.
/// </summary>
public record MultiTenancySettings
{
    /// <summary>JWT claim type read by <c>ClaimsTenantResolver</c>.</summary>
    public string ClaimName { get; set; } = "tenant_id";
}