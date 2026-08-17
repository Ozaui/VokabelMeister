using FluentValidation;
using Zausel.Application.Features.Words;

namespace Zausel.Application.Validators.Words;

public class GetUnmatchedWordConceptsQueryValidator : AbstractValidator<GetUnmatchedWordConceptsQuery>
{
    public GetUnmatchedWordConceptsQueryValidator()
    {
        RuleFor(x => x.LanguageId).GreaterThan(0).WithErrorCode("LANGUAGE_ID_INVALID");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("PAGE_INVALID");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("PAGE_SIZE_INVALID");
    }
}
