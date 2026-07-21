// ─────────────────────────────────────────────────────────────────────────────
// UpdateWordCommandValidator.cs
//
// AMAÇ: UpdateWordCommand'ın alan doğrulaması — CreateWordCommandValidator ile
//       birebir aynı temel kurallar + translation/grammar doğrulaması.
// NEDEN: İki validator `WordTranslationRules.ValidateTranslations` yardımcısını
//        (CreateWordCommandValidator.cs'te tanımlı) PAYLAŞIR — translation listesi
//        doğrulama mantığı tekrarlanmaz.
// BAĞIMLILIKLAR: FluentValidation, WordGrammarValidator, WordTranslationRules.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class UpdateWordCommandValidator : AbstractValidator<UpdateWordCommand>
{
    public UpdateWordCommandValidator(IValidator<WordGrammarInput> grammarValidator)
    {
        RuleFor(x => x.PartOfSpeech)
            .NotEmpty()
            .WithMessage("PartOfSpeech must not be empty.")
            .WithErrorCode("PART_OF_SPEECH_REQUIRED");

        RuleFor(x => x.DifficultyLevel)
            .NotEmpty()
            .WithMessage("DifficultyLevel must not be empty.")
            .WithErrorCode("DIFFICULTY_LEVEL_REQUIRED");

        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage("At least one translation is required.")
            .WithErrorCode("TRANSLATIONS_REQUIRED");

        RuleFor(x => x)
            .Custom(
                (command, context) =>
                    WordTranslationRules.ValidateTranslations(
                        command.Translations,
                        command.PartOfSpeech,
                        grammarValidator,
                        context
                    )
            );
    }
}
