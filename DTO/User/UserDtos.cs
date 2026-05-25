using DTO.File;
using DTO.Role;

namespace DTO.User;

public record UserToAddDto(
    string Username,
    string Email,
    string ContactNumber,
    string Password,
    string PasswordConfirmation,
    Guid? RoleId
);

public record UserToListDto(
    Guid Id,
    string Username,
    string Email,
    string ContactNumber,
    RoleToFkDto? Role,
    FileToListDto? ProfileFile
);

public record UserToUpdateDto(
    string Email,
    string ContactNumber,
    string Username,
    Guid? RoleId
);