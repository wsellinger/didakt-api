using Didakt.Api.Auth.Endpoints.Requests;
using FluentValidation;

namespace Didakt.Api.Auth.Endpoints.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}