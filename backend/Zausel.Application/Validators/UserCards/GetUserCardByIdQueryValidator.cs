using FluentValidation;
using Zausel.Application.Features.UserCards;

namespace Zausel.Application.Validators.UserCards;

public class GetUserCardByIdQueryValidator : AbstractValidator<GetUserCardByIdQuery>
{
    public GetUserCardByIdQueryValidator()
    {
        RuleFor(x => x.UserCardId).GreaterThan(0).WithErrorCode("USER_CARD_ID_INVALID");
    }
}
