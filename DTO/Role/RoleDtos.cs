using DTO.Permission;

namespace DTO.Role;

/// <summary>Inbound payload for creating a role with an initial permission set.</summary>
public record RoleToAddDto(string Name, string Key, List<Guid> PermissionIds);

/// <summary>Outbound role with its expanded permission collection.</summary>
public record RoleToListDto(Guid Id, string Name, string Key, List<PermissionToListDto> Permissions);

/// <summary>Foreign-key shape: just enough role data to render as a label on a related entity.</summary>
public record RoleToFkDto(Guid Id, string Name, string Key);

/// <summary>Inbound payload for editing a role. Permission set is replaced wholesale by <see cref="PermissionIds" />.</summary>
public record RoleToUpdateDto(string Name, string Key, List<Guid> PermissionIds);