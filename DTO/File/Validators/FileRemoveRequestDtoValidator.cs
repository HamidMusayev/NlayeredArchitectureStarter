using ENTITIES.Enums;
using FluentValidation;

namespace DTO.File.Validators;

/// <summary>
///     FluentValidation rules for <see cref="FileRemoveRequestDto" /> — required hash + ownership keys per
///     <see cref="FileType" />.
/// </summary>
public class FileRemoveRequestDtoValidator : AbstractValidator<FileRemoveRequestDto>
{
    public FileRemoveRequestDtoValidator()
    {
        RuleFor(p => p.HashName).NotNull().NotEmpty();
        RuleFor(p => p.Type).NotNull().NotEmpty();
        RuleFor(p => p.UserId).NotNull().When(p => p.Type == FileType.UserProfile);
        RuleFor(p => p.OrganizationId).NotNull().When(p => p.Type == FileType.OrganizationLogo);
    }
}