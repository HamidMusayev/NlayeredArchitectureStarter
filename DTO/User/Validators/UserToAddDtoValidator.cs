using FluentValidation;

namespace DTO.User.Validators;

public class UserToAddDtoValidator : AbstractValidator<UserToAddDto>
{
    public UserToAddDtoValidator()
    {
        RuleFor(p => p.Username).NotEmpty();
        RuleFor(p => p.Email).NotEmpty().EmailAddress();
        RuleFor(p => p.ContactNumber).NotEmpty();
        RuleFor(p => p.Password)
            .NotEmpty()
            .Matches("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")
            .WithMessage("Password must contain uppercase, lowercase, digit and special character");
        RuleFor(p => p.PasswordConfirmation)
            .Equal(p => p.Password)
            .WithMessage("Passwords do not match");
    }
}
