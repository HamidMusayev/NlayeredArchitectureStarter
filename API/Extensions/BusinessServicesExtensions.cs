using BLL.Concrete;
using BLL.Mappers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Scrutor-based scan of the BLL project. Picks up every concrete class in
///     <c>BLL.Concrete</c> and <c>BLL.Concrete.FileTypeHandlers</c> and registers it under
///     every interface it implements as <c>Scoped</c>. Adding a new service is a matter of
///     dropping the file in those namespaces.
///     <para>
///         Mapperly-generated mappers (under <c>BLL.Mappers</c>) are registered manually as
///         singletons here. They're stateless source-generated classes — singleton avoids
///         per-request allocation and there's no scoped state to carry.
///     </para>
/// </summary>
public static class BusinessServicesExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(UserService).Assembly)
            .AddClasses(classes => classes.InNamespaces(
                "BLL.Concrete",
                "BLL.Concrete.FileTypeHandlers"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Mapperly mappers — stateless source-generated singletons.
        services.TryAddSingleton<UserMapper>();
        services.TryAddSingleton<RoleMapper>();
        services.TryAddSingleton<PermissionMapper>();
        services.TryAddSingleton<FileMapper>();
        services.TryAddSingleton<OrganizationMapper>();
        services.TryAddSingleton<TokenMapper>();

        return services;
    }
}