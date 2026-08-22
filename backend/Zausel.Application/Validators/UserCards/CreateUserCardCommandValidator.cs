using FluentValidation;
using Zausel.Application.Features.UserCards;

namespace Zausel.Application.Validators.UserCards;

public class CreateUserCardCommandValidator : AbstractValidator<CreateUserCardCommand>
{
    public CreateUserCardCommandValidator()
    {
        RuleFor(x => x.FrontText).NotEmpty().WithErrorCode("USER_CARD_FRONT_TEXT_REQUIRED");
        RuleFor(x => x.FrontText).MaximumLength(500).WithErrorCode("USER_CARD_FRONT_TEXT_TOO_LONG");
        RuleFor(x => x.BackText).NotEmpty().WithErrorCode("USER_CARD_BACK_TEXT_REQUIRED");
        RuleFor(x => x.BackText).MaximumLength(500).WithErrorCode("USER_CARD_BACK_TEXT_TOO_LONG");
        RuleFor(x => x.Notes).MaximumLength(2000).WithErrorCode("USER_CARD_NOTES_TOO_LONG");

        RuleForEach(x => x.Examples).ChildRules(example =>
        {
            example.RuleFor(e => e.SentenceFront).NotEmpty().WithErrorCode("USER_CARD_EXAMPLE_SENTENCE_FRONT_REQUIRED");
            example.RuleFor(e => e.SentenceBack).NotEmpty().WithErrorCode("USER_CARD_EXAMPLE_SENTENCE_BACK_REQUIRED");
        });
    }
}
