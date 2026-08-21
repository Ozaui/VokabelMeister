using FluentValidation;
using Zausel.Application.Features.Progress;

namespace Zausel.Application.Validators.Progress;

public class ApplyWordLeechActionCommandValidator : AbstractValidator<ApplyWordLeechActionCommand>
{
    public ApplyWordLeechActionCommandValidator()
    {
        RuleFor(x => x.Action).Must(a => a is "Suspend" or "Reset" or "Continue").WithErrorCode("LEECH_ACTION_INVALID");
    }
}
