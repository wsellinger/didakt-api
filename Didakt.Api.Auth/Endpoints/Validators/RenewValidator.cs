using Didakt.Api.Auth.Endpoints.Requests;
using FluentValidation;

namespace Didakt.Api.Auth.Endpoints.Validators
{
    public class RenewValidator : AbstractValidator<RenewRequest>
    {
        public RenewValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}