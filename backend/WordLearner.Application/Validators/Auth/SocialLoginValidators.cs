using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().WithMessage("Token must not be empty.").WithErrorCode("TOKEN_REQUIRED");
    }
}

public class LoginWithAppleCommandValidator : AbstractValidator<LoginWithAppleCommand>
{
    public LoginWithAppleCommandValidator()
    {
        RuleFor(x => x.IdentityToken)
            .NotEmpty()
            .WithMessage("Token must not be empty.")
            .WithErrorCode("TOKEN_REQUIRED");
    }
}
