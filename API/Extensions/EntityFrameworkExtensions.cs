using CORE.Config;
using DAL.EntityFramework.Concrete;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     EF Core <see cref="DataContext" />, the <see cref="IUnitOfWork" />, and the Scrutor scan
///     that binds every <c>DAL.EntityFramework.Concrete.*</c> repository to its matching
///     interface. Always-on — if a project does not need EF Core, delete this file and the call
///     to <c>AddEntityFramework</c> in <c>Program.cs</c>.
/// </summary>
public static class EntityFrameworkExtensions
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, ConfigSettings config)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(config.ConnectionStrings.AppDb));

        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.Scan(scan => scan
            .FromAssemblies(typeof(UserRepository).Assembly)
            .AddClasses(classes => classes.InNamespaces("DAL.EntityFramework.Concrete"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
