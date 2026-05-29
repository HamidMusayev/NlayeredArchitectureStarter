using AutoMapper;

namespace BLL.Mappers;

/// <summary>
///     Reflection helper that discovers all AutoMapper <see cref="AutoMapper.Profile" /> subclasses
///     across every loaded assembly. Called at startup to register all mapping profiles without
///     manual enumeration.
/// </summary>
public static class Automapper
{
    public static IEnumerable<Type> GetAutoMapperProfilesFromAllAssemblies()
    {
        return from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from aType in assembly.GetTypes()
            where aType.IsClass && !aType.IsAbstract && aType.IsSubclassOf(typeof(Profile))
            select aType;
    }
}