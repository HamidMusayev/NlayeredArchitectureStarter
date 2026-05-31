using CORE.Config;
using DAL.EntityFramework.Concrete;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.Interceptors;
using DAL.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     EF Core <see cref="DataContext" />, the <see cref="IUnitOfWork" />, the audit /
///     soft-delete interceptors, and the Scrutor scan that binds every
///     <c>DAL.EntityFramework.Concrete.*</c> repository to its matching interface. Always-on —
///     if a project does not need EF Core, delete this file and the call to
///     <c>AddEntityFramework</c> in <c>Program.cs</c>.
/// </summary>
public static class EntityFrameworkExtensions
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, ConfigSettings config)
    {
        // Interceptors are scoped so they can capture the per-request ICurrentUser / ITenant.
        services.TryAddScoped<SoftDeleteInterceptor>();
        services.TryAddScoped<AuditableInterceptor>();

        services.AddDbContext<DataContext>((sp, options) =>
        {
            options.UseNpgsql(config.ConnectionStrings.AppDb);
            // Order matters: SoftDelete rewrites Deleted → Modified with IsDeleted=true; the
            // Auditable pass then sees Modified and stamps DeletedAt/DeletedBy.
            options.AddInterceptors(
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<AuditableInterceptor>());
        });

        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.Scan(scan => scan
            .FromAssemblies(typeof(UserRepository).Assembly)
            .AddClasses(classes => classes.InNamespaces("DAL.EntityFramework.Concrete"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}