using FluentValidation;
using Zausel.Application.Features.UserCards;

namespace Zausel.Application.Validators.UserCards;

public class DeleteUserCardCommandValidator : AbstractValidator<DeleteUserCardCommand>
{
    public DeleteUserCardCommandValidator()
    {
        RuleFor(x => x.UserCardId).GreaterThan(0).WithErrorCode("USER_CARD_ID_INVALID");
    }
}
