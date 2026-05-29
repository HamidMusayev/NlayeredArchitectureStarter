using CORE.Helpers;
using ENTITIES.Entities;
using ENTITIES.Enums;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Seeds;

/// <summary>
///     Seeds one <see cref="Role" /> row per <see cref="UserType" /> enum member via EF Core's
///     <c>HasData</c>. Role <c>Name</c> is pulled from the enum's
///     <see cref="System.ComponentModel.DescriptionAttribute" />,
///     falling back to the member name when no attribute is present.
/// </summary>
public static class RoleSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var roles = Enum.GetValues<UserType>()
            .Select(e => new Role
            {
                Id = Guid.NewGuid(),
                Key = Enum.GetName(e)!,
                Name = EnumHelper.GetEnumDescription(e)
            })
            .ToArray();

        modelBuilder.Entity<Role>().HasData(roles);
    }
}