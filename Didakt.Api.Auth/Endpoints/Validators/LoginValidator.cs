using Didakt.Api.Auth.Endpoints.Requests;
using FluentValidation;

namespace Didakt.Api.Auth.Endpoints.Validators;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}