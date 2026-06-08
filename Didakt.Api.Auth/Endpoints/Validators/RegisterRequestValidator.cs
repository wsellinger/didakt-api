using Didakt.Api.Auth.Models.Requests;
using FluentValidation;

namespace Didakt.Api.Auth.Endpoints.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .Length(3, 20)
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens.")
            .Matches("^[^_-].*[^_-]$").WithMessage("Username cannot start or end with an underscore or hyphen.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(12, 128);
    }
}