using FluentValidation;
using Zausel.Application.Features.Auth;

namespace Zausel.Application.Validators.Auth;

public class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Social login token is required").WithErrorCode("SOCIAL_TOKEN_REQUIRED");
    }
}
