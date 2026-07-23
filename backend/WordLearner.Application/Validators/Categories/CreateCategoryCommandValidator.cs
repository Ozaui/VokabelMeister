// ─────────────────────────────────────────────────────────────────────────────
// CreateCategoryCommandValidator.cs
//
// AMAÇ: CreateCategoryCommand ve UpdateCategoryCommand'ın alan doğrulaması —
//       CreateWordCommandValidator/UpdateWordCommandValidator (A-05) ile BİREBİR
//       aynı desen: her translation için LanguageCode + Name zorunlu, paylaşılan
//       döngü `CategoryTranslationRules.ValidateTranslations` içinde.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using FluentValidation.Results;
using WordLearner.Application.Features.Categories;

namespace WordLearner.Application.Validators.Categories;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage("At least one translation is required.")
            .WithErrorCode("CATEGORY_TRANSLATIONS_REQUIRED");

        RuleFor(x => x).Custom((command, context) => CategoryTranslationRules.ValidateTranslations(command.Translations, context));
    }
}

// AMAÇ: CreateCategoryCommandValidator ve UpdateCategoryCommandValidator'ın
//       PAYLAŞTIĞI translation doğrulama döngüsü.
internal static class CategoryTranslationRules
{
    public static void ValidateTranslations<T>(
        IReadOnlyList<CategoryTranslationInput> translations,
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

            if (string.IsNullOrWhiteSpace(translation.Name))
                context.AddFailure(Failure("CATEGORY_NAME_REQUIRED"));
        }
    }

    private static ValidationFailure Failure(string code) =>
        new("Translations", $"Translation validation failed: {code}.") { ErrorCode = code };
}
