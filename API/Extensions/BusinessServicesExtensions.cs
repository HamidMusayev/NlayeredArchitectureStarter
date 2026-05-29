using BLL.Concrete;

namespace API.Extensions;

/// <summary>
///     Scrutor-based scan of the BLL project. Picks up every concrete class in
///     <c>BLL.Concrete</c> and <c>BLL.Concrete.FileTypeHandlers</c> and registers it under
///     every interface it implements as <c>Scoped</c>. Adding a new service is a matter of
///     dropping the file in those namespaces.
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

        return services;
    }
}