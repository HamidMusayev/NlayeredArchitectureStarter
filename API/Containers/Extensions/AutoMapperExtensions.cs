using BLL.Mappers;

namespace API.Containers.Extensions;

/// <summary>
///     Registers AutoMapper by scanning all loaded assemblies for <c>Profile</c> subclasses via
///     <see cref="BLL.Mappers.Automapper.GetAutoMapperProfilesFromAllAssemblies" />.
/// </summary>
public static class AutoMapperExtensions
{
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { },
            Automapper.GetAutoMapperProfilesFromAllAssemblies().ToArray());

        return services;
    }
}