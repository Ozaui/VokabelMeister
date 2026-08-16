using FluentValidation;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class GetWordsQueryValidator : AbstractValidator<GetWordsQuery>
{
    public GetWordsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("PAGE_INVALID");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("PAGE_SIZE_INVALID");
    }
}
