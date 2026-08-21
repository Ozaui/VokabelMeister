using FluentValidation;
using Zausel.Application.Features.UserCategories;

namespace Zausel.Application.Validators.UserCategories;

public class CreateUserCategoryCommandValidator : AbstractValidator<CreateUserCategoryCommand>
{
    public CreateUserCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithErrorCode("USER_CATEGORY_NAME_REQUIRED");
        RuleFor(x => x.Name).MaximumLength(100).WithErrorCode("USER_CATEGORY_NAME_TOO_LONG");
        RuleFor(x => x.Description).MaximumLength(500).WithErrorCode("USER_CATEGORY_DESCRIPTION_TOO_LONG");
        RuleFor(x => x.Color).MaximumLength(10).WithErrorCode("USER_CATEGORY_COLOR_TOO_LONG");
        RuleFor(x => x.Icon).MaximumLength(100).WithErrorCode("USER_CATEGORY_ICON_TOO_LONG");
    }
}
