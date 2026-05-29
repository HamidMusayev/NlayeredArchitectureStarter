using ENTITIES.Enums;
using FluentValidation;

namespace DTO.File.Validators;

/// <summary>
///     FluentValidation rules for <see cref="FileUploadRequestDto" />. Enforces presence, a
///     5 MB hard cap, and per-<see cref="FileType" /> ownership keys (UserId or OrganizationId).
///     Stricter per-extension rules live in <c>FileController.UploadPolicy</c>.
/// </summary>
public class FileUploadRequestDtoValidator : AbstractValidator<FileUploadRequestDto>
{
    public FileUploadRequestDtoValidator()
    {
        RuleFor(p => p.File).NotNull().Must(p => p.Length < 5242880); // ~ 5 mb
        RuleFor(p => p.Type).NotNull().NotEmpty();
        RuleFor(p => p.UserId).NotNull().When(p => p.Type == FileType.UserProfile);
        RuleFor(p => p.OrganizationId).NotNull().When(p => p.Type == FileType.OrganizationLogo);
    }
}