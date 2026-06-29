using Didakt.Api.Leaderboard.Endpoints.Requests;
using FluentValidation;

namespace Didakt.Api.Leaderboard.Endpoints.Validators;

public class GetTopValidator : AbstractValidator<GetTopRequest>
{
    public GetTopValidator()
    {
        RuleFor(x => x.Count)
            .NotEmpty()
            .GreaterThan(0);
    }
}