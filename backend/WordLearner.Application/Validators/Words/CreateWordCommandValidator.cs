using FluentValidation;
using FluentValidation.Results;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class CreateWordCommandValidator : AbstractValidator<CreateWordCommand>
{
    public CreateWordCommandValidator(IValidator<WordGrammarInput> grammarValidator)
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

// CreateWordCommandValidator ve UpdateWordCommandValidator'ın paylaştığı translation doğrulaması.
internal static class WordTranslationRules
{
    public static void ValidateTranslations<T>(
        IReadOnlyList<WordTranslationInput> translations,
        string partOfSpeech,
        IValidator<WordGrammarInput> grammarValidator,
        ValidationContext<T> context
    )
    {
        foreach (var translation in translations)
        {
            if (string.IsNullOrWhiteSpace(translation.LanguageCode))
            {
                context.AddFailure(Failure("LANGUAGE_CODE_REQUIRED"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(translation.Text))
            {
                context.AddFailure(Failure("WORD_TEXT_REQUIRED"));
                continue;
            }

            var grammarDataJson = translation.WordDetail?.GrammarData?.GetRawText();
            var grammarResult = grammarValidator.Validate(
                new WordGrammarInput(translation.LanguageCode, partOfSpeech, grammarDataJson)
            );

            foreach (var failure in grammarResult.Errors)
                context.AddFailure(failure);
        }
    }

    private static ValidationFailure Failure(string code) =>
        new("Translations", $"Translation validation failed: {code}.") { ErrorCode = code };
}
