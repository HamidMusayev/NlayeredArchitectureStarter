namespace DTO.Permission;

/// <summary>Inbound payload for creating a permission.</summary>
public record PermissionToAddDto(string Name, string Key);

/// <summary>Outbound permission representation.</summary>
public record PermissionToListDto(Guid Id, string Name, string Key);

/// <summary>Inbound payload for renaming a permission.</summary>
public record PermissionToUpdateDto(string Name, string Key);