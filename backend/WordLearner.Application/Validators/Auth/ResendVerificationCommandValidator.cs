using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required").WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithMessage("Email is not a valid address").WithErrorCode("EMAIL_INVALID");
    }
}
