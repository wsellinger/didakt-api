using Didakt.Api.Leaderboard.Endpoints.Requests;
using FluentValidation;

namespace Didakt.Api.Leaderboard.Endpoints.Validators;

public class GetScoreValidator : AbstractValidator<GetScoreRequest>
{
    public GetScoreValidator()
    {
        RuleFor(x => x.Player)
            .NotEmpty();
    }
}