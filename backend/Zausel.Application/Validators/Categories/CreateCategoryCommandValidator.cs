using FluentValidation;
using Zausel.Application.Features.Categories;

namespace Zausel.Application.Validators.Categories;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private static readonly string[] ValidLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
    private static readonly string[] SupportedLanguageCodes = ["de", "tr"];

    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0).WithErrorCode("CATEGORY_DISPLAY_ORDER_INVALID");
        RuleFor(x => x.MinLevel).Must(v => v is null || ValidLevels.Contains(v)).WithErrorCode("DIFFICULTY_LEVEL_INVALID");
        RuleFor(x => x.MaxLevel).Must(v => v is null || ValidLevels.Contains(v)).WithErrorCode("DIFFICULTY_LEVEL_INVALID");

        RuleFor(x => x.Translations).NotEmpty().WithErrorCode("CATEGORY_TRANSLATIONS_REQUIRED");
        RuleFor(x => x.Translations)
            .Must(t => t.Select(x => x.LanguageCode).Distinct().Count() == t.Count)
            .WithErrorCode("CATEGORY_TRANSLATIONS_LANGUAGE_DUPLICATE");

        RuleForEach(x => x.Translations).ChildRules(translation =>
        {
            translation.RuleFor(t => t.LanguageCode).Must(SupportedLanguageCodes.Contains).WithErrorCode("LANGUAGE_CODE_UNSUPPORTED");
            translation.RuleFor(t => t.Name).NotEmpty().WithErrorCode("CATEGORY_NAME_REQUIRED");
        });
    }
}
