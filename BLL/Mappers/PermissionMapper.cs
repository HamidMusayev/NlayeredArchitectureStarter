using DTO.Permission;
using ENTITIES.Entities;
using Riok.Mapperly.Abstractions;

namespace BLL.Mappers;

/// <summary>
///     Compile-time mapper for <c>Permission</c> ↔ DTO conversions (Mapperly source generator).
///     <see cref="UpdateEntity" /> writes to a caller-constructed target so audit / nav fields
///     the DTOs don't carry stay untouched.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PermissionMapper
{
    public partial PermissionToListDto ToListDto(Permission source);
    public partial List<PermissionToListDto> ToListDtos(IEnumerable<Permission> source);

    public partial void UpdateEntity(PermissionToAddDto source, Permission target);
    public partial void UpdateEntity(PermissionToUpdateDto source, Permission target);
}