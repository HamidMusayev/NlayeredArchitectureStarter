using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using CORE.Abstract;
using DAL.EntityFramework.Conversions;
using DAL.EntityFramework.Seeds;
using ENTITIES.Entities;
using ENTITIES.Entities.Generic;
using ENTITIES.Identifiers;
using Microsoft.EntityFrameworkCore;
using File = ENTITIES.Entities.File;

namespace DAL.EntityFramework.Context;

public class DataContext(
    DbContextOptions<DataContext> options,
    ITenant tenant)
    : DbContext(options)
{
    private static readonly MethodInfo BuildFilterMethod = typeof(DataContext)
        .GetMethod(nameof(BuildAuditableFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    ///     Tells the C# compiler that the EF Core base class populates the <c>required</c>
    ///     DbSets on our behalf. Lets callers (including the design-time factory and the
    ///     standard DI container) construct without filling in every property by hand.
    /// </summary>
    [SetsRequiredMembers]
    public DataContext(DbContextOptions<DataContext> options) : this(options, null!)
    {
    }

    public required DbSet<User> Users { get; set; }
    public required DbSet<File> Files { get; set; }
    public required DbSet<Organization> Organizations { get; set; }
    public required DbSet<Role> Roles { get; set; }
    public required DbSet<Permission> Permissions { get; set; }
    public required DbSet<Token> Tokens { get; set; }
    public required DbSet<OutboxMessage> OutboxMessages { get; set; }
    public required DbSet<AuditLog> AuditLogs { get; set; }

    /// <summary>
    ///     EF Core re-evaluates this property on every query when it's referenced inside a
    ///     global query filter (the standard "dynamic filter" pattern). Returns null when the
    ///     resolver can't determine a tenant — the filter then short-circuits to "see everything".
    /// </summary>
    public TenantId? CurrentTenantId => tenant.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    /* migration commands
      dotnet ef --startup-project ../API migrations add initial --context DataContext
      dotnet ef --startup-project ../API database update --context DataContext
    */

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Strongly-typed identifiers — Postgres still sees plain `uuid` on the wire and at rest;
        // the C# side gets a record-struct wrapper that won't implicitly convert to/from a raw
        // Guid. Apply once globally so every property of type TenantId picks up the converter
        // without per-entity HasConversion(...) plumbing.
        configurationBuilder.Properties<TenantId>().HaveConversion<TenantIdValueConverter>();
        configurationBuilder.Properties<RoleId>().HaveConversion<RoleIdValueConverter>();
        configurationBuilder.Properties<OrganizationId>().HaveConversion<OrganizationIdValueConverter>();
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdValueConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite global filter — soft-delete + optional per-tenant scoping.
        // Applied to every Auditable derivative; the tenant clause short-circuits when
        // multi-tenancy is disabled because CurrentTenantId returns null.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(Auditable).IsAssignableFrom(t.ClrType)))
            ApplyGlobalFilterFor(modelBuilder, entityType.ClrType);

        // Outbox dispatcher's "pending oldest-first" scan.
        modelBuilder.Entity<OutboxMessage>()
            .HasIndex(o => new { o.ProcessedOnUtc, o.OccurredOnUtc });

        // Refresh-token rotation lookup paths. Rotation queries by hash only — the plaintext
        // never lands on disk.
        modelBuilder.Entity<Token>().HasIndex(t => t.FamilyId);
        modelBuilder.Entity<Token>().HasIndex(t => t.RefreshTokenHash);
        // Per-request validation lookup keys on Jti (the JWT's jti claim). Unique because each
        // issuance gets a fresh Guid and a collision would mean two tokens claiming the same id.
        modelBuilder.Entity<Token>().HasIndex(t => t.Jti).IsUnique();

        // AuditLog lookups: by-user and time-bounded scans for the retention prune job.
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.OccurredAt);
        modelBuilder.Entity<AuditLog>().HasIndex(a => new { a.UserId, a.OccurredAt });
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.Action);

        DataSeed.Seed(modelBuilder);
    }

    private void ApplyGlobalFilterFor(ModelBuilder modelBuilder, Type entityType)
    {
        var closed = BuildFilterMethod.MakeGenericMethod(entityType);
        var lambda = (LambdaExpression)closed.Invoke(this, null)!;
        modelBuilder.Entity(entityType).HasQueryFilter(lambda);
    }

    /// <summary>
    ///     Composite global query filter. <c>CurrentTenantId</c> is captured via <c>this</c>
    ///     so EF re-evaluates it per query — the filter behaves dynamically without rebuilding
    ///     the model.
    /// </summary>
    private LambdaExpression BuildAuditableFilter<TEntity>() where TEntity : Auditable
    {
        Expression<Func<TEntity, bool>> filter = e =>
            !e.IsDeleted && (CurrentTenantId == null || e.TenantId == CurrentTenantId);
        return filter;
    }
}