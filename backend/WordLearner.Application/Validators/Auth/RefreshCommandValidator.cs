using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required").WithErrorCode("REFRESH_TOKEN_REQUIRED");
    }
}
