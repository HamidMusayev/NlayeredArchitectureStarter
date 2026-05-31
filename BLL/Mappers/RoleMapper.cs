using DTO.Permission;
using DTO.Role;
using ENTITIES.Entities;
using Riok.Mapperly.Abstractions;

namespace BLL.Mappers;

/// <summary>
///     Compile-time mapper for <c>Role</c> ↔ DTO conversions (Mapperly source generator).
///     <para>
///         Outbound: <see cref="ToListDto" /> projects the role plus its <c>Permissions</c>
///         collection (nested mapping resolved by the local <see cref="ToPermissionListDto" />
///         helper — drop it when PermissionMapper goes Mapperly).
///     </para>
///     <para>
///         Inbound: <see cref="UpdateEntity(RoleToAddDto, Role)" /> /
///         <see cref="UpdateEntity(RoleToUpdateDto, Role)" /> copy name/key onto a caller-
///         constructed entity. <c>PermissionIds</c> is ignored on the mapping — the service
///         resolves it into the entity's <c>Permissions</c> nav collection explicitly so
///         Mapperly doesn't try to materialize Permission objects from bare Guids.
///     </para>
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class RoleMapper
{
    public partial RoleToListDto ToListDto(Role source);
    public partial List<RoleToListDto> ToListDtos(IEnumerable<Role> source);
    public partial RoleToFkDto ToFkDto(Role source);

    [MapperIgnoreSource(nameof(RoleToAddDto.PermissionIds))]
    public partial void UpdateEntity(RoleToAddDto source, Role target);

    [MapperIgnoreSource(nameof(RoleToUpdateDto.PermissionIds))]
    public partial void UpdateEntity(RoleToUpdateDto source, Role target);

    // Nested helper for the Permissions collection inside ToListDto. Drop when PermissionMapper
    // migrates to Mapperly and can be composed via DI / the partial-class graph.
    private partial PermissionToListDto ToPermissionListDto(Permission source);
}