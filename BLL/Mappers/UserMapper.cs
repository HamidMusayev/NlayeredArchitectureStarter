using DTO.File;
using DTO.Role;
using DTO.User;
using ENTITIES.Entities;
using Riok.Mapperly.Abstractions;
using File = ENTITIES.Entities.File;

namespace BLL.Mappers;

/// <summary>
///     Compile-time mapper for <c>User</c> ↔ DTO conversions (Mapperly source generator).
///     Replaces the AutoMapper <c>Profile</c> shape with build-time-checked mappings.
///     <para>
///         Inbound DTOs use the <c>UpdateEntity(source, target)</c> shape rather than
///         <c>Map&lt;User&gt;(dto)</c>. Reason: <c>User</c> has <c>required</c> fields
///         (<c>Password</c>, <c>Salt</c>) that DTOs don't carry, and constructing a fresh entity
///         from a DTO would either explode at compile time or — worse — silently zero out
///         fields the DTO doesn't know about (the bug AutoMapper used to mask in
///         <c>UserService.UpdateAsync</c>, which would have wiped <c>FailedLoginAttempts</c> and
///         <c>LockedUntil</c> on every profile edit).
///     </para>
///     <para>
///         Nested <see cref="Role" /> and <see cref="File" /> projections are included here as
///         private helpers so this mapper is self-contained. When the matching dedicated
///         mappers (RoleMapper, FileMapper) are migrated to Mapperly, drop the helpers and let
///         Mapperly compose via the partial-class graph instead.
///     </para>
///     <para>
///         <b>Lifetime</b>: stateless — registered as <c>Singleton</c> in
///         <c>BusinessServicesExtensions</c>. It lives in <c>BLL.Mappers</c> (not
///         <c>BLL.Concrete</c>) so Scrutor's auto-scan won't miscategorize it.
///     </para>
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class UserMapper
{
    // Outbound: entity → DTO. Pure projection.
    public partial UserToListDto ToListDto(User source);
    public partial List<UserToListDto> ToListDtos(IEnumerable<User> source);

    // Inbound: DTO → existing entity. Caller constructs / loads the target; Mapperly copies
    // only the matching properties, leaving Password/Salt/audit fields untouched.
    // Password is explicitly ignored on the Add path — the service hashes it externally and
    // sets it during construction. Leaving Mapperly to copy DTO.Password would overwrite the
    // hashed value with the plaintext (security regression).
    [MapperIgnoreTarget(nameof(User.Password))]
    [MapperIgnoreTarget(nameof(User.Salt))]
    [MapperIgnoreSource(nameof(UserToAddDto.Password))]
    [MapperIgnoreSource(nameof(UserToAddDto.PasswordConfirmation))]
    public partial void UpdateEntity(UserToAddDto source, User target);

    public partial void UpdateEntity(UserToUpdateDto source, User target);

    // Nested-type helpers used by ToListDto. Remove when RoleMapper / FileMapper are
    // Mapperly-ized.
    private partial RoleToFkDto? ToRoleFk(Role? source);
    private partial FileToListDto? ToFileListDto(File? source);
}