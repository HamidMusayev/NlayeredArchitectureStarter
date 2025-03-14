﻿using System.ComponentModel.DataAnnotations;
using DTO.File;
using DTO.Role;

namespace DTO.User;

public record UserToAddDto(
    string Username,
    string Email,
    string ContactNumber,
    [property: RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$",
        ErrorMessage = "Şifrə formatı düzgün deyil")]
    string Password,
    [property: Compare("Password", ErrorMessage = "Şifrələr eyni deyil")]
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