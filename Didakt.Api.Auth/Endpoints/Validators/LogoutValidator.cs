using Didakt.Api.Auth.Endpoints.Requests;
using FluentValidation;

namespace Didakt.Api.Auth.Endpoints.Validators
{
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken).
                NotEmpty();
        }
    }
}