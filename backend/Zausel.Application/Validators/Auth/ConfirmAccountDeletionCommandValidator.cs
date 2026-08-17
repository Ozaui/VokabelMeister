using FluentValidation;
using Zausel.Application.Features.Auth;

namespace Zausel.Application.Validators.Auth;

public class ConfirmAccountDeletionCommandValidator : AbstractValidator<ConfirmAccountDeletionCommand>
{
    public ConfirmAccountDeletionCommandValidator()
    {
        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required").WithErrorCode("OTP_CODE_REQUIRED")
            .Matches(@"^\d{6}$").WithMessage("OTP code must be 6 digits").WithErrorCode("OTP_CODE_INVALID_FORMAT");
    }
}
