using FluentValidation;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Features.Words;
using WordLearner.Domain.Enums.Content;

namespace WordLearner.Application.Validators.Words;

// CreateWordCommandValidator ile AYNI kural kümesi (+WordConceptId) — FluentValidation'ın
// AbstractValidator<T> tek bir T'ye bağlı olması yüzünden (Create'de WordConceptId yok, farklı
// tip) tek bir ortak validator sınıfı PAYLAŞILAMIYOR, kurallar tekrar yazılıyor.
public class UpdateWordCommandValidator : AbstractValidator<UpdateWordCommand>
{
    private static readonly string[] ValidLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
    private static readonly string[] SupportedLanguageCodes = ["de", "tr"];
    private static readonly string[] ValidExampleTypes = ["Normal", "Idiom", "Formal", "Colloquial"];

    public UpdateWordCommandValidator()
    {
        RuleFor(x => x.WordConceptId).GreaterThan(0).WithErrorCode("WORD_CONCEPT_ID_INVALID");
        RuleFor(x => x.PartOfSpeech).Must(p => Enum.TryParse<PartOfSpeech>(p, out _)).WithErrorCode("PART_OF_SPEECH_INVALID");
        RuleFor(x => x.DifficultyLevel).Must(ValidLevels.Contains).WithErrorCode("DIFFICULTY_LEVEL_INVALID");

        RuleFor(x => x.Translations).NotEmpty().WithErrorCode("TRANSLATIONS_REQUIRED");
        RuleFor(x => x.Translations).Must(t => t.Count <= 2).WithErrorCode("TRANSLATIONS_COUNT_INVALID");
        RuleFor(x => x.Translations)
            .Must(t => t.Select(x => x.LanguageCode).Distinct().Count() == t.Count)
            .WithErrorCode("TRANSLATIONS_LANGUAGE_DUPLICATE");

        RuleForEach(x => x.Translations).ChildRules(translation =>
        {
            translation.RuleFor(t => t.LanguageCode).Must(SupportedLanguageCodes.Contains).WithErrorCode("LANGUAGE_CODE_UNSUPPORTED");
            translation.RuleForEach(t => t.Examples).ChildRules(example =>
            {
                example.RuleFor(e => e.SentenceText).NotEmpty().WithErrorCode("EXAMPLE_SENTENCE_TEXT_REQUIRED");
                example.RuleFor(e => e.Level).Must(ValidLevels.Contains).WithErrorCode("EXAMPLE_LEVEL_INVALID");
                example.RuleFor(e => e.ExampleType).Must(v => v is null || ValidExampleTypes.Contains(v)).WithErrorCode("EXAMPLE_TYPE_INVALID");
            });
        });

        RuleFor(x => x).Custom(ValidateGrammar);
    }

    private static void ValidateGrammar(UpdateWordCommand command, ValidationContext<UpdateWordCommand> context)
    {
        if (!Enum.TryParse<PartOfSpeech>(command.PartOfSpeech, out var partOfSpeech))
            return;

        foreach (var translation in command.Translations)
        {
            if (!SupportedLanguageCodes.Contains(translation.LanguageCode))
                continue;

            var grammarValidator = new WordGrammarValidator(translation.LanguageCode, partOfSpeech);
            var input = new WordGrammarInput(translation.Text, translation.Definition, translation.WordDetail?.GrammarData);
            var result = grammarValidator.Validate(input);
            foreach (var failure in result.Errors)
                context.AddFailure(failure);
        }
    }
}
