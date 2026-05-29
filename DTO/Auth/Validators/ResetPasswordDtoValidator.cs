using FluentValidation;

namespace DTO.Auth.Validators;

/// <summary>
///     FluentValidation rules for <see cref="ResetPasswordDto" />: email format + password complexity + confirmation
///     match.
/// </summary>
public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(p => p.Email).NotEmpty().EmailAddress();
        RuleFor(p => p.Password)
            .NotEmpty()
            .Matches("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")
            .WithMessage("Password must contain uppercase, lowercase, digit and special character");
        RuleFor(p => p.PasswordConfirmation)
            .Equal(p => p.Password)
            .WithMessage("Passwords do not match");
    }
}