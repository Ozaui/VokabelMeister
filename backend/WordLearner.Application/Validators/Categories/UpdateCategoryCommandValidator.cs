using FluentValidation;
using WordLearner.Application.Features.Categories;

namespace WordLearner.Application.Validators.Categories;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Translations)
            .NotEmpty()
            .WithMessage("At least one translation is required.")
            .WithErrorCode("CATEGORY_TRANSLATIONS_REQUIRED");

        RuleFor(x => x).Custom((command, context) => CategoryTranslationRules.ValidateTranslations(command.Translations, context));
    }
}
