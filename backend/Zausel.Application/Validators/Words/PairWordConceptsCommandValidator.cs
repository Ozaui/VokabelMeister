using FluentValidation;
using Zausel.Application.Features.Words;

namespace Zausel.Application.Validators.Words;

public class PairWordConceptsCommandValidator : AbstractValidator<PairWordConceptsCommand>
{
    public PairWordConceptsCommandValidator()
    {
        RuleFor(x => x.PrimaryId).GreaterThan(0).WithErrorCode("WORD_CONCEPT_ID_INVALID");
        RuleFor(x => x.OtherConceptId)
            .GreaterThan(0).WithErrorCode("WORD_CONCEPT_ID_INVALID")
            .Must((command, otherConceptId) => otherConceptId != command.PrimaryId).WithErrorCode("SAME_CONCEPT_PAIR_NOT_ALLOWED");
    }
}
