using System.Linq.Expressions;
using System.Reflection;
using CORE.Abstract;
using DAL.EntityFramework.Seeds;
using ENTITIES.Entities;
using ENTITIES.Entities.Generic;
using Microsoft.EntityFrameworkCore;
using File = ENTITIES.Entities.File;

namespace DAL.EntityFramework.Context;

public class DataContext(
    DbContextOptions<DataContext> options,
    ICurrentUser currentUser,
    ITenant tenant)
    : DbContext(options)
{
    private static readonly MethodInfo BuildFilterMethod = typeof(DataContext)
        .GetMethod(nameof(BuildAuditableFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public required DbSet<User> Users { get; set; }
    public required DbSet<File> Files { get; set; }
    public required DbSet<Organization> Organizations { get; set; }
    public required DbSet<Role> Roles { get; set; }
    public required DbSet<Permission> Permissions { get; set; }
    public required DbSet<Token> Tokens { get; set; }
    public required DbSet<OutboxMessage> OutboxMessages { get; set; }

    /// <summary>
    ///     EF Core re-evaluates this property on every query when it's referenced inside a
    ///     global query filter (the standard "dynamic filter" pattern). Returns null when the
    ///     resolver can't determine a tenant — the filter then short-circuits to "see everything".
    /// </summary>
    public Guid? CurrentTenantId => tenant.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /* migration commands
      dotnet ef --startup-project ../API migrations add initial --context DataContext
      dotnet ef --startup-project ../API database update --context DataContext
    */

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

        // Refresh-token rotation lookup paths.
        modelBuilder.Entity<Token>().HasIndex(t => t.FamilyId);
        modelBuilder.Entity<Token>().HasIndex(t => t.RefreshToken);

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

    private void SetAuditProperties()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Auditable && e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    var added = (Auditable)entityEntry.Entity;
                    added.CreatedAt = DateTime.UtcNow;
                    added.CreatedById = currentUser.UserId;
                    // Stamp tenant when the entity didn't already specify one.
                    if (added.TenantId is null)
                        added.TenantId = tenant.TenantId;
                    break;
                case EntityState.Modified:
                {
                    var modified = (Auditable)entityEntry.Entity;
                    Entry(modified).Property(p => p.CreatedAt).IsModified = false;
                    Entry(modified).Property(p => p.CreatedById).IsModified = false;
                    // TenantId is immutable post-creation — never let an update rewrite it.
                    Entry(modified).Property(p => p.TenantId).IsModified = false;

                    if (modified.IsDeleted)
                    {
                        Entry(modified).Property(p => p.ModifiedBy).IsModified = false;
                        Entry(modified).Property(p => p.ModifiedAt).IsModified = false;

                        modified.DeletedAt = DateTime.UtcNow;
                        modified.DeletedBy = currentUser.UserId;
                    }
                    else
                    {
                        modified.ModifiedAt = DateTime.UtcNow;
                        modified.ModifiedBy = currentUser.UserId;
                    }

                    break;
                }
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }
}