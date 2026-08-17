using FluentValidation;
using Zausel.Application.Features.QrLogin;

namespace Zausel.Application.Validators.QrLogin;

public class DenyQrLoginCommandValidator : AbstractValidator<DenyQrLoginCommand>
{
    public DenyQrLoginCommandValidator()
    {
        RuleFor(x => x.QrToken)
            .NotEmpty().WithMessage("QR token is required").WithErrorCode("QR_TOKEN_REQUIRED");
    }
}
