using FluentValidation;
using Zausel.Application.Features.UserCards;

namespace Zausel.Application.Validators.UserCards;

public class GetUserCardsQueryValidator : AbstractValidator<GetUserCardsQuery>
{
    public GetUserCardsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("PAGE_INVALID");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("PAGE_SIZE_INVALID");
    }
}
