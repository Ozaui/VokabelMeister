using FluentValidation;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class DeleteWordCommandValidator : AbstractValidator<DeleteWordCommand>
{
    public DeleteWordCommandValidator()
    {
        RuleFor(x => x.WordConceptId).GreaterThan(0).WithErrorCode("WORD_CONCEPT_ID_INVALID");
    }
}
