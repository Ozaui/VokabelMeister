using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class ConfirmAccountDeletionCommandValidator : AbstractValidator<ConfirmAccountDeletionCommand>
{
    public ConfirmAccountDeletionCommandValidator()
    {
        RuleFor(x => x.OtpCode).ValidOtpCode();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.").WithErrorCode("PASSWORD_REQUIRED");
    }
}
