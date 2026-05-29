using API.Filters;
using DTO.Auth.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace API.Containers.Extensions;

/// <summary>
///     Registers FluentValidation auto-validation, scans the DTO assembly for all
///     <c>AbstractValidator&lt;T&gt;</c> implementations, and registers
///     <see cref="API.Filters.ModelValidatorActionFilter" /> as a scoped service.
/// </summary>
public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<ResetPasswordDtoValidator>();

        services.AddScoped<ModelValidatorActionFilter>();

        return services;
    }
}