using FluentValidation;
using WordLearner.Application.Features.QrLogin;

namespace WordLearner.Application.Validators.QrLogin;

public class ConfirmQrLoginCommandValidator : AbstractValidator<ConfirmQrLoginCommand>
{
    public ConfirmQrLoginCommandValidator()
    {
        RuleFor(x => x.QrToken)
            .NotEmpty().WithMessage("QR token is required").WithErrorCode("QR_TOKEN_REQUIRED");
    }
}
