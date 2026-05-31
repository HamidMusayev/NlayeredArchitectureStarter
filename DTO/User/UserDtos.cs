using DTO.File;
using DTO.Role;
using ENTITIES.Identifiers;

namespace DTO.User;

/// <summary>
///     Inbound payload for user registration / admin-create. Plaintext password is hashed in the service layer before
///     persistence.
/// </summary>
public record UserToAddDto(
    string Username,
    string Email,
    string ContactNumber,
    string Password,
    string PasswordConfirmation,
    RoleId? RoleId
);

/// <summary>Outbound user representation. Strips the password/salt and embeds role + profile-picture summaries.</summary>
public record UserToListDto(
    Guid Id,
    string Username,
    string Email,
    string ContactNumber,
    RoleToFkDto? Role,
    FileToListDto? ProfileFile
);

/// <summary>Inbound payload for user profile edits. Password changes go through a dedicated reset endpoint.</summary>
public record UserToUpdateDto(
    string Email,
    string ContactNumber,
    string Username,
    RoleId? RoleId
);