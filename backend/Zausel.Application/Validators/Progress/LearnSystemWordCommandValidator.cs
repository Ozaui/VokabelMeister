using FluentValidation;
using Zausel.Application.Features.Progress;

namespace Zausel.Application.Validators.Progress;

public class LearnSystemWordCommandValidator : AbstractValidator<LearnSystemWordCommand>
{
    public LearnSystemWordCommandValidator()
    {
        RuleFor(x => x.WordId).GreaterThan(0).WithErrorCode("WORD_ID_INVALID");
    }
}
