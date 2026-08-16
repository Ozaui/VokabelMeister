using FluentValidation;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class GetWordByIdQueryValidator : AbstractValidator<GetWordByIdQuery>
{
    public GetWordByIdQueryValidator()
    {
        RuleFor(x => x.WordConceptId).GreaterThan(0).WithErrorCode("WORD_CONCEPT_ID_INVALID");
    }
}
