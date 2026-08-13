using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        // UserId doğrulanmaz — [Authorize] arkasından JWT'den gelir, zaten geçerli bir int.
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required").WithErrorCode("REFRESH_TOKEN_REQUIRED");
    }
}
