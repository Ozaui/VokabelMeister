using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class LoginWithAppleCommandValidator : AbstractValidator<LoginWithAppleCommand>
{
    public LoginWithAppleCommandValidator()
    {
        // FirstName/LastName BİLEREK doğrulanmıyor — Apple bu alanları yalnızca ilk izin anında
        // gönderir (LoginWithAppleCommand'ın kendi yorumu), sonraki girişlerde null gelmesi NORMAL.
        RuleFor(x => x.IdentityToken)
            .NotEmpty().WithMessage("Social login token is required").WithErrorCode("SOCIAL_TOKEN_REQUIRED");
    }
}
