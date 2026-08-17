using FluentValidation;
using Zausel.Application.Features.QrLogin;

namespace Zausel.Application.Validators.QrLogin;

public class GetQrLoginStatusQueryValidator : AbstractValidator<GetQrLoginStatusQuery>
{
    public GetQrLoginStatusQueryValidator()
    {
        RuleFor(x => x.QrToken)
            .NotEmpty().WithMessage("QR token is required").WithErrorCode("QR_TOKEN_REQUIRED");
    }
}
