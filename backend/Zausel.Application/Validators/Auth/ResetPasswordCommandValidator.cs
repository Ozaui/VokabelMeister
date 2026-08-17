using FluentValidation;
using Zausel.Application.Features.Auth;

namespace Zausel.Application.Validators.Auth;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required").WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithMessage("Email is not a valid address").WithErrorCode("EMAIL_INVALID");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required").WithErrorCode("OTP_CODE_REQUIRED")
            .Matches(@"^\d{6}$").WithMessage("OTP code must be 6 digits").WithErrorCode("OTP_CODE_INVALID_FORMAT");

        // RegisterCommandValidator'daki 5 şifre gücü kuralıyla BİREBİR aynı.
        RuleFor(x => x.NewPassword)
            .MinimumLength(12).WithMessage("Password must be at least 12 characters").WithErrorCode("PASSWORD_TOO_SHORT")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least 1 uppercase letter").WithErrorCode("PASSWORD_MISSING_UPPERCASE")
            .Matches(@"[a-z]").WithMessage("Password must contain at least 1 lowercase letter").WithErrorCode("PASSWORD_MISSING_LOWERCASE")
            .Matches(@"[0-9]").WithMessage("Password must contain at least 1 digit").WithErrorCode("PASSWORD_MISSING_DIGIT")
            .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least 1 special character").WithErrorCode("PASSWORD_MISSING_SPECIAL_CHAR");
    }
}
