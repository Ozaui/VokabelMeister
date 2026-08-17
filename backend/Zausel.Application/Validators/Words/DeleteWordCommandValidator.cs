using FluentValidation;
using Zausel.Application.Features.Words;

namespace Zausel.Application.Validators.Words;

public class DeleteWordCommandValidator : AbstractValidator<DeleteWordCommand>
{
    public DeleteWordCommandValidator()
    {
        RuleFor(x => x.WordConceptId).GreaterThan(0).WithErrorCode("WORD_CONCEPT_ID_INVALID");
    }
}
