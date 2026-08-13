using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required").WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithMessage("Email is not a valid address").WithErrorCode("EMAIL_INVALID");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required").WithErrorCode("OTP_CODE_REQUIRED")
            .Matches(@"^\d{6}$").WithMessage("OTP code must be 6 digits").WithErrorCode("OTP_CODE_INVALID_FORMAT");
    }
}
