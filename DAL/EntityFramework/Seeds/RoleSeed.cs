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
///     <para>
///         Ids are <b>literal, deterministic</b> per enum value — derived from the enum's int.
///         <c>Guid.NewGuid()</c> here would regenerate every model build, and EF would scaffold
///         a destructive <c>DeleteData</c>+<c>InsertData</c> swap into every new migration.
///         If you add a new <see cref="UserType" /> value, add a matching entry here in the same
///         PR.
///     </para>
/// </summary>
public static class RoleSeed
{
    // Stable Guid template: zero-prefix + single-byte enum suffix. Covers up to 255 roles.
    private static Guid IdFor(UserType type)
    {
        return new Guid($"00000000-0000-0000-0000-0000000000{(int)type:x2}");
    }

    public static void Seed(ModelBuilder modelBuilder)
    {
        var roles = Enum.GetValues<UserType>()
            .Select(e => new Role
            {
                Id = IdFor(e),
                Key = Enum.GetName(e)!,
                Name = EnumHelper.GetEnumDescription(e)
            })
            .ToArray();

        modelBuilder.Entity<Role>().HasData(roles);
    }
}